using System.Globalization;

namespace POS.Application.Reports;

public static class ReportTextFormatter
{
    public static void Write(AssessmentReport report, TextWriter output)
    {
        var students = report.Students.ToDictionary(student => student.Number);
        var teachers = report.Teachers.ToDictionary(teacher => teacher.Number);
        var criteria = report.Rubric.ToDictionary(criterion => criterion.Number);
        bool showsTotal = report.GroupBy is ReportGrouping.Student or ReportGrouping.Teacher;
        string scoreWeightLabel = (report.GroupBy, report.UsesTeacherWeights) switch
        {
            (ReportGrouping.Teacher, _) => "вес студента",
            (_, true) => "вес преподавателя",
            _ => "вес оценки"
        };
        output.WriteLine(report.GroupBy switch
        {
            ReportGrouping.Teacher => "Отчёт по преподавателям",
            ReportGrouping.Criterion => "Отчёт по критериям",
            _ => "Отчёт по студентам"
        });
        output.WriteLine((report.GroupBy, report.UsesTeacherWeights) switch
        {
            (_, true) => "Среднее по каждому критерию учитывает веса преподавателей, выставивших баллы.",
            (ReportGrouping.Teacher, _) => "В среднем по каждому критерию все оценённые студенты имеют одинаковый вес.",
            _ => "В среднем по каждому критерию все баллы имеют одинаковый вес."
        });
        if (showsTotal)
            output.WriteLine("Общий итог учитывает долю от максимума и вес каждого критерия с оценками.");

        foreach (var group in report.Groups)
        {
            output.WriteLine();
            output.WriteLine(report.GroupBy switch
            {
                ReportGrouping.Teacher => $"Преподаватель №{group.Number} {teachers[group.Number].Name}",
                ReportGrouping.Criterion => $"Критерий №{group.Number} {criteria[group.Number].Name}",
                _ => $"Студент №{group.Number} {students[group.Number].Name}"
            });

            string countLabel = report.GroupBy == ReportGrouping.Student ? "преподавателей" : "оценок";
            var scoresByCriterion = group.Scores.ToLookup(score => score.CriterionNumber);
            foreach (var result in group.StatisticsByCriterion)
            {
                var criterion = criteria[result.CriterionNumber];
                string weightInTotal = showsTotal && result.Average.HasValue
                    ? $" | вес критерия {criterion.Weight.ToString(CultureInfo.InvariantCulture)}"
                    : "";
                output.WriteLine($"№{criterion.Number} {criterion.Name} | максимум {Format(criterion.MaxScore)}"
                    + $" | среднее {Format(result.Average)} | мин. {Format(result.LowestScore)}"
                    + $" | макс. {Format(result.HighestScore)} | {countLabel} {result.ScoreCount}{weightInTotal}");

                foreach (var score in scoresByCriterion[result.CriterionNumber])
                {
                    string details = report.GroupBy switch
                    {
                        ReportGrouping.Teacher =>
                            $"Студент №{score.StudentNumber} {students[score.StudentNumber].Name}",
                        ReportGrouping.Criterion =>
                            $"Преподаватель №{score.TeacherNumber} {teachers[score.TeacherNumber].Name}"
                            + $" -> студент №{score.StudentNumber} {students[score.StudentNumber].Name}",
                        _ => $"Преподаватель №{score.TeacherNumber} {teachers[score.TeacherNumber].Name}"
                    };
                    decimal weight = report.UsesTeacherWeights ? teachers[score.TeacherNumber].Weight : 1m;
                    output.WriteLine($"  {details} | балл {Format(score.Score)}"
                        + $" | {scoreWeightLabel} {weight.ToString(CultureInfo.InvariantCulture)}");
                }
            }

            if (group.Total is decimal total)
            {
                var assessedNames = group.StatisticsByCriterion.Where(result => result.ScoreCount > 0)
                    .Select(result => $"«{criteria[result.CriterionNumber].Name}»");
                output.WriteLine($"Итог по критериям {string.Join(", ", assessedNames)} - "
                    + total.ToString("0.####%", CultureInfo.InvariantCulture));
            }
            else if (showsTotal)
            {
                output.WriteLine("Итог - пока нет оценок по выбранным критериям.");
            }
        }
    }

    private static string Format(decimal? value) => value?.ToString("0.####", CultureInfo.InvariantCulture) ?? "-";
}

