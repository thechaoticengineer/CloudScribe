using CloudScribe.Notes.API.Api.Endpoints;
using CloudScribe.Notes.API.ExceptionHandling;
using CloudScribe.Notes.API.Extensions;
using CloudScribe.Notes.API.Infrastructure.Data;
using CloudScribe.Notes.API.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<CloudScribeDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<NotesService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
    app.MapOpenApi();
}

app.MapNotesEndpoints();
app.UseHttpsRedirection();
app.Run();