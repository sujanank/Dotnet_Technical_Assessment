using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenSenseMapAPI.Filters;
public class AuthorizationHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add Bearer token security requirement specifically for logout endpoint
        if (context.MethodInfo?.Name == "Logout" && context.MethodInfo.DeclaringType?.Name == "UsersController")
        {
            operation.Security = new List<OpenApiSecurityRequirement>
             {
                 new OpenApiSecurityRequirement
                 {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                             }
                         },
                         new string[] { }
                     }
                 }
             };
        }
    }
}