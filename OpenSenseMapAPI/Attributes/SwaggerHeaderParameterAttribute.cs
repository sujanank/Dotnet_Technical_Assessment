using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenSenseMapAPI.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class SwaggerHeaderParameterAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public bool Required { get; }
    public string? Example { get; }

    public SwaggerHeaderParameterAttribute(string name, string description = "", bool required = false, string? example = null)
    {
        Name = name;
        Description = description;
        Required = required;
        Example = example;
    }
}

public class SwaggerHeaderParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attributes = context.MethodInfo?.GetCustomAttributes(typeof(SwaggerHeaderParameterAttribute), false)
            .Cast<SwaggerHeaderParameterAttribute>()
            .ToList();

        if (attributes != null && attributes.Any())
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            foreach (var attribute in attributes)
            {
                var existingParam = operation.Parameters.FirstOrDefault(p => 
                    p.Name.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase));

                if (existingParam == null)
                {
                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = attribute.Name,
                        In = ParameterLocation.Header,
                        Description = attribute.Description,
                        Required = attribute.Required,
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Example = attribute.Example != null 
                                ? new Microsoft.OpenApi.Any.OpenApiString(attribute.Example)
                                : null
                        }
                    });
                }
            }
        }
    }
}
