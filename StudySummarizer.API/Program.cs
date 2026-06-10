using Microsoft.EntityFrameworkCore;
using StudySummarizer.API.Middleware;
using StudySummarizer.Infrastructure.Data;
using StudySummarizer.Service.Interfaces;
using StudySummarizer.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

builder.Services.AddScoped<ITextExtractor, TextExtractor>();
builder.Services.AddScoped<ISummarizer, StubSummarizer>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ISummaryService, SummaryService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    )
);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
