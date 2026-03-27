using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.Api.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configurationManager = builder.InitializeConfiguration();
var loggerFactory = builder.InitializeLogger();
// Add services to the container.

builder.UseBloggiDatabase(loggerFactory);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();