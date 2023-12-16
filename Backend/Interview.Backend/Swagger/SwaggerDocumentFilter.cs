using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Interview.Backend.Swagger
{
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        private readonly string _prefix;

        public SwaggerDocumentFilter(string prefix)
        {
            _prefix = prefix.StartsWith('/') ? prefix : "/" + prefix;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Url = _prefix,
            });
        }
    }
}
