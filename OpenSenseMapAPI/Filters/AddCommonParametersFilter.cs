using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Xml.Linq;

namespace OpenSenseMapAPI.Filters;

public class AddCommonParametersFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        // Add x-api-version header parameter
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "x-api-version",
            In = ParameterLocation.Header,
            Description = "API version in header",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("x-api-version")
            }
        });

        // Add api-version query parameter
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "api-version",
            In = ParameterLocation.Query,
            Description = "API version in query string",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("api-version")
            }
        });
    }
}
