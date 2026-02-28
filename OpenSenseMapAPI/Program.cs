using Microsoft.OpenApi.Models;
using OpenSenseMapAPI.Filters;
using OpenSenseMapAPI.Middleware;
using OpenSenseMapAPI.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "OpenSenseMap API",
        Version = "v1.0",
        Description = "Backend API services for OpenSenseMap integration - Dotnet Technical Assessment"        
    });

    options.OrderActionsBy(apiDesc =>
    {
        var controllerName = apiDesc.ActionDescriptor.RouteValues["controller"];
        var order = controllerName switch
        {
            "Users" => "1",
            "Boxes" => "2",
            _ => "3"
        };
        return $"{order}_{controllerName}_{apiDesc.RelativePath}";
    });

    // Add Bearer token authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your bearer token in the format: Bearer {your-token-here}"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add common parameters to all endpoints
    options.OperationFilter<AddCommonParametersFilter>();
    
    // Add proper Authorization header handling for logout
    options.OperationFilter<AuthorizationHeaderFilter>();    
    
});



builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IOpenSenseMapService, OpenSenseMapService>();

builder.Services.AddScoped<IOpenSenseMapService, OpenSenseMapService>();
builder.Services.AddSingleton<ITokenCacheService, TokenCacheService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenSenseMap API v1.0");
        options.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting OpenSenseMap API application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
