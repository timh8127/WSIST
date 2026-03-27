namespace WSIST.Engine;

public class TestManagement
{
    private readonly WsistContext context;

    public TestManagement(WsistContext context)
    {
        this.context = context;
    }

    public List<Test> LoadAllTests() => context.Tests.ToList();

    public void NewTestMaker(string title, Test.Subjects subject, DateOnly dueDate,
        Test.TestVolume volume, Test.PersonalUnderstanding understanding, double? grade, int userId)
    {
        var test = new Test
        {
            Id = Guid.NewGuid(),
            Title = title,
            Subject = subject,
            DueDate = dueDate,
            Volume = volume,
            Understanding = understanding,
            Grade = TestAssistants.GradeVerifier(dueDate, grade),
            UserId = userId
        };
        context.Tests.Add(test);
        context.SaveChanges();
    }

    public void TestEditor(Guid id, string title, Test.Subjects subject, DateOnly dueDate,
        Test.TestVolume volume, Test.PersonalUnderstanding understanding, double? grade)
    {
        var test = context.Tests.Find(id);
        if (test is null) return;

        test.Title = title;
        test.Subject = subject;
        test.DueDate = dueDate;
        test.Volume = volume;
        test.Understanding = understanding;
        test.Grade = TestAssistants.GradeVerifier(dueDate, grade);
        context.SaveChanges();
    }

    public void TestRemover(Guid id)
    {
        var test = context.Tests.Find(id);
        if (test is null) return;
        context.Tests.Remove(test);
        context.SaveChanges();
    }
}