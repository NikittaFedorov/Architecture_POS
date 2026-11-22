namespace Architecture_1.Domain;

public sealed class Criterion
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal MaxScore { get; }
    public decimal Weight { get; }

    public Criterion(Guid id, string name, decimal maxScore, decimal weight)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxScore);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weight);

        Id = id;
        Name = name;
        MaxScore = maxScore;
        Weight = weight;
    }
}
