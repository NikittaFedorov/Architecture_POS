namespace POS.Domain;

public readonly record struct WeightedValue(decimal Value, decimal Weight);

public interface IAverageStrategy
{
    decimal Calculate(IReadOnlyList<WeightedValue> values);
}

public sealed class SimpleAverage : IAverageStrategy
{
    public decimal Calculate(IReadOnlyList<WeightedValue> values) =>
        WeightedAverage.Calculate(values, entry => entry.Value, _ => 1m);
}

public sealed class WeightedAverageStrategy : IAverageStrategy
{
    public decimal Calculate(IReadOnlyList<WeightedValue> values) =>
        WeightedAverage.Calculate(values, entry => entry.Value, entry => entry.Weight);
}

public sealed record TableAxis<TRow>(
    Func<TRow, int> GetGroupNumber, IReadOnlyDictionary<int, decimal?> Members, bool IncludeEmpty = false);

public sealed record TableGroup<TRow>(
    int Key, decimal? AppliedWeight, IReadOnlyList<TRow> Rows,
    IReadOnlyList<TableGroup<TRow>> Children, decimal? Average, decimal? Total,
    decimal? Minimum, decimal? Maximum);

public sealed class ResultCalculator(IAverageStrategy average)
{
    public IReadOnlyList<TableGroup<TRow>> Calculate<TRow>(IEnumerable<TRow> rows,
        IReadOnlyList<TableAxis<TRow>> axes, Func<TRow, decimal> getRawValue, Func<TRow, decimal> getNormalizedValue)
    {
        if (axes.Count == 0)
            throw new ArgumentException("At least one grouping axis is required.", nameof(axes));

        var matchingRows = rows.Where(row =>
            axes.All(axis => axis.Members.ContainsKey(axis.GetGroupNumber(row))));
        return GroupRows(matchingRows, level: 0);

        TableGroup<TRow>[] GroupRows(IEnumerable<TRow> source, int level)
        {
            var axis = axes[level];
            var rowsByKey = source.ToLookup(axis.GetGroupNumber);
            var keys = axis.IncludeEmpty
                ? axis.Members.Keys
                : rowsByKey.Select(group => group.Key);

            var groups = new List<TableGroup<TRow>>();
            foreach (int key in keys.Order())
            {
                var groupRows = rowsByKey[key].ToArray();
                decimal? weight = null;
                if (level > 0 && groupRows.Length > 0)
                    weight = axis.Members[key];

                if (level + 1 == axes.Count)
                {
                    groups.Add(CalculateLeaf(key, weight, groupRows));
                    continue;
                }

                var children = GroupRows(groupRows, level + 1);
                groups.Add(CombineChildren(key, weight, groupRows, children));
            }

            return groups.ToArray();
        }

        TableGroup<TRow> CalculateLeaf(int key, decimal? weight, IReadOnlyList<TRow> groupRows)
        {
            if (groupRows.Count == 0)
                return new TableGroup<TRow>(key, weight, groupRows, [], null, null, null, null);

            var rawValues = groupRows.Select(getRawValue).ToArray();
            var rawMean = Mean(rawValues.Select(score => new WeightedValue(score, 1m)));
            var normalizedMean = Mean(groupRows.Select(row => new WeightedValue(getNormalizedValue(row), 1m)));
            return new TableGroup<TRow>(key, weight, groupRows, [],
                rawMean, normalizedMean, rawValues.Min(), rawValues.Max());
        }
    }

    private TableGroup<TRow> CombineChildren<TRow>(
        int key, decimal? weight, IReadOnlyList<TRow> rows, IReadOnlyList<TableGroup<TRow>> children)
    {
        var assessed = children.Where(child => child.Average.HasValue).ToArray();
        if (assessed.Length == 0)
            return new TableGroup<TRow>(key, weight, rows, children, null, null, null, null);

        // A child's weight is used here, when its result is averaged with its siblings.
        var rawMean = Mean(assessed.Select(child =>
            new WeightedValue(child.Average!.Value, child.AppliedWeight ?? 1m)));
        var normalizedMean = Mean(assessed.Select(child =>
            new WeightedValue(child.Total!.Value, child.AppliedWeight ?? 1m)));

        return new TableGroup<TRow>(key, weight, rows, children,
            rawMean, normalizedMean, assessed.Min(child => child.Minimum), assessed.Max(child => child.Maximum));
    }

    private decimal? Mean(IEnumerable<WeightedValue> values)
    {
        var entries = values.ToArray();
        return entries.Length == 0 ? null : average.Calculate(entries);
    }
}

internal static class WeightedAverage
{
    public static decimal Calculate<T>(
        IReadOnlyList<T> entries, Func<T, decimal> getEntryValue, Func<T, decimal> getEntryWeight)
    {
        decimal largestWeight = entries.Max(getEntryWeight);
        try
        {
            return Average(entries, getEntryValue, getEntryWeight, Math.Min(1m, largestWeight), 1m);
        }
        catch (OverflowException)
        {
            decimal largestValue = entries.Max(getEntryValue);
            if (largestValue == 0m)
                return 0m;

            return Math.Min(1m, Average(entries, getEntryValue, getEntryWeight, largestWeight, largestValue)) * largestValue;
        }
    }

    private static decimal Average<T>(
        IReadOnlyList<T> entries, Func<T, decimal> getEntryValue, Func<T, decimal> getEntryWeight,
        decimal weightScale, decimal valueScale)
    {
        decimal sum = 0m;
        decimal totalWeight = 0m;
        foreach (var entry in entries)
        {
            decimal scaledWeight = getEntryWeight(entry) / weightScale;
            sum += getEntryValue(entry) / valueScale * scaledWeight;
            totalWeight += scaledWeight;
        }

        return sum / totalWeight;
    }
}
