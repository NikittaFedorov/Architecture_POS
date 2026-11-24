namespace POS.Domain;

public readonly record struct WeightedScore(Score Score, decimal Weight);

public sealed record CriterionStatistics(
    int CriterionNumber, int ScoreCount,
    decimal? Average, decimal? LowestScore, decimal? HighestScore);

public interface IAverageStrategy
{
    decimal Calculate(IReadOnlyList<WeightedScore> scores);
}

public sealed class SimpleAverage : IAverageStrategy
{
    public decimal Calculate(IReadOnlyList<WeightedScore> scores) =>
        WeightedAverage.Calculate(scores, entry => entry.Score.Value, _ => 1m);
}

public sealed class WeightedAverageStrategy : IAverageStrategy
{
    public decimal Calculate(IReadOnlyList<WeightedScore> scores) =>
        WeightedAverage.Calculate(scores, entry => entry.Score.Value, entry => entry.Weight);
}

public sealed class ResultCalculator(IAverageStrategy average)
{
    public CriterionStatistics CalculateCriterionStatistics(int criterionNumber, IReadOnlyList<WeightedScore> scores)
    {
        if (scores.Count == 0)
            return new CriterionStatistics(criterionNumber, 0, null, null, null);

        return new CriterionStatistics(
            criterionNumber, scores.Count,
            average.Calculate(scores), scores.Min(entry => entry.Score.Value), scores.Max(entry => entry.Score.Value));
    }

    public decimal? CalculateTotal(IReadOnlyList<CriterionStatistics> statistics, IReadOnlyDictionary<int, Criterion> criteria)
    {
        var assessed = statistics.Where(criterion => criterion.Average.HasValue).ToArray();
        if (assessed.Length == 0)
            return null;

        return WeightedAverage.Calculate(assessed,
            result => result.Average!.Value / criteria[result.CriterionNumber].MaxScore,
            result => criteria[result.CriterionNumber].Weight);
    }
}

internal static class WeightedAverage
{
    public static decimal Calculate<T>(IReadOnlyList<T> entries, Func<T, decimal> value, Func<T, decimal> weight)
    {
        decimal largestWeight = entries.Max(weight);
        try
        {
            return Average(entries, value, weight, Math.Min(1m, largestWeight), 1m);
        }
        catch (OverflowException)
        {
            decimal largestValue = entries.Max(value);
            if (largestValue == 0m)
                return 0m;

            return Math.Min(1m, Average(entries, value, weight, largestWeight, largestValue)) * largestValue;
        }
    }

    private static decimal Average<T>(
        IReadOnlyList<T> entries, Func<T, decimal> value, Func<T, decimal> weight,
        decimal weightScale, decimal valueScale)
    {
        decimal sum = 0m;
        decimal totalWeight = 0m;
        foreach (var entry in entries)
        {
            decimal scaledWeight = weight(entry) / weightScale;
            sum += value(entry) / valueScale * scaledWeight;
            totalWeight += scaledWeight;
        }

        return sum / totalWeight;
    }
}
