using System.Net;
using Interview.Backend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Interview.Backend;

/// <summary>
/// DefaultResponseCodesFilter.
/// </summary>
public class DefaultResponseCodesFilter : IOperationFilter
{
    private readonly Dictionary<string, OpenApiResponse> _defaultResponses = new([
        CreateMessageResponseOpenApiResponse(HttpStatusCode.InternalServerError, "Error content"),
        CreateMessageResponseOpenApiResponse(HttpStatusCode.BadRequest, "Error content"),
        CreateMessageResponseOpenApiResponse(HttpStatusCode.TooManyRequests, "Too many requests")
    ]);

    private readonly Dictionary<string, OpenApiResponse> _authResponses = new([
        CreateMessageResponseOpenApiResponse(HttpStatusCode.Unauthorized, "Unauthorized"),
        CreateMessageResponseOpenApiResponse(HttpStatusCode.Forbidden, "Forbidden")
    ]);

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        AddResponsesIfNeeded(_defaultResponses, operation);

        var attribute = context.MethodInfo?.CustomAttributes.FirstOrDefault(e => e.AttributeType == typeof(AuthorizeAttribute));
        if (attribute is not null)
        {
            AddResponsesIfNeeded(_authResponses, operation);
        }
    }

    private static KeyValuePair<string, OpenApiResponse> CreateMessageResponseOpenApiResponse(HttpStatusCode statusCode, string exampleMessage)
    {
        var response = new OpenApiResponse
        {
            Description = System.Text.RegularExpressions.Regex.Replace(statusCode.ToString(), "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim(),
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Reference = new OpenApiReference
                        {
                            Id = typeof(MessageResponse).FullName,
                            Type = ReferenceType.Schema,
                        },
                        Example = new OpenApiObject
                        {
                            [nameof(MessageResponse.Message)] = new OpenApiString(exampleMessage),
                        },
                    },
                },
            },
        };

        var strCode = ((int)statusCode).ToString();
        return new KeyValuePair<string, OpenApiResponse>(strCode, response);
    }

    private void AddResponsesIfNeeded(Dictionary<string, OpenApiResponse> addResponses, OpenApiOperation operation)
    {
        foreach (var (statusCode, value) in addResponses)
        {
            if (!operation.Responses.ContainsKey(statusCode))
            {
                operation.Responses.TryAdd(statusCode, value);
            }
        }
    }
}
