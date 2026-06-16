using System.Globalization;
using System.Text;

namespace WSIST.Engine;

/// <summary>
/// One row of a user's data export. Holds typed values so JSON keeps real
/// types (ISO date, numeric grade) while CSV formats them itself.
/// </summary>
public record TestExportRow(
    string Title,
    string Subject,
    DateOnly DueDate,
    string Volume,
    string Understanding,
    double? Grade
);

public static class TestExporter
{
    private static readonly string[] Header =
    [
        "Title",
        "Subject",
        "DueDate",
        "Volume",
        "Understanding",
        "Grade",
    ];

    /// <summary>
    /// Serialises export rows to RFC 4180 CSV (CRLF line endings, quoted
    /// fields where needed) with formula-injection mitigation on the free-text
    /// columns.
    /// </summary>
    public static string ToCsv(IReadOnlyList<TestExportRow> rows)
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(',', Header.Select(Field))).Append("\r\n");

        foreach (var r in rows)
        {
            sb.Append(Field(r.Title))
                .Append(',')
                .Append(Field(r.Subject))
                .Append(',')
                .Append(r.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(Field(r.Volume))
                .Append(',')
                .Append(Field(r.Understanding))
                .Append(',')
                .Append(r.Grade?.ToString(CultureInfo.InvariantCulture) ?? "")
                .Append("\r\n");
        }

        return sb.ToString();
    }

    private static string Field(string value)
    {
        var v = value ?? "";

        // Formula-injection mitigation: a cell beginning with =, +, -, @, or a
        // control character can be executed by spreadsheet apps. Prefix with a
        // single quote so it is treated as literal text. Titles and custom
        // subject names are user-controlled, so this matters.
        if (v.Length > 0 && v[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            v = "'" + v;

        // RFC 4180: quote fields containing the delimiter, a quote, or a newline,
        // doubling any embedded quotes.
        if (v.Contains('"') || v.Contains(',') || v.Contains('\n') || v.Contains('\r'))
            v = "\"" + v.Replace("\"", "\"\"") + "\"";

        return v;
    }
}
