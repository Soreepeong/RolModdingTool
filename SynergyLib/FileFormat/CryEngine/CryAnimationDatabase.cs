using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SynergyLib.FileFormat.CryEngine.CryAnimationDatabaseElements;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine;

public class CryAnimationDatabase {
    public readonly Dictionary<string, Animation> Animations = new();

    public void PasteFrom(Dictionary<string, Animation> animations, bool overwrite) {
        foreach (var (k, v) in animations) {
            if (overwrite || !Animations.ContainsKey(k))
                Animations[k] = v;
        }
    }

    public void PasteFrom(CryAnimationDatabase? adb, bool overwrite) {
        if (adb is not null)
            PasteFrom(adb.Animations, overwrite);
    }

    public void ApplyScaleTransformation(float scale) {
        foreach (var t in Animations.Values.SelectMany(x => x.Tracks.Values.Select(y => y.Position)).Distinct()) {
            if (t is null)
                continue;
            
            foreach (var i in Enumerable.Range(0, t.Data.Length))
                t.Data[i] *= scale;
        }
    }

    public void WriteTo(NativeWriter writer) {
        var chunks = new CryChunks {
            Type = CryFileType.Geometry, // sic
            Version = CryFileVersion.CryTek3
        };

        var pos = Animations.Values
            .SelectMany(x => x.Tracks.Values.Select(y => y.Position))
            .Distinct()
            .Where(x => x is not null)
            .Cast<ControllerKeyPosition>()
            .OrderBy(x => x.Format)
            .ToList();
        var posDict = pos
            .Select((x, i) => (x: x!, i))
            .ToDictionary(x => x.x, x => x.i);
        var rot = Animations.Values
            .SelectMany(x => x.Tracks.Values.Select(y => y.Rotation))
            .Distinct()
            .Where(x => x is not null)
            .OrderBy(x => x!.Format)
            .Cast<ControllerKeyRotation>()
            .ToList();
        var rotDict = rot
            .Select((x, i) => (x: x!, i))
            .ToDictionary(x => x.x, x => x.i);
        var time = Animations.Values
            .SelectMany(
                x => x.Tracks.Values.Select(y => y.PositionTime)
                    .Concat(x.Tracks.Values.Select(y => y.RotationTime)))
            .Distinct()
            .Where(x => x is not null)
            .OrderBy(x => x!.Format)
            .Cast<ControllerKeyTime>()
            .ToList();
        var timeDict = time
            .Select((x, i) => (x: x!, i))
            .ToDictionary(x => x.x, x => x.i);
        chunks.AddChunkBE(
            ChunkType.Controller,
            0x905,
            new ControllerChunk {
                KeyTimes = time,
                KeyPositions = pos,
                KeyRotations = rot,
                Animations = Animations.Select(
                    x => new ControllerGroup {
                        Name = x.Key,
                        FootPlanBits = Array.Empty<byte>(),
                        MotionParams = x.Value.MotionParams,
                        Controllers = x.Value.Tracks.Select(
                            y => new ControllerTrack {
                                ControllerId = y.Key,
                                PosTrack = y.Value.Position is null
                                    ? ControllerTrack.InvalidTrack
                                    : posDict[y.Value.Position],
                                RotTrack = y.Value.Rotation is null
                                    ? ControllerTrack.InvalidTrack
                                    : rotDict[y.Value.Rotation],
                                PosKeyTimeTrack = y.Value.PositionTime is null
                                    ? ControllerTrack.InvalidTrack
                                    : timeDict[y.Value.PositionTime],
                                RotKeyTimeTrack = y.Value.RotationTime is null
                                    ? ControllerTrack.InvalidTrack
                                    : timeDict[y.Value.RotationTime],
                            }).ToList(),
                    }).ToList(),
            });
        chunks.WriteTo(writer);
    }

    public void WriteTo(Stream stream) => WriteTo(new NativeWriter(stream, Encoding.UTF8, true));
    
    public byte[] GetBytes() {
        var ms = new MemoryStream();
        WriteTo(ms);
        return ms.ToArray();
    }

    private static void NotSupportedIfFalse(bool test, string? message = null) {
        if (!test)
            throw new NotSupportedException(message);
    }
    
    public static CryAnimationDatabase FromStream(Stream stream, bool leaveOpen = false) {
        var res = new CryAnimationDatabase();
        var chunks = CryChunks.FromStream(stream, leaveOpen);
        foreach (var chunk in chunks.Values.OfType<ControllerChunk>()) {
            foreach (var anim in chunk.Animations) {
                NotSupportedIfFalse(!anim.FootPlanBits.Any());
                var a = new Animation {MotionParams = anim.MotionParams};
                foreach (var track in anim.Controllers) {
                    a.Tracks[track.ControllerId] = new() {
                        Position = track.HasPosTrack ? chunk.KeyPositions[track.PosTrack] : null,
                        Rotation = track.HasRotTrack ? chunk.KeyRotations[track.RotTrack] : null,
                        PositionTime = track.HasPosTrack ? chunk.KeyTimes[track.PosKeyTimeTrack] : null,
                        RotationTime = track.HasRotTrack ? chunk.KeyTimes[track.RotKeyTimeTrack] : null,
                    };
                }

                res.Animations[anim.Name] = a;
            }
        }

        return res;
    }
}
