using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.CryEngine.CryAnimationDatabaseElements;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.FileFormat.GltfInterop.Models;
using SynergyLib.Util;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public partial class CryCharacter {
    public CharacterDefinition? Definition;
    public CryModel Model;
    public CharacterParameters? CharacterParameters;
    public CryAnimationDatabase? CryAnimationDatabase;
    public List<CryModel> Attachments = new();

    public CryCharacter(CryModel model) {
        Model = model;
    }

    public Task<GltfTuple> ToGltf(
        Func<string, CancellationToken, Task<Stream>> getStream,
        bool useAnimation,
        bool exportOnlyRequiredTextures,
        CancellationToken cancellationToken) =>
        new GltfExporter(this, getStream, useAnimation, exportOnlyRequiredTextures, cancellationToken).Process();

    public static CryCharacter FromGltf(GltfTuple gltf, string? name, CancellationToken cancellationToken) =>
        new GltfImporter(gltf, name, cancellationToken).Process();

    public static async Task<CryCharacter> FromCryEngineFiles(
        Func<string, CancellationToken, Task<Stream>> streamOpener,
        string baseName,
        CancellationToken cancellationToken) {
        if (Path.GetExtension(baseName).ToLowerInvariant() is ".cdf" or ".cgf" or ".chr")
            baseName = Path.ChangeExtension(baseName, null);
        CharacterDefinition? definition = null;

        CryModel model;
        try {
            await using (var definitionStream = await streamOpener($"{baseName}.cdf", cancellationToken))
                definition = PbxmlFile.FromStream(definitionStream).DeserializeAs<CharacterDefinition>();
            if (definition.Model is null)
                throw new InvalidDataException("Definition.Model should not be null");
            if (definition.Model.File is null)
                throw new InvalidDataException("Definition.Model.File should not be null");
            if (definition.Model.Material is null)
                throw new InvalidDataException("Definition.Model.Material should not be null");

            model = await CryModel.FromCryEngineFiles(
                streamOpener,
                definition.Model.File,
                definition.Model.Material,
                cancellationToken);
        } catch (FileNotFoundException) {
            try {
                model = await CryModel.FromCryEngineFiles(streamOpener, $"{baseName}.chr", null, cancellationToken);
            } catch (FileNotFoundException) {
                model = await CryModel.FromCryEngineFiles(streamOpener, $"{baseName}.cgf", null, cancellationToken);
            }
        }

        var res = new CryCharacter(model) {Definition = definition};
        if (definition is not null) {
            foreach (var d in (IEnumerable<Attachment>?) definition.Attachments ?? Array.Empty<Attachment>()) {
                if (d.Binding is null)
                    throw new InvalidDataException("Attachment.Binding should not be null");
                if (d.Material is null)
                    throw new InvalidDataException("Attachment.Material should not be null");
                res.Attachments.Add(
                    await CryModel.FromCryEngineFiles(streamOpener, d.Binding, d.Material, cancellationToken));
            }
        }

        try {
            await using (var s = await streamOpener($"{baseName}.chrparams", cancellationToken))
                res.CharacterParameters = PbxmlFile.FromStream(s).DeserializeAs<CharacterParameters>();
            if (res.CharacterParameters.TracksDatabasePath is not null)
                await using (var s = await streamOpener(res.CharacterParameters.TracksDatabasePath, cancellationToken))
                    res.CryAnimationDatabase = CryAnimationDatabase.FromStream(s);
        } catch (FileNotFoundException) { }

        return res;
    }


    public static Vector3 SwapAxes(Vector3 val) => new(-val.X, val.Z, val.Y);
    protected static Vector3 SwapAxesScale(Vector3 val) => new(val.X, val.Z, val.Y);
    protected static Vector4 SwapAxesTangent(Vector4 val) => new(-val.X, val.Z, val.Y, val.W);
    protected static Quaternion SwapAxes(Quaternion val) => new(-val.X, val.Z, val.Y, val.W);

    protected static Matrix4x4 SwapAxes(Matrix4x4 val) => new(
        val.M11,
        -val.M13,
        -val.M12,
        -val.M14,
        -val.M31,
        val.M33,
        val.M32,
        val.M34,
        -val.M21,
        val.M23,
        val.M22,
        val.M24,
        -val.M41,
        val.M43,
        val.M42,
        val.M44);

    public void ApplyScaleTransformation(float scale) {
        Model.ApplyScaleTransformation(scale);
        CryAnimationDatabase?.ApplyScaleTransformation(scale);
        // todo: scale attachments
    }
}
