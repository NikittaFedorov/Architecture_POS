namespace Architecture_1.Domain;

public readonly record struct Score
{
    public decimal Value { get; }

    public Score(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }
}
