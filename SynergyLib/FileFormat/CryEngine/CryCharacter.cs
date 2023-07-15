using System;
using System.Collections.Generic;
using System.IO;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

namespace SynergyLib.FileFormat.CryEngine;

public class CryCharacter {
    public CharacterDefinition? Definition;
    public CryModel Model;
    public CharacterParameters? CharacterParameters;
    public CryAnimationDatabase? CryAnimationDatabase;
    public List<CryModel> Attachments = new();

    public CryCharacter(Func<string, Stream> streamOpener, string baseName) {
        try {
            Definition = PbxmlFile.FromStream(streamOpener($"{baseName}.cdf")).DeserializeAs<CharacterDefinition>();
            if (Definition.Model is null)
                throw new InvalidDataException("Definition.Model should not be null");
            if (Definition.Model.File is null)
                throw new InvalidDataException("Definition.Model.File should not be null");
            if (Definition.Model.Material is null)
                throw new InvalidDataException("Definition.Model.Material should not be null");

            Model = new(streamOpener(Definition.Model.File), streamOpener(Definition.Model.Material));
            foreach (var d in (IEnumerable<Attachment>?) Definition.Attachments ?? Array.Empty<Attachment>()) {
                if (d.Binding is null)
                    throw new InvalidDataException("Attachment.Binding should not be null");
                if (d.Material is null)
                    throw new InvalidDataException("Attachment.Material should not be null");
                Attachments.Add(new(streamOpener(d.Binding), streamOpener(d.Material)));
            }
        } catch (FileNotFoundException) {
            Model = new(streamOpener($"{baseName}.chr"), streamOpener($"{baseName}.mtl"));
        }

        try {
            CharacterParameters = PbxmlFile.FromStream(streamOpener($"{baseName}.chrparams"))
                .DeserializeAs<CharacterParameters>();
            if (CharacterParameters.TracksDatabasePath is not null)
                CryAnimationDatabase = new(streamOpener(CharacterParameters.TracksDatabasePath));
        } catch (FileNotFoundException) { }
    }
}
