using KanjiDicAnalysis;

record class Series(
    string Radical,
    int KanjiCoverage,
    int ReadingsCoverage,
    string[] Kanji,
    IReadOnlyDictionary<string, int> Readings
)
{
    public static IEnumerable<Series> Report()
    {
        var kanjidic = Kanjidic.Create();
        var charByKanjiString = kanjidic.ToDictionary(x => x.Literal);

        var kvg = KanjiVG.Create();
        var kvgByPhon = kvg
            .Select(x => x.Value.Phon)
            .Where(x => x != null)
            .Distinct()
            .Select(x => (x, kvg.Where(y => y.Key == x || y.Value.Radicals.Contains(x))))
            .ToDictionary(x => x.x, x => x.Item2.Select(y => y.Key).ToHashSet());

        foreach (var group in kvgByPhon)
        {
            var kanjiForPhon = group.Value
                .Select(x => (exists: charByKanjiString.TryGetValue(x, out var c), character: c))
                .Where(x => x.exists)
                .Select(x => x.character)
                .ToList();

            var literals = kanjiForPhon
                .Select(x => x.Literal)
                .ToArray();

            var allReadings = kanjiForPhon
                .SelectMany(x => x.Readings)
                .ToArray();

            if (!allReadings.Any())
                continue;

            var groupedReadings = allReadings
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            var distinctReadings = allReadings
                .Distinct()
                .ToArray();

            var mainReading = groupedReadings
                .OrderByDescending(x => x.Value)
                .First();

            float kCoverage = mainReading.Value / (float)literals.Length;
            int reading1 = Math.Min(literals.Length, allReadings.Length),
                reading2 = Math.Max(literals.Length, allReadings.Length);

            yield return new Series(
                group.Key,
                (int)(kCoverage * 100),
                (int)(kCoverage * (reading1 / (float)reading2 * 100)),
                literals.OrderByDescending(x => charByKanjiString[x].Frequency).ToArray(),
                groupedReadings
            );
        }
    }
}