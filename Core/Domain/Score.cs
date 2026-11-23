namespace Architecture_1.Domain;

public readonly record struct Score
{
    public decimal Value { get; }

    public Score(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        if (decimal.Round(value, 1) != value)
            throw new ArgumentOutOfRangeException(nameof(value), "Балл можно указать только с точностью до десятых.");

        Value = value;
    }
}
