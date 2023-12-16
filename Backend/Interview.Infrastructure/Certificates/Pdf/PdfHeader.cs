using Interview.Domain.Certificates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Interview.Infrastructure.Certificates.Pdf;

internal class PdfHeader : IComponent
{
    private readonly PdfCertificateGenerator.Settings _currentSettings;
    private readonly CertificateDetail _detail;

    public PdfHeader(CertificateDetail detail, PdfCertificateGenerator.Settings currentSettings)
    {
        _detail = detail;
        _currentSettings = currentSettings;
    }

    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text(txt =>
            {
                txt.Line("Certificate").FontSize(_currentSettings.HeaderSize);
                txt.AlignCenter();
            });

            col.Item().Text(txt =>
            {
                txt.Span("Issued: ").FontSize(_currentSettings.PersonWhomCertificateSize);
                txt.Span(_detail.CandidateFullName).FontSize(_currentSettings.PersonWhomCertificateSize).Underline();
                txt.AlignLeft();
            });
        });
    }
}
