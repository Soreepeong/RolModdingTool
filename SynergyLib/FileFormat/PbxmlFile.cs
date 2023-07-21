using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat;

public class PbxmlFile {
    public static readonly ImmutableArray<byte> Magic = "pbxml\0"u8.ToArray().ToImmutableArray();

    public readonly XmlDocument Document;

    public PbxmlFile() {
        Document = new();
    }

    public PbxmlFile(XmlDocument document) {
        Document = document;
    }

    public T DeserializeAs<T>(bool throwOnUnknown = true) where T : class {
        var oldCulture = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = (CultureInfo) oldCulture.Clone();
            CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator = ",";
            var serializer = new XmlSerializer(typeof(T));
            if (throwOnUnknown) {
                serializer.UnknownAttribute += (_, args) =>
                    throw new NotSupportedException($"Unknown attribute: {args.Attr.Name}");
                serializer.UnknownElement += (_, args) =>
                    throw new NotSupportedException($"Unknown element: {args.Element.Name}");
            }

            using var reader = new XmlNodeReader(Document);
            return serializer.Deserialize(reader) as T ?? throw new NullReferenceException();
        } finally {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    public void WriteBinaryToFile(string file) => WriteBinary(File.Create(file));

    public void WriteBinary(Stream stream, bool leaveOpen = false) =>
        WriteBinary(new BinaryWriter(stream, Encoding.UTF8, true), leaveOpen);

    public void WriteBinary(BinaryWriter target, bool leaveOpen = false) {
        try {
            target.Write(Magic.AsSpan());
            PackElement(target, Document.ChildNodes.OfType<XmlElement>().Single());
        } finally {
            if (!leaveOpen)
                target.Dispose();
        }
    }

    public void WriteTextToFile(string file) => WriteText(File.Create(file));

    public void WriteText(Stream target, bool leaveOpen = false) =>
        WriteText(new StreamWriter(target, new UTF8Encoding(), -1, true), leaveOpen);

    public void WriteText(StreamWriter target, bool leaveOpen = false) {
        try {
            Document.Save(new XmlTextWriter(target) {Formatting = Formatting.Indented});
        } finally {
            if (!leaveOpen)
                target.Dispose();
        }
    }

    public static void SaveObjectToTextFile<T>(string path, T obj) where T : class =>
        FromObject(obj).WriteTextToFile(path);

    public static PbxmlFile FromObject<T>(T obj) where T : class {
        var oldCulture = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = (CultureInfo) oldCulture.Clone();
            CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator = ",";
            var xns = new XmlSerializerNamespaces();
            var serializer = new XmlSerializer(typeof(T));
            xns.Add(string.Empty, string.Empty);
            var doc = new XmlDocument();
            using (var w = doc.CreateNavigator()!.AppendChild())
                serializer.Serialize(w, obj, xns);
            return new(doc);
        } finally {
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    public static PbxmlFile FromReader(BinaryReader reader, bool leaveOpen = false) {
        try {
            Span<byte> magicChecker = stackalloc byte[Magic.Length];
            reader.BaseStream.ReadExactly(magicChecker);

            var res = new PbxmlFile();
            if (IsPbxmlFile(magicChecker))
                res.Document.AppendChild(UnpackElement(reader, res.Document));
            else {
                reader.BaseStream.Position -= magicChecker.Length;
                res.Document.Load(reader.BaseStream);
            }

            return res;
        } finally {
            if (!leaveOpen)
                reader.Dispose();
        }
    }

    public static PbxmlFile FromStream(Stream stream, bool leaveOpen = false) => FromReader(
        new(stream, Encoding.UTF8, true),
        leaveOpen);

    public static PbxmlFile FromBytes(byte[] data) => FromStream(new MemoryStream(data));

    public static PbxmlFile FromPath(params string[] path) {
        using var stream = File.OpenRead(Path.Join(path));
        return FromStream(stream);
    }

    public static bool IsPbxmlFile(ReadOnlySpan<byte> buffer) =>
        buffer.Length >= Magic.Length &&
        buffer.CommonPrefixLength(Magic.AsSpan()) == Magic.Length;

    private static XmlElement UnpackElement(BinaryReader reader, XmlDocument doc, XmlNamespaceManager? nsmgr = null) {
        nsmgr ??= new(new NameTable());
        nsmgr.PushScope();

        var numberOfChildren = reader.ReadCryInt();
        var numberOfAttributes = reader.ReadCryInt();
        var nodeName = reader.ReadCString();

        var attrs = new Dictionary<string, string>();
        for (var i = 0; i < numberOfAttributes; i++) {
            var key = reader.ReadCString();
            var value = reader.ReadCString();

            attrs.Add(key, value);
            if (key.StartsWith("xmlns:"))
                nsmgr.AddNamespace(key[6..], value);
        }

        var element = nodeName.IndexOf(':') switch {
            -1 => doc.CreateElement(nodeName),
            var r => doc.CreateElement(nodeName[..r], nodeName[(r + 1)..], nsmgr.LookupNamespace(nodeName[..r])),
        };

        foreach (var (key, value) in attrs) {
            var sep = key.IndexOf(':');
            if (sep == -1 || key.StartsWith("xmlns:"))
                element.SetAttribute(key, value);
            else
                element.SetAttribute(key[(sep + 1)..], nsmgr.LookupNamespace(key[..sep]), value);
        }

        var nodeText = reader.ReadCString();
        if (nodeText != "")
            element.AppendChild(doc.CreateTextNode(nodeText));

        for (var i = 0; i < numberOfChildren; i++) {
            var expectedLength = reader.ReadCryInt();
            var expectedPosition = reader.BaseStream.Position + expectedLength;
            element.AppendChild(UnpackElement(reader, doc, nsmgr));
            if (i + 1 == numberOfChildren) {
                if (expectedLength != 0)
                    throw new InvalidDataException("Last child node must not have an expectedLength.");
            } else {
                if (reader.BaseStream.Position != expectedPosition)
                    throw new InvalidDataException("Expected length does not match.");
            }
        }

        nsmgr.PopScope();
        return element;
    }

    private static void PackElement(BinaryWriter writer, XmlNode element) {
        var textElement = element.ChildNodes.OfType<XmlText>().SingleOrDefault();
        var childElements = element.ChildNodes.OfType<XmlElement>().ToArray();
        var attributes = element.Attributes?.Cast<XmlAttribute>().ToArray() ?? Array.Empty<XmlAttribute>();

        writer.WriteCryInt(childElements.Length);
        writer.WriteCryInt(attributes.Length);
        writer.WriteCString(element.Name);
        foreach (var attrib in attributes) {
            writer.WriteCString(attrib.Name);
            writer.WriteCString(attrib.Value);
        }

        writer.WriteCString(textElement?.Value ?? "");

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        for (var i = 0; i < childElements.Length; i++) {
            if (i == childElements.Length - 1) {
                writer.WriteCryInt(0);
                PackElement(writer, childElements[i]);
            } else {
                ms.SetLength(ms.Position = 0);
                PackElement(bw, childElements[i]);
                writer.WriteCryInt((int) ms.Length);
                ms.Position = 0;
                ms.CopyTo(writer.BaseStream);
            }
        }
    }
}
