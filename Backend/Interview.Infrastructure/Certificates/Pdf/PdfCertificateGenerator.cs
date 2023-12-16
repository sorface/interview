using Interview.Domain.Certificates;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Interview.Infrastructure.Certificates.Pdf;

public sealed class PdfCertificateGenerator : ICertificateGenerator
{
    public Settings CurrentSettings { get; } = new();

    public Task<Stream> GenerateAsync(CertificateDetail detail, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(CurrentSettings.TextSize));
                page.Background().Border(0.4f, Unit.Inch);
                page.Margin(1, Unit.Inch);
                page.Header().Component(new PdfHeader(detail, CurrentSettings));
                page.Content().Component(new PdfContent(detail));
                page.Footer().Component(new PdfFooter(detail));
            });
        });

        return Task.FromResult(AsStream(document));
    }

    private static Stream AsStream(Document document)
    {
        var stream = new MemoryStream();
        document.GeneratePdf(stream);
        stream.Position = 0;
        return stream;
    }

    public sealed class Settings
    {
        private const int DefaultHeaderSize = 36;

        public int TextSize { get; set; } = 18;

        public int HeaderSize { get; set; } = DefaultHeaderSize;

        public int PersonWhomCertificateSize { get; set; } = DefaultHeaderSize - 8;
    }
}
