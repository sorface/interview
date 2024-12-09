using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Interview.Backend.Swagger
{
    public class SwaggerDocumentFilter(string prefix) : IDocumentFilter
    {
        private readonly string _prefix = prefix.StartsWith('/') ? prefix : "/" + prefix;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Url = _prefix,
            });
        }
    }
}
