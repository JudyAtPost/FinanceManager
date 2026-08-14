using System.Text.Json.Serialization;
using PersonalFinance.Api;
using PersonalFinance.Api.Endpoints;
using PersonalFinance.Application;
using PersonalFinance.Infrastructure;
using PersonalFinance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevCorsPolicy = "angular-dev";

string connectionString = builder.Configuration.GetConnectionString("PersonalFinance")
	?? throw new InvalidOperationException("Connection string 'PersonalFinance' is not configured.");

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

builder.Services.ConfigureHttpJsonOptions(options =>
	options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
	?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
	options.AddPolicy(AngularDevCorsPolicy, policy => policy
		.WithOrigins(allowedOrigins)
		.AllowAnyHeader()
		.AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors(AngularDevCorsPolicy);

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Personal Finance API"));

app.MapCategoryEndpoints();
app.MapTransactionEndpoints();
app.MapBudgetEndpoints();
app.MapSummaryEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
	.WithTags("Health")
	.WithName("GetHealth");

await DatabaseInitializer.InitializeAsync(app.Services);

await app.RunAsync();
