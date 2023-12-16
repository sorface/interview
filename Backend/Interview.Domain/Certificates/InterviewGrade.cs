using Ardalis.SmartEnum;

namespace Interview.Domain.Certificates;

public sealed class InterviewGrade : SmartEnum<InterviewGrade>
{
    public static readonly InterviewGrade Awful = new("Awful", 0);

    public static readonly InterviewGrade Bad = new("Bad", 1);

    public static readonly InterviewGrade NotBad = new("Not bad", 2);

    public static readonly InterviewGrade Good = new("Good", 3);

    public static readonly InterviewGrade Perfect = new("Perfect", 4);

    private InterviewGrade(string name, int value)
        : base(name, value)
    {
    }
}
