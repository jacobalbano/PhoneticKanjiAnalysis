using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KanjiDicAnalysis;

internal class KanjiVG : IReadOnlyDictionary<string, KanjiVG.Entry>
{
    public record class Entry(string Phon, IReadOnlyCollection<string> Radicals);

    public static KanjiVG Create()
    {
        if (File.Exists("kanjivg.bin"))
            return DeserializeBinary();

        var result = LoadFiles();
        result.SerializeBinary();
        return result;
    }

    private void SerializeBinary()
    {
        using var stream = File.OpenWrite("kanjivg.bin");
        var writer = new BinaryWriter(stream);

        writer.Write7BitEncodedInt(Storage.Count);
        foreach (var (k, p) in Storage)
        {
            writer.Write(k);
            writer.Write(p.Phon != null);
            if (p.Phon != null)
                writer.Write(p.Phon);
            writer.Write7BitEncodedInt(p.Radicals.Count);
            foreach (var r in p.Radicals)
                writer.Write(r);
        }
    }

    private static KanjiVG DeserializeBinary()
    {
        using var stream = File.OpenRead("kanjivg.bin");
        var reader = new BinaryReader(stream);
        var storage = new Dictionary<string, Entry>();

        var count = reader.Read7BitEncodedInt();
        for (int i = 0; i < count; i++)
        {
            var kanji = reader.ReadString();
            var phon = reader.ReadBoolean() ? reader.ReadString() : null;
            var rads = new string[reader.Read7BitEncodedInt()];
            for (int j = 0; j < rads.Length; j++)
                rads[j] = reader.ReadString();

            storage[kanji] = new Entry(phon, rads);
        }

        return new KanjiVG { Storage = storage };
    }

    private static KanjiVG LoadFiles()
    {
        if (!Directory.Exists("kanjivg"))
            throw new Exception("Cannot build optimized file. Please include the 'kanjivg' repository in the working directory");

        var kvg = XNamespace.Get("http://kanjivg.tagaini.net");
        var xmlns = XNamespace.Get("http://www.w3.org/2000/svg");
        var kvgElement = kvg + "element";
        var kvgPhon = kvg + "phon";

        var storage = new Dictionary<string, Entry>();
        foreach (var file in Directory.EnumerateFiles("kanjivg", "*.svg", SearchOption.AllDirectories))
        {
            if (new FileInfo(file).Name.Contains('-'))
            {
                Debug.WriteLine("Skipping variant");
                continue;
            }

            var svg = XDocument.Load(file);
            var elements = svg.Descendants(xmlns + "g")
                .Where(x => x.Attribute(kvg + "element") != null)
                .Select(x => (
                    element: x.Attribute(kvgElement)!.Value,
                    hasPhon: x.Attribute(kvgPhon) != null
                )).ToList();

            if (elements.Count == 1)
            {
                Debug.WriteLine($"Skipping {elements.First().element}");
                continue;
            }

            string kanji = elements.First().element, phon = null;
            var radicals = new HashSet<string>();
            foreach (var (el, hasPhon) in elements.Skip(1))
            {
                if (hasPhon) phon = el;
                radicals.Add(el);
            }

            if (phon == null)
                Debug.WriteLine($"No phonetic component for {kanji}");
            else if (elements.Count(x => x.hasPhon) > 1)
                Debug.WriteLine($"Multiple phonetic elements found in {kanji}", "warning");

            storage.Add(kanji, new Entry(phon, radicals));
        }

        return new KanjiVG() { Storage = storage };
    }


    #region impl
    private Dictionary<string, Entry> Storage { get; init; } = new();
    public bool ContainsKey(string key) => Storage.ContainsKey(key);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Entry value) => Storage.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, Entry>> GetEnumerator() => Storage.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Storage.GetEnumerator();
    public IEnumerable<string> Keys => Storage.Keys;
    public IEnumerable<Entry> Values => Storage.Values;
    public int Count => Storage.Count;
    public Entry this[string key] => Storage[key];
    #endregion
}
