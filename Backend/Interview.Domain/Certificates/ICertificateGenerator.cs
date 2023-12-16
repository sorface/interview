namespace Interview.Domain.Certificates;

public interface ICertificateGenerator
{
    Task<Stream> GenerateAsync(CertificateDetail detail, CancellationToken cancellationToken = default);
}
