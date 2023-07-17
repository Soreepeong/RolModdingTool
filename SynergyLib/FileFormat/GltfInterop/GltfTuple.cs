using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.DirectDrawSurface;
using SynergyLib.FileFormat.GltfInterop.Models;
using SynergyLib.Util;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.GltfInterop;

public class GltfTuple {
    public const uint GlbMagic = 0x46546C67;
    public const uint GlbJsonMagic = 0x4E4F534A;
    public const uint GlbDataMagic = 0x004E4942;

    public readonly GltfRoot Root;
    public readonly List<MemoryStream> DataStreams = new();

    private int _defaultDataStreamIndex = -1;

    public GltfTuple() : this(new()) {
        Root.Scene = Root.Scenes.AddAndGetIndex(new());
    }

    public GltfTuple(GltfRoot root) {
        Root = root;
    }

    public static GltfTuple FromStream(Stream glbStream, bool leaveOpen = false) {
        using var lbr = new NativeReader(glbStream, Encoding.UTF8, leaveOpen);
        if (lbr.ReadUInt32() != GlbMagic)
            throw new InvalidDataException("Not a glb file.");
        if (lbr.ReadInt32() != 2)
            throw new InvalidDataException("Currently a glb file may only have exactly 2 entries.");
        if (glbStream.Length < lbr.ReadInt32())
            throw new InvalidDataException("File is truncated.");

        var jsonLength = lbr.ReadInt32();
        if (lbr.ReadUInt32() != GlbJsonMagic)
            throw new InvalidDataException("First entry must be a JSON file.");

        var root = JsonConvert.DeserializeObject<GltfRoot>(lbr.ReadFString(jsonLength, Encoding.UTF8))
            ?? throw new InvalidDataException("JSON was empty.");

        var dataLength = lbr.ReadInt32();
        if (lbr.ReadUInt32() != GlbDataMagic)
            throw new InvalidDataException("Second entry must be a data file.");

        var res = new GltfTuple(root);
        var ms = new MemoryStream();
        ms.SetLength(dataLength);
        glbStream.ReadExactly(ms.GetBuffer().AsSpan(0, dataLength));
        res.DataStreams.Add(ms);
        return res;
    }

    public void Compile(Stream target) {
        var oldBufferViews = Root.BufferViews;
        var oldBuffers = Root.Buffers;
        try {
            Root.BufferViews = new();

            var bufferOffset = 0L;
            foreach (var bv in oldBufferViews) {
                bufferOffset = (bufferOffset + 3) / 4 * 4;
                Root.BufferViews.Add(
                    new() {
                        Name = bv.Name,
                        Buffer = 0,
                        ByteOffset = bufferOffset,
                        ByteLength = bv.ByteLength,
                        Target = bv.Target,
                    });
                bufferOffset += bv.ByteLength;
            }

            Root.Buffers = new() {new() {ByteLength = bufferOffset}};

            var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Root));
            if (json.Length % 4 != 0) {
                var rv = new byte[(json.Length + 3) / 4 * 4];
                Buffer.BlockCopy(json, 0, rv, 0, json.Length);
                for (var i = json.Length; i < rv.Length; i++)
                    rv[i] = 0x20; // space
                json = rv;
            }

            using var writer = new NativeWriter(target, Encoding.UTF8, true) {IsBigEndian = false};
            writer.Write(GlbMagic);
            writer.Write(2);
            writer.Write(checked(12 + 8 + json.Length + 8 + (int) bufferOffset));
            writer.Write(json.Length);
            writer.Write(GlbJsonMagic);
            writer.Write(json);

            writer.Write(checked((int) bufferOffset));
            writer.Write(GlbDataMagic);

            bufferOffset = 0L;
            Span<byte> zeroes = stackalloc byte[4];
            foreach (var bv in oldBufferViews) {
                var alignment = (4 - (int) bufferOffset % 4) % 4;
                bufferOffset += alignment;
                target.Write(zeroes[..alignment]);
                target.Write(DataStreams[bv.Buffer].GetBuffer(), (int) bv.ByteOffset, (int) bv.ByteLength);
                Root.BufferViews.Add(
                    new() {
                        Name = bv.Name,
                        Buffer = 0,
                        ByteOffset = bufferOffset,
                        ByteLength = bv.ByteLength,
                        Target = bv.Target,
                    });
                bufferOffset += bv.ByteLength;
            }
        } finally {
            Root.BufferViews = oldBufferViews;
            Root.Buffers = oldBuffers;
        }
    }

    public IEnumerable<Tuple<string, MemoryStream>> CompileToFiles(string gltfName) {
        var oldBuffers = Root.Buffers;
        try {
            Root.Buffers = oldBuffers.Select(
                (x, i) => new GltfBuffer {
                    ByteLength = x.ByteLength,
                    Uri = x.Uri ?? (i == 0 ? $"{gltfName}.bin" : $"{gltfName}.bin{i}")
                }).ToList();
            yield return Tuple.Create(
                gltfName + ".gltf",
                new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Root))));
        } finally {
            Root.Buffers = oldBuffers;
        }

        for (var i = 0; i < Root.Buffers.Count; i++) {
            DataStreams[i].Position = 0;
            yield return Tuple.Create(
                Root.Buffers[i].Uri ?? (i == 0 ? $"{gltfName}.bin" : $"{gltfName}.bin{i}"),
                DataStreams[i]);
        }
    }

    private unsafe int AddBufferView<T>(
        string? uri,
        string? baseName,
        GltfBufferViewTarget? target,
        ReadOnlySpan<T> data)
        where T : unmanaged {
        if (Root.Buffers.Count != DataStreams.Count)
            throw new InvalidOperationException("len(Buffers) != len(DataStreams)");

        var dataStreamIndex = _defaultDataStreamIndex;
        if (dataStreamIndex == -1 && uri is null) {
            dataStreamIndex = _defaultDataStreamIndex = DataStreams.AddAndGetIndex(new());
            Root.Buffers.Add(new());
        } else if (uri is not null) {
            dataStreamIndex = DataStreams.AddAndGetIndex(new());
            Root.Buffers.Add(new() {Uri = uri});
        }

        var targetStream = DataStreams[dataStreamIndex];

        var byteLength = sizeof(T) * data.Length;
        var index = Root.BufferViews.AddAndGetIndex(
            new() {
                Name = baseName is null ? null : $"{baseName}/bufferView",
                Buffer = dataStreamIndex,
                ByteOffset = checked((int) (targetStream.Position = (targetStream.Length + 3) / 4 * 4)),
                ByteLength = byteLength,
                Target = target,
            });

        fixed (void* src = data)
            targetStream.Write(new((byte*) src, byteLength));

        Root.Buffers[dataStreamIndex].ByteLength = targetStream.Length;

        return index;
    }

    public int AddAccessor<T>(
        string? baseName,
        Span<T> data,
        int start = 0,
        int count = int.MaxValue,
        int? bufferView = null,
        GltfBufferViewTarget? target = null)
        where T : unmanaged => AddAccessor(baseName, (ReadOnlySpan<T>) data, start, count, bufferView, target);

    public unsafe int AddAccessor<T>(
        string? baseName,
        ReadOnlySpan<T> data,
        int start = 0,
        int count = int.MaxValue,
        int? bufferView = null,
        GltfBufferViewTarget? target = null)
        where T : unmanaged {
        bufferView ??= AddBufferView(null, baseName, target, data);

        {
            var (componentType, type) = typeof(T) switch {
                var t when t == typeof(byte) => (GltfAccessorComponentTypes.u8, GltfAccessorTypes.Scalar),
                var t when t == typeof(sbyte) => (GltfAccessorComponentTypes.s8, GltfAccessorTypes.Scalar),
                var t when t == typeof(ushort) => (GltfAccessorComponentTypes.u16, GltfAccessorTypes.Scalar),
                var t when t == typeof(short) => (GltfAccessorComponentTypes.s16, GltfAccessorTypes.Scalar),
                var t when t == typeof(uint) => (GltfAccessorComponentTypes.u32, GltfAccessorTypes.Scalar),
                var t when t == typeof(float) => (GltfAccessorComponentTypes.f32, GltfAccessorTypes.Scalar),
                var t when t == typeof(Quaternion) => (GltfAccessorComponentTypes.f32, GltfAccessorTypes.Vec4),
                var t when t == typeof(Vector2) => (GltfAccessorComponentTypes.f32, GltfAccessorTypes.Vec2),
                var t when t == typeof(Vector3) => (GltfAccessorComponentTypes.f32, GltfAccessorTypes.Vec3),
                var t when t == typeof(Vector4) => (GltfAccessorComponentTypes.f32, GltfAccessorTypes.Vec4),
                var t when t == typeof(Matrix4x4) => (GltfAccessorComponentTypes.f32, GltfAccessorTypes.Mat4),
                var t when t == typeof(Vector4<byte>) => (GltfAccessorComponentTypes.u8, GltfAccessorTypes.Vec4),
                var t when t == typeof(Vector4<sbyte>) => (GltfAccessorComponentTypes.s8, GltfAccessorTypes.Vec4),
                var t when t == typeof(Vector4<ushort>) => (GltfAccessorComponentTypes.u16, GltfAccessorTypes.Vec4),
                var t when t == typeof(Vector4<short>) => (GltfAccessorComponentTypes.s16, GltfAccessorTypes.Vec4),
                var t when t == typeof(Vector4<uint>) => (GltfAccessorComponentTypes.u32, GltfAccessorTypes.Vec4),
                var t when t == typeof(Vector4<float>) => (GltfAccessorComponentTypes.f32, GltfAccessorTypes.Vec4),
                _ => throw new NotSupportedException(),
            };

            var componentCount = type switch {
                GltfAccessorTypes.Scalar => 1,
                GltfAccessorTypes.Vec2 => 2,
                GltfAccessorTypes.Vec3 => 3,
                GltfAccessorTypes.Vec4 => 4,
                GltfAccessorTypes.Mat2 => 4,
                GltfAccessorTypes.Mat3 => 9,
                GltfAccessorTypes.Mat4 => 16,
                _ => throw new NotSupportedException(),
            };

            if (count == int.MaxValue)
                count = data.Length - start;

            Tuple<float[], float[]> MinMax<TComponent>(ReadOnlySpan<T> data2)
                where TComponent : unmanaged, INumber<TComponent> {
                var mins = new float[componentCount];
                var maxs = new float[componentCount];
                fixed (void* pData = data2) {
                    var span = new ReadOnlySpan<TComponent>(
                        (TComponent*) ((T*) pData + start),
                        count * componentCount);
                    for (var i = 0; i < componentCount; i++) {
                        var min = span[i];
                        var max = span[i];
                        for (var j = 1; j < count; j++) {
                            var v = span[j * componentCount + i];
                            if (v < min)
                                min = v;
                            if (v > max)
                                max = v;
                        }

                        mins[i] = Convert.ToSingle(min);
                        maxs[i] = Convert.ToSingle(max);
                    }
                }

                return Tuple.Create(mins, maxs);
            }

            var accessor = new GltfAccessor {
                Name = baseName is null ? null : $"{baseName}/accessor[{start}..{start + count}]",
                ByteOffset = start * sizeof(T),
                BufferView = bufferView.Value,
                ComponentType = componentType,
                Count = count,
                Type = type,
                Min = count == 0 ? null : new float[componentCount],
                Max = count == 0 ? null : new float[componentCount],
            };

            (accessor.Min, accessor.Max) = componentType switch {
                GltfAccessorComponentTypes.s8 => MinMax<sbyte>(data),
                GltfAccessorComponentTypes.u8 => MinMax<byte>(data),
                GltfAccessorComponentTypes.s16 => MinMax<short>(data),
                GltfAccessorComponentTypes.u16 => MinMax<ushort>(data),
                GltfAccessorComponentTypes.u32 => MinMax<int>(data),
                GltfAccessorComponentTypes.f32 => MinMax<float>(data),
                _ => throw new NotSupportedException(),
            };

            return Root.Accessors.AddAndGetIndex(accessor);
        }
    }

    public int AddTexture<TPixel>(string name, Image<TPixel> image, PngColorType colorType, DdsFile? ddsFile = null)
        where TPixel : unmanaged, IPixel<TPixel> {
        for (var i = 0; i < Root.Textures.Count; i++)
            if (Root.Textures[i].Name == name)
                return i;

        using var pngStream = new MemoryStream();
        image.Save(pngStream, new PngEncoder {ColorType = colorType});

        if (ddsFile is not null)
            Root.ExtensionsUsed.Add("MSFT_texture_dds");

        var pngName = Path.GetFileNameWithoutExtension(name) + ".png";
        var ddsName = "dds/" + Path.GetFileNameWithoutExtension(name) + ".dds";
        return Root.Textures.AddAndGetIndex(
            new() {
                Name = name,
                Source = Root.Images.AddAndGetIndex(
                    new() {
                        Name = pngName,
                        MimeType = "image/png",
                        BufferView = AddBufferView(
                            pngName,
                            pngName,
                            null,
                            new ReadOnlySpan<byte>(pngStream.GetBuffer(), 0, (int) pngStream.Length)),
                    }),
                Extensions = ddsFile is null
                    ? null
                    : new() {
                        MsftTextureDds = new() {
                            Source = Root.Images.AddAndGetIndex(
                                new() {
                                    Name = ddsName,
                                    MimeType = "image/vnd-ms.dds",
                                    BufferView = AddBufferView(
                                        ddsName,
                                        ddsName,
                                        null,
                                        ddsFile.Data),
                                }),
                        },
                    },
            });
    }

    public byte[] ReadBufferView(int bufferViewIndex) {
        var bufferView = Root.BufferViews[bufferViewIndex];
        return DataStreams[bufferView.Buffer].GetBuffer()[(int) bufferView.ByteOffset .. (int) bufferView.ByteOffsetTo];
    }

    public unsafe T[] ReadTypedArray<T>(int accessorIndex) where T : unmanaged {
        var accessor = Root.Accessors[accessorIndex];
        var bufferView = Root.BufferViews[accessor.BufferView];

        var scalarSize = accessor.ComponentType switch {
            GltfAccessorComponentTypes.s8 => 1,
            GltfAccessorComponentTypes.u8 => 1,
            GltfAccessorComponentTypes.s16 => 2,
            GltfAccessorComponentTypes.u16 => 2,
            GltfAccessorComponentTypes.u32 => 4,
            GltfAccessorComponentTypes.f32 => 4,
            _ => throw new NotSupportedException(),
        };
        var scalarCount = accessor.Type switch {
            GltfAccessorTypes.Scalar => 1,
            GltfAccessorTypes.Vec2 => 2,
            GltfAccessorTypes.Vec3 => 3,
            GltfAccessorTypes.Vec4 => 4,
            GltfAccessorTypes.Mat2 => 4,
            GltfAccessorTypes.Mat3 => 9,
            GltfAccessorTypes.Mat4 => 16,
            _ => throw new NotSupportedException(),
        };
        var elementSize = scalarCount * scalarSize;
        if (elementSize != sizeof(T))
            throw new InvalidOperationException();

        var bytesSpan = DataStreams[bufferView.Buffer].GetBuffer().AsSpan(
            (int) bufferView.ByteOffset,
            (int) bufferView.ByteLength);
        bytesSpan = bytesSpan.Slice((int) accessor.ByteOffset, accessor.Count * elementSize);

        var res = new T[accessor.Count];
        fixed (void* p = res)
            bytesSpan.CopyTo(new(p, bytesSpan.Length));
        return res;
    }

    public float[] ReadSingleArray(int accessorIndex) => Root.Accessors[accessorIndex].ComponentType switch {
        GltfAccessorComponentTypes.f32 => ReadTypedArray<float>(accessorIndex),
        _ => throw new InvalidOperationException(),
    };

    public Vector2[] ReadVector2Array(int accessorIndex) => Root.Accessors[accessorIndex].ComponentType switch {
        GltfAccessorComponentTypes.f32 => ReadTypedArray<Vector2>(accessorIndex),
        _ => throw new InvalidOperationException(),
    };

    public Vector3[] ReadVector3Array(int accessorIndex) => Root.Accessors[accessorIndex].ComponentType switch {
        GltfAccessorComponentTypes.f32 => ReadTypedArray<Vector3>(accessorIndex),
        _ => throw new InvalidOperationException(),
    };

    public Vector4[] ReadVector4Array(int accessorIndex) => Root.Accessors[accessorIndex].ComponentType switch {
        GltfAccessorComponentTypes.f32 => ReadTypedArray<Vector4>(accessorIndex),
        _ => throw new InvalidOperationException(),
    };

    public Quaternion[] ReadQuaternionArray(int accessorIndex) => Root.Accessors[accessorIndex].ComponentType switch {
        GltfAccessorComponentTypes.f32 => ReadTypedArray<Quaternion>(accessorIndex),
        _ => throw new InvalidOperationException(),
    };

    public Vector4<float>[] ReadVector4SingleArray(int accessorIndex) =>
        Root.Accessors[accessorIndex].ComponentType switch {
            GltfAccessorComponentTypes.f32 => ReadTypedArray<Vector4<float>>(accessorIndex),
            _ => throw new InvalidOperationException(),
        };

    public Vector4<ushort>[] ReadVector4UInt16Array(int accessorIndex) =>
        Root.Accessors[accessorIndex].ComponentType switch {
            GltfAccessorComponentTypes.u8 => ReadTypedArray<Vector4<byte>>(accessorIndex)
                .Select(x => new Vector4<ushort>(x.Select(y => (ushort) y))).ToArray(),
            GltfAccessorComponentTypes.u16 => ReadTypedArray<Vector4<ushort>>(accessorIndex).ToArray(),
            GltfAccessorComponentTypes.u32 => ReadTypedArray<Vector4<uint>>(accessorIndex)
                .Select(x => new Vector4<ushort>(x.Select(y => checked((ushort) y)))).ToArray(),
            _ => throw new InvalidOperationException(),
        };

    public ushort[] ReadUInt16Array(int accessorIndex) => Root.Accessors[accessorIndex].ComponentType switch {
        GltfAccessorComponentTypes.u8 => ReadTypedArray<byte>(accessorIndex).Select(x => (ushort) x).ToArray(),
        GltfAccessorComponentTypes.u16 => ReadTypedArray<ushort>(accessorIndex).Select(x => x).ToArray(),
        GltfAccessorComponentTypes.u32 =>
            ReadTypedArray<uint>(accessorIndex).Select(x => checked((ushort) x)).ToArray(),
        _ => throw new InvalidOperationException(),
    };
}
