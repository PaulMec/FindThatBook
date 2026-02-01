using FindThatBook.Application.Interfaces;
using FindThatBook.Application.UseCases;
using FindThatBook.Infrastructure.AI;
using FindThatBook.Infrastructure.Matching;
using FindThatBook.Infrastructure.OpenLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Find That Book API",
        Version = "v1",
        Description = "A book discovery API that uses AI to find books from messy queries"
    });
});

// Configure Options
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection(GeminiOptions.SectionName));

builder.Services.Configure<OpenLibraryOptions>(
    builder.Configuration.GetSection(OpenLibraryOptions.SectionName));

// Register HttpClients
builder.Services.AddHttpClient<IAIFieldExtractor, GeminiAIProvider>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "FindThatBook/1.0");
});

builder.Services.AddHttpClient<IOpenLibraryClient, OpenLibraryClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "FindThatBook/1.0");
});

// Register Services
builder.Services.AddScoped<IBookMatcher, BookMatcher>();
builder.Services.AddScoped<IBookRanker, BookRanker>();
builder.Services.AddScoped<SearchBooksUseCase>();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Find That Book API v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();