namespace Architecture_1.Domain;

public sealed class Student
{
    public Guid Id { get; }
    public string Name { get; }

    public Student(Guid id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }
}
