namespace WSIST.Engine;

public class TestManagement
{
    private readonly WsistContext _context;

    public TestManagement(WsistContext context)
    {
        _context = context;
    }

    public List<Test> Tests => _context.Tests.ToList();

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
        _context.Tests.Add(test);
        _context.SaveChanges();
    }

    public void TestEditor(Guid id, string title, Test.Subjects subject, DateOnly dueDate,
        Test.TestVolume volume, Test.PersonalUnderstanding understanding, double? grade)
    {
        var test = _context.Tests.Find(id);
        if (test is null) return;

        test.Title = title;
        test.Subject = subject;
        test.DueDate = dueDate;
        test.Volume = volume;
        test.Understanding = understanding;
        test.Grade = TestAssistants.GradeVerifier(dueDate, grade);
        _context.SaveChanges();
    }

    public void TestRemover(Guid id)
    {
        var test = _context.Tests.Find(id);
        if (test is null) return;
        _context.Tests.Remove(test);
        _context.SaveChanges();
    }
}