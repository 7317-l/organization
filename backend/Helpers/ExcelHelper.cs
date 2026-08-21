using System.Globalization;
using System.Text;
using CsvHelper;
using ClosedXML.Excel;
using PartySchoolApi.Models.DTOs;

namespace PartySchoolApi.Helpers;

/// <summary>
/// Excel/CSV文件解析工具类
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    /// 解析党员导入文件（支持.xlsx和.csv）
    /// 列顺序：姓名,手机号,密码,角色(0党员/1书记/2管理员),组织Id
    /// </summary>
    public static List<MemberImportRow> ParseMemberFile(Stream stream, string fileName)
    {
        var rows = new List<MemberImportRow>();

        if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                rows.Add(new MemberImportRow
                {
                    Name = csv.GetField(0)?.Trim() ?? string.Empty,
                    Phone = csv.GetField(1)?.Trim() ?? string.Empty,
                    Password = csv.GetField(2)?.Trim() ?? "123456",
                    Role = int.TryParse(csv.GetField(3), out var r) ? r : 0,
                    OrganizationId = int.TryParse(csv.GetField(4), out var oid) ? oid : 0
                });
            }
        }
        else // xlsx
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            for (int i = 2; i <= rowCount; i++) // 跳过表头
            {
                var row = worksheet.Row(i);
                rows.Add(new MemberImportRow
                {
                    Name = row.Cell(1).GetString().Trim(),
                    Phone = row.Cell(2).GetString().Trim(),
                    Password = row.Cell(3).GetString().Trim(),
                    Role = int.TryParse(row.Cell(4).GetString(), out var r) ? r : 0,
                    OrganizationId = int.TryParse(row.Cell(5).GetString(), out var oid) ? oid : 0
                });
            }
        }

        return rows;
    }

    /// <summary>
    /// 解析题目导入文件
    /// 列顺序：题目类型(0单选/1多选/2判断),题干,选项(JSON),正确答案,分值,分类Id
    /// </summary>
    public static List<QuestionImportRow> ParseQuestionFile(Stream stream, string fileName)
    {
        var rows = new List<QuestionImportRow>();

        if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                rows.Add(new QuestionImportRow
                {
                    QuestionType = int.TryParse(csv.GetField(0), out var t) ? t : 0,
                    Stem = csv.GetField(1)?.Trim() ?? string.Empty,
                    Options = csv.GetField(2)?.Trim() ?? "[]",
                    CorrectAnswer = csv.GetField(3)?.Trim() ?? string.Empty,
                    Score = int.TryParse(csv.GetField(4), out var s) ? s : 5,
                    CategoryId = int.TryParse(csv.GetField(5), out var cid) ? cid : (int?)null
                });
            }
        }
        else
        {
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            for (int i = 2; i <= rowCount; i++)
            {
                var row = worksheet.Row(i);
                rows.Add(new QuestionImportRow
                {
                    QuestionType = int.TryParse(row.Cell(1).GetString(), out var t) ? t : 0,
                    Stem = row.Cell(2).GetString().Trim(),
                    Options = row.Cell(3).GetString().Trim(),
                    CorrectAnswer = row.Cell(4).GetString().Trim(),
                    Score = int.TryParse(row.Cell(5).GetString(), out var s) ? s : 5,
                    CategoryId = int.TryParse(row.Cell(6).GetString(), out var cid) ? cid : (int?)null
                });
            }
        }

        return rows;
    }
}

/// <summary>党员导入行</summary>
public class MemberImportRow
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = "123456";
    public int Role { get; set; }
    public int OrganizationId { get; set; }
}

/// <summary>题目导入行</summary>
public class QuestionImportRow
{
    public int QuestionType { get; set; }
    public string Stem { get; set; } = string.Empty;
    public string Options { get; set; } = "[]";
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Score { get; set; } = 5;
    public int? CategoryId { get; set; }
}
