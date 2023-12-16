namespace Interview.Domain.Certificates;

public sealed record CertificateDetail(string CandidateFullName, InterviewGrade Grade, string Description,
    string Sign = Constants.DefaultSign)
{
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.Now);
}
