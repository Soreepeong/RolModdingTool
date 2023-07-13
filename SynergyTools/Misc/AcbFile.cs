using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using SonicAudioLib.Archives;
using SonicAudioLib.CriMw;
using SynergyLib.Util.BinaryRW;

namespace SynergyTools.Misc;

[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class AcbFile {
    private static readonly CriTableWriterSettings WriterSettings = new() {
        EncodingType = Encoding.UTF8,
        PutBlankString = false,
        LeaveOpen = true,
        RemoveDuplicateStrings = false,
    };

    public readonly Dictionary<string, List<int>> RelevantWaveformIds = new();
    public readonly Dictionary<string, int> CueNameToSequenceIndex = new();
    public readonly CriTable Acb;
    public readonly CriTable CueTable;
    public readonly CriTable CueNameTable;
    public readonly CriTable SequenceTable;
    public readonly CriTable TrackTable;
    public readonly CriTable CommandTable;
    public readonly CriTable SynthTable;
    public readonly CriTable WaveformTable;
    public readonly List<CriRow> InternalWaveformRows;
    public readonly List<CriRow> ExternalWaveformRows;
    public readonly Dictionary<int, byte[]> InternalWaveforms;
    public readonly Dictionary<int, byte[]>? ExternalWaveforms;

    public AcbFile(string acbPath, string? awbPath) {
        Acb = new() {WriterSettings = WriterSettings};
        using (var s = File.OpenRead(acbPath))
            Acb.Read(s);

        WaveformTable = new() {WriterSettings = WriterSettings};
        WaveformTable.Load(Acb.Rows[0].GetValue<byte[]>("WaveformTable"));

        CueTable = new() {WriterSettings = WriterSettings};
        CueTable.Load(Acb.Rows[0].GetValue<byte[]>("CueTable"));

        CueNameTable = new() {WriterSettings = WriterSettings};
        CueNameTable.Load(Acb.Rows[0].GetValue<byte[]>("CueNameTable"));

        SynthTable = new() {WriterSettings = WriterSettings};
        SynthTable.Load(Acb.Rows[0].GetValue<byte[]>("SynthTable"));

        CommandTable = new() {WriterSettings = WriterSettings};
        CommandTable.Load(Acb.Rows[0].GetValue<byte[]>("CommandTable"));

        TrackTable = new() {WriterSettings = WriterSettings};
        TrackTable.Load(Acb.Rows[0].GetValue<byte[]>("TrackTable"));

        SequenceTable = new() {WriterSettings = WriterSettings};
        SequenceTable.Load(Acb.Rows[0].GetValue<byte[]>("SequenceTable"));

        InternalWaveformRows = WaveformTable.Rows.Where(x => x.GetValue<byte>("Streaming") == 0).ToList();
        ExternalWaveformRows = WaveformTable.Rows.Except(InternalWaveformRows).ToList();

        var awbArchive = new CriAfs2Archive();
        var awbData = Acb.Rows[0].GetValue<byte[]>("AwbFile");
        if (awbData.Any()) {
            awbArchive.Load(awbData);
            InternalWaveforms = new(awbArchive.Count);
            foreach (var row in InternalWaveformRows) {
                var id = row.GetValue<ushort>("Id");
                var awbEntry = awbArchive.Single(x => x.Id == id);
                InternalWaveforms.Add(
                    id,
                    awbData[(int) awbEntry.Position .. (int) (awbEntry.Position + awbEntry.Length)]);
            }
        } else
            InternalWaveforms = new();

        if (awbPath is not null) {
            awbData = File.ReadAllBytes(awbPath);
            awbArchive = new();
            awbArchive.Load(awbData);
            ExternalWaveforms = new(awbArchive.Count);
            foreach (var row in ExternalWaveformRows) {
                var id = row.GetValue<ushort>("Id");
                var awbEntry = awbArchive.Single(x => x.Id == id);
                ExternalWaveforms.Add(
                    id,
                    awbData[(int) awbEntry.Position .. (int) (awbEntry.Position + awbEntry.Length)]);
            }
        }

        for (var i = 0; i < CueTable.Rows.Count; i++) {
            var rowCue = CueTable.Rows[i];
            if (rowCue.GetValue<byte>("ReferenceType") != 3)
                throw new NotSupportedException();

            var rowCueName = CueNameTable.Rows.Single(x => x.GetValue<ushort>("CueIndex") == i);
            var cueName = rowCueName.GetValue<string>("CueName");
            var waveformIds = RelevantWaveformIds[cueName] = new();

            var sequenceIndex = rowCue.GetValue<ushort>("ReferenceIndex");
            CueNameToSequenceIndex[cueName] = sequenceIndex;
            var sequence = SequenceTable.Rows[sequenceIndex];
            var trackIndices = BinaryMiscUtils.GetBigEndianArray<ushort>(sequence.GetValue<byte[]>("TrackIndex"));
            foreach (var trackIndex in trackIndices) {
                var track = TrackTable.Rows[trackIndex];
                var eventIndex = track.GetValue<ushort>("EventIndex");
                if (eventIndex == 65535)
                    continue;

                var command = NativeReader.FromBytes(CommandTable.Rows[eventIndex].GetValue<byte[]>("Command"));
                command.IsBigEndian = true;
                while (command.BaseStream.Position < command.BaseStream.Length) {
                    var code = command.ReadUInt16();
                    var size = command.ReadByte();
                    var next = command.BaseStream.Position + size;
                    switch (code) {
                        case 0: // no-op
                            break;
                        case 2000: // noteOn
                        case 2003: // noteOnWithNo
                            var referenceType = command.ReadUInt16();
                            var referenceIndex = command.ReadUInt16();
                            if (referenceType != 2) // don't care if it's not a synth
                                break;

                            var synth = SynthTable.Rows[referenceIndex];
                            var referenceItems =
                                BinaryMiscUtils.GetBigEndianArray<ushort>(synth.GetValue<byte[]>("ReferenceItems"));
                            for (var j = 0; j < referenceItems.Length; j += 2) {
                                var itemType = referenceItems[j];
                                var itemIndex = referenceItems[j + 1];
                                switch (itemType) {
                                    case 0:
                                        break;
                                    case 1:
                                        waveformIds.Add(WaveformTable.Rows[itemIndex].GetValue<ushort>("Id"));
                                        break;
                                    default:
                                        throw new NotSupportedException();
                                }
                            }

                            break;
                        case 999:
                        case 1990:
                        case 2001:
                        case 4000:
                            break; // unknown opcodes
                        default:
                            throw new NotSupportedException();
                    }

                    command.BaseStream.Position = next;
                }
            }
        }
    }

    public ushort[] GetTrackIndices(int sequenceIndex) {
        var sequence = SequenceTable.Rows[sequenceIndex];
        return BinaryMiscUtils.GetBigEndianArray<ushort>(sequence.GetValue<byte[]>("TrackIndex"));
    }

    public void SetTrackIndices(int sequenceIndex, ushort[] trackIndices) {
        var sequence = SequenceTable.Rows[sequenceIndex];
        sequence["NumTracks"] = (ushort) trackIndices.Length;

        var ms = new MemoryStream();
        var bw = new NativeWriter(ms) {IsBigEndian = true};
        foreach (var x in trackIndices)
            bw.Write(x);
        sequence["TrackIndex"] = ms.ToArray();

        ms.Position = 0;
        sequence["PlaybackRatio"] = (ushort) 100;
        if (trackIndices.Length == 0) {
            sequence["TrackValues"] = Array.Empty<byte>();
        } else {
            foreach (var _ in trackIndices.SkipLast(1))
                bw.Write((ushort) (100 / trackIndices.Length));
            bw.Write((ushort) (100 - 100 / trackIndices.Length * (trackIndices.Length - 1)));
            sequence["TrackValues"] = ms.ToArray();
        }
    }

    public ushort AddTrack(CriRow sourceRow, byte[] hca) {
        var waveformIndex = (ushort) WaveformTable.Rows.Count;
        var waveformRow = WaveformTable.NewRow();
        for (var i = 0; i < WaveformTable.Fields.Count; i++)
            waveformRow[i] = sourceRow[i];

        var id = checked((ushort) (InternalWaveforms.Count == 0 ? 0 : InternalWaveforms.Keys.Max() + 1));
        waveformRow["Id"] = id;
        WaveformTable.Rows.Add(waveformRow);
        InternalWaveformRows.Add(waveformRow);
        InternalWaveforms.Add(id, hca);

        var synthIndex = (ushort) SynthTable.Rows.Count;
        var synthRow = SynthTable.NewRow();
        for (var i = 0; i < SynthTable.Fields.Count; i++)
            synthRow[i] = SynthTable.Rows[0][i];
        synthRow["ReferenceItems"] = new byte[] {0x00, 0x01, (byte) (waveformIndex >> 8), (byte) waveformIndex};
        synthRow["ControlWorkArea1"] = synthRow["ControlWorkArea2"] = synthIndex;
        SynthTable.Rows.Add(synthRow);

        var commandIndex = (ushort) CommandTable.Rows.Count;
        var commandRow = CommandTable.NewRow();
        commandRow["Command"] = new byte[] {
            // op=0x07d0(2000), length=0x04, synth=0x0002, synthIndex
            0x07, 0xd0, 0x04, 0x00, 0x02, (byte) (synthIndex >> 8), (byte) synthIndex,

            // apparently without this it keeps playing next thing
            // op=0x0000(0), length=0x00
            0x00, 0x00, 0x00,
        };
        CommandTable.Rows.Add(commandRow);

        var trackIndex = (ushort) TrackTable.Rows.Count;
        var trackRow = TrackTable.NewRow();
        for (var i = 0; i < TrackTable.Fields.Count; i++)
            trackRow[i] = TrackTable.Rows[0][i];
        trackRow["EventIndex"] = commandIndex;
        TrackTable.Rows.Add(trackRow);

        return trackIndex;
    }

    public void Save(string baseName, bool disableExternalAwb = false) {
        var awbArchive = new CriAfs2Archive {
            Align = 32,
            SubKey = 0
        };

        if (disableExternalAwb || ExternalWaveforms is null) {
            for (var i = 0; i < WaveformTable.Rows.Count; i++) {
                var row = WaveformTable.Rows[i];
                var id = row.GetValue<ushort>("Id");
                if (row.GetValue<byte>("Streaming") == 0) {
                    awbArchive.Add(
                        new() {
                            Id = (uint) i,
                            Data = InternalWaveforms[id],
                            Length = InternalWaveforms[id].Length,
                        });
                } else {
                    Debug.Assert(ExternalWaveforms is not null);
                    awbArchive.Add(
                        new() {
                            Id = (uint) i,
                            Data = ExternalWaveforms[id],
                            Length = ExternalWaveforms[id].Length,
                        });
                }

                row["Id"] = (ushort) i;
                row["Streaming"] = (byte) 0;
            }

            using var ms = new MemoryStream();
            awbArchive.Write(ms);
            Acb.Rows[0]["AwbFile"] = ms.ToArray();
        } else {
            if (InternalWaveforms.Any()) {
                for (var i = 0; i < InternalWaveforms.Count; i++) {
                    var id = InternalWaveformRows[i].GetValue<ushort>("Id");
                    awbArchive.Add(
                        new() {
                            Id = id,
                            Data = InternalWaveforms[id],
                            Length = InternalWaveforms[id].Length,
                        });
                }

                using var ms = new MemoryStream();
                awbArchive.Write(ms);
                Acb.Rows[0]["AwbFile"] = ms.ToArray();
            } else
                Acb.Rows[0]["AwbFile"] = Array.Empty<byte>();

            awbArchive.Clear();
            for (var i = 0; i < ExternalWaveforms.Count; i++) {
                var id = ExternalWaveformRows[i].GetValue<ushort>("Id");
                awbArchive.Add(
                    new() {
                        Id = id,
                        Data = ExternalWaveforms[id],
                        Length = ExternalWaveforms[id].Length,
                    });
            }

            awbArchive.Save(baseName + ".awb");
        }

        using (var ms = new MemoryStream()) {
            WaveformTable.Write(ms);
            Acb.Rows[0]["WaveformTable"] = ms.ToArray();
        }

        using (var ms = new MemoryStream()) {
            SynthTable.Write(ms);
            Acb.Rows[0]["SynthTable"] = ms.ToArray();
        }

        using (var ms = new MemoryStream()) {
            CommandTable.Write(ms);
            Acb.Rows[0]["CommandTable"] = ms.ToArray();
        }

        using (var ms = new MemoryStream()) {
            TrackTable.Write(ms);
            Acb.Rows[0]["TrackTable"] = ms.ToArray();
        }

        using (var ms = new MemoryStream()) {
            SequenceTable.Write(ms);
            Acb.Rows[0]["SequenceTable"] = ms.ToArray();
        }

        Acb.Save(baseName + ".acb");
    }
}
