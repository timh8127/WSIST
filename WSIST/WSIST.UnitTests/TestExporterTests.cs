using NUnit.Framework;
using WSIST.Engine;

namespace WSIST.UnitTests;

public class TestExporterTests
{
    [Test]
    public void ToCsv_WritesHeaderAndFormatsValues()
    {
        var rows = new List<TestExportRow>
        {
            new("Exam", "Math", new DateOnly(2026, 03, 01), "High", "Low", 5.5),
        };

        var csv = TestExporter.ToCsv(rows);

        Assert.That(csv, Does.StartWith("Title,Subject,DueDate,Volume,Understanding,Grade"));
        Assert.That(csv, Does.Contain("2026-03-01"));
        Assert.That(csv, Does.Contain("5.5"));
    }

    [Test]
    public void ToCsv_EscapesDelimitersAndQuotes()
    {
        var rows = new List<TestExportRow>
        {
            new("Title, comma", "Sub \"quote\"", new DateOnly(2026, 03, 01), "High", "Low", null),
        };

        var csv = TestExporter.ToCsv(rows);

        Assert.That(csv, Does.Contain("\"Title, comma\""));
        Assert.That(csv, Does.Contain("\"Sub \"\"quote\"\"\""));
    }

    [Test]
    public void ToCsv_MitigatesFormulaInjection()
    {
        var rows = new List<TestExportRow>
        {
            new("=SUM(A1:A2)", "Math", new DateOnly(2026, 03, 01), "High", "Low", null),
        };

        var csv = TestExporter.ToCsv(rows);

        //a leading '=' must be neutralised so spreadsheets treat it as text
        Assert.That(csv, Does.Contain("'=SUM(A1:A2)"));
    }
}
