#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

namespace KanjiDicAnalysis;

public class Kanjidic : IReadOnlyList<Kanjidic.Entry>
{
	public record class Entry(string Literal, IReadOnlyList<string> Readings, int Frequency);

    public static Kanjidic Create()
	{
		if (File.Exists("kanjidic.bin"))
			return DeserializeBinary();

		var result = LoadFromXml();
		result.SerializeBinary();
		return result;
	}

    public static Kanjidic LoadFromXml()
	{
		if (!File.Exists("kanjidic2.xml"))
			throw new Exception("Cannot build optimized file. Please include 'kanjidict2.xml' in the working directory");

		var x = new XmlSerializer(typeof(Kanjidic2));
		using var stream = File.OpenRead("kanjidic2.xml");
		using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse });
		var result = (Kanjidic2)x.Deserialize(reader)!;

		return new Kanjidic
		{
			Entries = result.Characters
				.Select(x => new Entry(x.Literal, OnReadings(x), x.Misc.Freq))
				.ToList(),
		};
	}

	[XmlRoot("kanjidic2")]
	public class Kanjidic2
	{
		[XmlElement("character")]
		public List<Character> Characters { get; init; }

		public class Character
		{
			[XmlElement("literal")]
			public string Literal { get; init; }

			[XmlElement("misc")]
			public MiscData Misc { get; init; }

			[XmlElement("reading_meaning")]
			public ReadingMeaning ReadingGroups { get; init; }

			public record class MiscData
			{
				[XmlElement("freq")]
				public int Freq { get; init; }
			}

			public class Reading
			{
				[XmlAttribute("r_type")]
				public string Type { get; init; }

				[XmlText]
				public string Text { get; init; }
			}

			public class ReadingMeaning
			{
				[XmlElement("rmgroup")]
				public Rmgroup Group { get; init; }

				public class Rmgroup
				{
					[XmlElement("reading")]
					public List<Reading> Reading { get; init; }
				}
			}
		}
	}

	private void SerializeBinary()
	{
		using var stream = File.OpenWrite("kanjidic.bin");
		var writer = new BinaryWriter(stream);

		writer.Write7BitEncodedInt(Entries.Count);
		foreach (var e in Entries)
		{
			writer.Write(e.Literal);
			writer.Write(e.Frequency);
			writer.Write7BitEncodedInt(e.Readings.Count);
			foreach (var r in e.Readings)
				writer.Write(r);
		}
	}

	private static Kanjidic DeserializeBinary()
	{
		using var stream = File.OpenRead("kanjidic.bin");
		var reader = new BinaryReader(stream);
		var entries = new List<Entry>();

		var count = reader.Read7BitEncodedInt();
		for (int i = 0; i < count; i++)
		{
			var kanji = reader.ReadString();
			var freq = reader.ReadInt32();
			var readings = new string[reader.Read7BitEncodedInt()];
			for (int j = 0; j < readings.Length; j++)
				readings[j] = reader.ReadString();

			entries.Add(new Entry(kanji, readings, freq));
		}

		return new Kanjidic {  Entries = entries };
	}

	private static string[] OnReadings(Kanjidic2.Character arg)
	{
		if (arg.ReadingGroups == null)
			return Array.Empty<string>();

		return arg.ReadingGroups.Group.Reading
			.Where(x => x.Type == "ja_on")
			.Select(x => x.Text)
			.Distinct()
			.ToArray();
	}
    #region impl
    private IReadOnlyList<Entry> Entries { get; init; }
	public int Count => Entries.Count;
	public Entry this[int index] => Entries[index];
	public IEnumerator<Entry> GetEnumerator() => Entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Entries).GetEnumerator();
    #endregion

    private Kanjidic() { }
}


#pragma warning restore CS8618