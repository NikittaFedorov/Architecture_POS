using POS.Domain;

namespace POS.Application;

internal sealed record AssessmentScope(Numbered<Teacher>? Teacher, Numbered<Student>? Student)
{
    public int? TeacherNumber => Teacher?.Number;
    public int? StudentNumber => Student?.Number;

    public bool CanSelectTeacher => Teacher == null;
    public bool CanSelectStudent => Teacher != null && Student == null;
    public bool CanGoBack => Teacher != null;

    public void EnsureStudentAllowed(int number) => EnsureAllowed(StudentNumber, number, "студента");
    public void EnsureTeacherAllowed(int number) => EnsureAllowed(TeacherNumber, number, "преподавателя");

    private static void EnsureAllowed(int? selectedNumber, int requestedNumber, string role)
    {
        if (selectedNumber is int number && number != requestedNumber)
            throw new InvalidOperationException($"Текущий режим уже ограничивает выбор {role} №{number}.");
    }
}
