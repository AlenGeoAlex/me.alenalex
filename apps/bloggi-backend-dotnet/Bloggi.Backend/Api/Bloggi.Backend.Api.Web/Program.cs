using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bloggi.Backend.Api.Web.Attributes;
using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.Api.Web.Extensions;
using Bloggi.Backend.Api.Web.Features.Glossary;
using Bloggi.Backend.Api.Web.Features.Post;
using Bloggi.Backend.Api.Web.Features.User;
using Bloggi.Backend.Api.Web.Infrastructure;
using ErrorOr;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);
var configurationManager = builder.InitializeConfiguration();
var loggerFactory = builder.InitializeLogger();
builder.Services.AddFastEndpoints()
    .SwaggerDocument(op =>
    {
        op.MaxEndpointVersion = 1;
        op.DocumentSettings = settings =>
        {
            settings.OperationProcessors.Add(new QueryEnumOperationProcessor());
        };
    });
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddExceptionHandler<ResponseExceptionHandler>();
builder.Services.AddProblemDetails();
builder.UseBloggiDatabase(loggerFactory);
builder.Services.AddGlossaryModule(configurationManager);
builder.Services.AddPostModule(configurationManager);
builder.Services.AddUserModule(configurationManager);
builder.Services.AddContext();
builder.Services.AddPipelineServices();
builder.ConfigureTokenSecrets();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c =>
{
    c.Errors.UseProblemDetails();
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    c.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    c.Endpoints.Configurator = (ep) =>
    {
        if (!ep.ResDtoType.IsAssignableTo(typeof(IErrorOr))) return;
        ep.DontAutoSendResponse();
        ep.PostProcessor<ResponseSender>(Order.After);
    };
});
app.UseSwaggerGen();
app.UseExceptionHandler();
await app.RunAsync();