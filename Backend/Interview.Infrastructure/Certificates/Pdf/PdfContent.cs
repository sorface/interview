using Interview.Domain.Certificates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Interview.Infrastructure.Certificates.Pdf;

internal class PdfContent : IComponent
{
    private readonly CertificateDetail _detail;

    public PdfContent(CertificateDetail detail)
    {
        _detail = detail;
    }

    public void Compose(IContainer container)
    {
        container
            .Column(column =>
            {
                column.Spacing(0.1f, Unit.Inch);

                column.Item().Text(txt => txt.EmptyLine());

                column.Item().Text(text =>
                {
                    text.Span("Result: ");
                    text.Span(_detail.Grade.Name).Underline();
                });

                column.Item().Text(_detail.Description);
            });
    }
}
