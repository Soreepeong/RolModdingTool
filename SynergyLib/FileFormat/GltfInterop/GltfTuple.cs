using System;
using System.IO;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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
    public readonly MemoryStream DataStream;

    public GltfTuple() {
        Root = new();
        DataStream = new();
        Root.Buffers.Add(new());
        Root.Scene = Root.Scenes.AddAndGetIndex(new());
    }

    public GltfTuple(Stream glbStream, bool leaveOpen = false) {
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

        Root = JsonConvert.DeserializeObject<GltfRoot>(lbr.ReadFString(jsonLength, Encoding.UTF8))
            ?? throw new InvalidDataException("JSON was empty.");

        var dataLength = lbr.ReadInt32();
        if (lbr.ReadUInt32() != GlbDataMagic)
            throw new InvalidDataException("Second entry must be a data file.");

        DataStream = new();
        DataStream.SetLength(dataLength);
        glbStream.ReadExactly(DataStream.GetBuffer().AsSpan(0, dataLength));
    }

    public ReadOnlySpan<byte> Data => DataStream.GetBuffer().AsSpan(0, (int) DataStream.Length);

    public void Compile(Stream target) {
        Root.Buffers[0].ByteLength = DataStream.Length;

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
        writer.Write(checked(12 + 8 + json.Length + 8 + (int) DataStream.Length));
        writer.Write(json.Length);
        writer.Write(GlbJsonMagic);
        writer.Write(json);

        writer.Write(checked((int) DataStream.Length));
        writer.Write(GlbDataMagic);
        DataStream.Position = 0;
        DataStream.CopyTo(target);
    }

    public unsafe int AddBufferView<T>(
        string? baseName,
        GltfBufferViewTarget? target,
        ReadOnlySpan<T> data)
        where T : unmanaged {
        var byteLength = sizeof(T) * data.Length;
        var index = Root.BufferViews.AddAndGetIndex(
            new() {
                Name = baseName is null ? null : $"{baseName}/bufferView",
                ByteOffset = checked((int) (DataStream.Position = (DataStream.Length + 3) / 4 * 4)),
                ByteLength = byteLength,
                Target = target,
            });

        fixed (void* src = data)
            DataStream.Write(new((byte*) src, byteLength));

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
        bufferView ??= AddBufferView(baseName, target, data);

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

    public void AddToScene(int nodeIndex) => Root.Scenes[Root.Scene].Nodes.Add(nodeIndex);

    public int AddTexture(string name, Image<Rgba32> image) {
        for (var i = 0; i < Root.Textures.Count; i++)
            if (Root.Textures[i].Name == name)
                return i;

        using var pngStream = new MemoryStream();
        image.Save(pngStream, new PngEncoder());

        return Root.Textures.AddAndGetIndex(
            new() {
                Name = name,
                Source = Root.Images.AddAndGetIndex(
                    new() {
                        Name = Path.ChangeExtension(name, ".png"),
                        MimeType = "image/png",
                        BufferView = AddBufferView(
                            name + ".png",
                            null,
                            new ReadOnlySpan<byte>(pngStream.GetBuffer(), 0, (int) pngStream.Length)),
                    }),
            });
    }
}
