using KanjiDicAnalysis;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.CommandLine;

var kanjiCovOption = new Option<int>(
    "--min-kanji-coverage",
    () => 66,
    "Minimum kanji coverage for a series to be included"
);

var readingCovOption = new Option<int>(
    "--min-reading-coverage",
    () => 33,
    "Minimum reading coverage for a series to be included"
);

var countOption = new Option<int>(
    "--min-count",
    () => 3,
    "Minimum kanji count for a series to be included"
);

var outpathOption = new Option<string>(
    "--output",
    "Output filename (optional)"
);

var rootCommand = new RootCommand("Generate phonetic series report");
rootCommand.Add(kanjiCovOption);
rootCommand.Add(readingCovOption);
rootCommand.Add(countOption);
rootCommand.Add(outpathOption);
rootCommand.SetHandler((minKanjiCov, minReadingCov, minCount, outpath) =>
{
    var report = Series.Report()
        .OrderByDescending(x => x.KanjiCoverage)
        .ThenByDescending(x => x.Kanji.Length)
        .ThenByDescending(x => x.ReadingsCoverage)
        ;

    int skippedSeries = 0;
    var sb = new StringBuilder();
    foreach (var (radical, kCov, rCov, kanji, readings) in report)
    {
        if (kCov < minKanjiCov || rCov < minReadingCov || kanji.Length < minCount)
        {
            skippedSeries++;
            continue;
        }

        sb.AppendLine(string.Join('\t', new[]
        {
            radical,
            kCov.ToString(),
            rCov.ToString(),
            string.Join(" ", kanji),
            string.Join(", ", readings.OrderByDescending(x => x.Value)),
        }));
    }

    outpath ??= $"minK={minKanjiCov},minR={minReadingCov},minC={minCount}.tsv";
    File.WriteAllText(outpath, sb.ToString());

    Console.WriteLine($"Skipped {skippedSeries} series according to parameters");
    Console.WriteLine($"Written to {outpath}");
}, kanjiCovOption, readingCovOption, countOption, outpathOption);

await rootCommand.InvokeAsync(args);