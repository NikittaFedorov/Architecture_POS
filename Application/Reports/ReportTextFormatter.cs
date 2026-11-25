using System.Globalization;
using POS.Domain;

namespace POS.Application.Reports;

public static class ReportTextFormatter
{
    public static void Write(AssessmentReport report, TextWriter output)
    {
        var students = report.Students.ToDictionary(student => student.Number);
        var teachers = report.Teachers.ToDictionary(teacher => teacher.Number);
        var criteria = report.Rubric.ToDictionary(criterion => criterion.Number);
        output.WriteLine(report.GroupBy switch
        {
            ReportGrouping.Teacher => "Отчёт по преподавателям",
            ReportGrouping.Criterion => "Отчёт по критериям",
            _ => "Отчёт по студентам"
        });
        output.WriteLine(report.AverageMethod == AverageMethod.Weighted
            ? "Взвешенное среднее."
            : "Обычное среднее.");

        foreach (var group in report.Groups)
        {
            output.WriteLine();
            output.WriteLine(report.GroupBy switch
            {
                ReportGrouping.Teacher => $"Преподаватель №{group.Key} {teachers[group.Key].Name}",
                ReportGrouping.Criterion => $"Критерий №{group.Key} {criteria[group.Key].Name}",
                _ => $"Студент №{group.Key} {students[group.Key].Name}"
            });

            switch (report.GroupBy)
            {
                case ReportGrouping.Teacher:
                    WriteTeacherDetails(group);
                    break;
                case ReportGrouping.Criterion:
                    WriteCriterionDetails(group);
                    break;
                default:
                    WriteStudentDetails(group);
                    break;
            }

            WriteTotal(group, report.GroupBy switch
            {
                ReportGrouping.Teacher => "Итог преподавателя по студентам",
                ReportGrouping.Criterion => "Итог критерия по преподавателям",
                _ => "Итог студента по критериям"
            });
        }

        void WriteTeacherDetails(TableGroup<ReportScoreRow> teacher)
        {
            foreach (var student in teacher.Children)
            {
                output.WriteLine($"  {StudentLabel(student.Key)}");
                foreach (var criterion in student.Children)
                {
                    var entry = criteria[criterion.Key];
                    string label = $"    №{entry.Number} {entry.Name}";
                    if (criterion.Average is not decimal score)
                    {
                        output.WriteLine($"{label} | балл -");
                        continue;
                    }

                    WriteScore(score, label, null, $" | максимум {Format(entry.MaxScore)}"
                        + WeightLabel("вес критерия", criterion.AppliedWeight));
                }
                WriteTotal(student, "  Итог студента по критериям");
            }
        }

        void WriteCriterionDetails(TableGroup<ReportScoreRow> criterion)
        {
            WriteCriterion(criterion);
            foreach (var teacher in criterion.Children)
            {
                output.WriteLine($"  {TeacherLabel(teacher.Key)}");
                foreach (var student in teacher.Children)
                {
                    foreach (var score in student.Rows)
                        WriteScore(score.Score, $"    {StudentLabel(student.Key)}", teacher.AppliedWeight);
                }
                WriteTotal(teacher, "  Итог преподавателя по студентам");
            }
        }

        void WriteStudentDetails(TableGroup<ReportScoreRow> student)
        {
            foreach (var criterion in student.Children)
            {
                WriteCriterion(criterion);
                foreach (var teacher in criterion.Children)
                {
                    foreach (var score in teacher.Rows)
                        WriteScore(score.Score, $"  {TeacherLabel(teacher.Key)}", teacher.AppliedWeight);
                }
                WriteTotal(criterion, "  Итог критерия по преподавателям");
            }
        }

        void WriteTotal(TableGroup<ReportScoreRow> group, string label)
        {
            string result = group.Total is decimal total
                ? total.ToString("0.####%", CultureInfo.InvariantCulture)
                : "пока нет оценок";
            output.WriteLine($"{label} - {result}.");
        }

        string TeacherLabel(int number) => $"Преподаватель №{number} {teachers[number].Name}";
        string StudentLabel(int number) => $"Студент №{number} {students[number].Name}";

        void WriteCriterion(TableGroup<ReportScoreRow> group)
        {
            var criterion = criteria[group.Key];
            output.WriteLine($"№{criterion.Number} {criterion.Name} | максимум {Format(criterion.MaxScore)}"
                + $" | среднее {Format(group.Average)} | мин. {Format(group.Minimum)}"
                + $" | макс. {Format(group.Maximum)} | оценок {group.Rows.Count}"
                + WeightLabel("вес критерия", group.AppliedWeight));
        }

        void WriteScore(decimal score, string label, decimal? weight, string details = "") =>
            output.WriteLine($"{label} | балл {Format(score)}" + WeightLabel("вес оценки", weight) + details);
    }

    private static string WeightLabel(string label, decimal? weight) =>
        weight is decimal value ? $" | {label} {value.ToString(CultureInfo.InvariantCulture)}" : "";

    private static string Format(decimal? value) => value?.ToString("0.####", CultureInfo.InvariantCulture) ?? "-";
}

