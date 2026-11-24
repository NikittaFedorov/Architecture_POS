namespace POS.Domain;

public sealed class Evaluation
{
    private readonly Criterion _criterion;

    public Guid StudentId { get; }
    public Guid TeacherId { get; }
    public Guid CriterionId => _criterion.Id;
    public Score Score { get; private set; }

    public Evaluation(Student student, Teacher teacher, Criterion criterion, Score score)
    {
        ArgumentNullException.ThrowIfNull(student);
        ArgumentNullException.ThrowIfNull(teacher);
        ArgumentNullException.ThrowIfNull(criterion);

        StudentId = student.Id;
        TeacherId = teacher.Id;
        _criterion = criterion;
        CorrectScore(score);
    }

    public void CorrectScore(Score score)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            score.Value, _criterion.MaxScore, nameof(score));
        Score = score;
    }
}
