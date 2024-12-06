using Interview.Domain.Certificates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Interview.Infrastructure.Certificates.Pdf;

internal class PdfFooter(CertificateDetail detail) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Row(descriptor =>
        {
            descriptor.AutoItem().AlignLeft().Text(text =>
            {
                text.Element()
                    .Border(0.01f, Unit.Inch)
                    .Column(col =>
                    {
                        col.Item().Text(txt =>
                        {
                            txt.Span("Date");
                            txt.AlignCenter();
                        });

                        col.Item()
                            .PaddingLeft(5)
                            .PaddingRight(5)
                            .Text(txt =>
                            {
                                txt.Span(detail.Date.ToString("dd-MM-yyyy"));
                                txt.AlignCenter();
                            });
                    });
            });

            descriptor.AutoItem().MinWidth(350).AlignRight().Text(detail.Sign);
        });
    }
}
