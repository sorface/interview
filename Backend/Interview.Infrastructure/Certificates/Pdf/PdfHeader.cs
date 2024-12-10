using Interview.Domain.Certificates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Interview.Infrastructure.Certificates.Pdf;

internal class PdfHeader(CertificateDetail detail, PdfCertificateGenerator.Settings currentSettings) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text(txt =>
            {
                txt.Line("Certificate").FontSize(currentSettings.HeaderSize);
                txt.AlignCenter();
            });

            col.Item().Text(txt =>
            {
                txt.Span("Issued: ").FontSize(currentSettings.PersonWhomCertificateSize);
                txt.Span(detail.CandidateFullName).FontSize(currentSettings.PersonWhomCertificateSize).Underline();
                txt.AlignLeft();
            });
        });
    }
}
