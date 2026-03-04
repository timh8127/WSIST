using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace WSIST.Engine;

public class TestManagement
{
    private Database database;
    

    public TestManagement(Database database)
    {
        database.Query("SELECT * FROM Tests WHERE Id = @Id", new Dictionary<string, object>
        {
            { "Id", 123 }
        });
    }
    
    //TODO: Global
    // - Remove all mentions of the List test and load tests individually
    // - Rewrite Save and Load methods
    // - Figure out how Tests now need to be saved...
    
    public void NewTestMaker(
        string title,
        Test.Subjects subject,
        DateOnly dueDate,
        Test.TestVolume volume,
        Test.PersonalUnderstanding understanding,
        double? grade
    )
    {
        Test newTest = new()
        {
            Title = title,
            Subject = subject,
            DueDate = dueDate,
            Volume = volume,
            Understanding = understanding,
            Grade = grade,
        };
        TestAssistants.GradeVerifier(dueDate, grade);
        SaveTests(subject, title, dueDate, volume, understanding, grade);
    }

    public void TestEditor(int id, string title, Test.Subjects subject, DateOnly dueDate,
        Test.TestVolume volume, Test.PersonalUnderstanding understanding, double? grade)
    {
        var verifiedGrade = TestAssistants.GradeVerifier(dueDate, grade);
        database.Query(
            "UPDATE Tests SET Title=@Title, Subject=@Subject, DueDate=@DueDate, " +
            "Volume=@Volume, Understanding=@Understanding, Grade=@Grade WHERE Id=@Id;",
            new Dictionary<string, object>
            {
                { "Id", id },
                { "Title", title },
                { "Subject", (int)subject },
                { "DueDate", dueDate.ToDateTime(TimeOnly.MinValue) },
                { "Volume", (int)volume },
                { "Understanding", (int)understanding },
                { "Grade", (object?)verifiedGrade ?? DBNull.Value }
            }
        );
    }

    public void TestRemover(int id)
    {
        var dataTable = database.Query(
            "DELETE FROM Tests WHERE Id = @Id;",
            new Dictionary<string, object>
            {
                { "id", id }
            }
        );
    }

    private void SaveTests(Test.Subjects subject, string title, DateOnly dueDate,
        Test.TestVolume volume, Test.PersonalUnderstanding understanding, double? grade)
    {
        database.Query(
            "INSERT INTO Tests (Title, Subject, DueDate, Volume, Understanding, Grade) " +
            "VALUES (@Title, @Subject, @DueDate, @Volume, @Understanding, @Grade);",
            new Dictionary<string, object>
            {
                { "Title", title },
                { "Subject", (int)subject },
                { "DueDate", dueDate.ToDateTime(TimeOnly.MinValue) },
                { "Volume", (int)volume },
                { "Understanding", (int)understanding },
                { "Grade", (object?)grade ?? DBNull.Value }
            }
        );
    }

    public List<Test> LoadAllTests()
    {
        var dataTable = database.Query(
            "SELECT Id, Title, Subject, DueDate, Volume, Understanding, Grade FROM Tests;"
        );

        var tests = new List<Test>();
        foreach (DataRow row in dataTable.Rows)
        {
            tests.Add(new Test
            {
                Id            = (int)row["Id"],
                Title         = row["Title"].ToString()!,
                Subject       = (Test.Subjects)(int)row["Subject"],
                DueDate       = DateOnly.FromDateTime((DateTime)row["DueDate"]),
                Volume        = (Test.TestVolume)(int)row["Volume"],
                Understanding = (Test.PersonalUnderstanding)(int)row["Understanding"],
                Grade         = row["Grade"] == DBNull.Value ? null : (double?)row["Grade"],
            });
        }
        return tests;
    }
    
    private Test LoadTest(int id)
    {
        var dataTable = database.Query(
            "SELECT Id, Title, Subject, DueDate, Volume, Understanding, Grade FROM Tests WHERE Id = @Id;",
            new Dictionary<string, object> { { "Id", id } }
        );

        if (dataTable.Rows.Count == 0)
            throw new KeyNotFoundException($"Test with Id {id} not found.");

        var row = dataTable.Rows[0];
        return new Test
        {
            Id            = (int)row["Id"],
            Title         = row["Title"].ToString()!,
            Subject       = (Test.Subjects)(int)row["Subject"],
            DueDate       = DateOnly.FromDateTime((DateTime)row["DueDate"]),
            Volume        = (Test.TestVolume)(int)row["Volume"],
            Understanding = (Test.PersonalUnderstanding)(int)row["Understanding"],
            Grade         = row["Grade"] == DBNull.Value ? null : (double?)row["Grade"],
        };
    }

    public List<Test> Refresh()
    {
        return LoadAllTests();
    }
}
