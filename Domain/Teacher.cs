namespace Architecture_1.Domain;

public sealed class Teacher
{
    public Guid Id { get; }
    public string Name { get; }
    public decimal Weight { get; }

    public Teacher(Guid id, string name, decimal weight = 1m)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weight);

        Id = id;
        Name = name;
        Weight = weight;
    }
}
