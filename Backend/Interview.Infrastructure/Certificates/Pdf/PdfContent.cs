using Interview.Domain.Certificates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Interview.Infrastructure.Certificates.Pdf;

internal class PdfContent(CertificateDetail detail) : IComponent
{
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
                    text.Span(detail.Grade.Name).Underline();
                });

                column.Item().Text(detail.Description);
            });
    }
}
