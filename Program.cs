using ChatbotApi.Data;
using ChatbotApi.Services;
using ChatbotApi.Middleware;
using ChatbotApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers with FluentValidation
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ChatRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Chatbot API", Version = "v1" });
});

// HttpClient for Gemini API
builder.Services.AddHttpClient<IGeminiApiService, GeminiApiService>();

// Services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IGeminiApiService, GeminiApiService>();

// CORS for React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
                         .AllowAnyMethod()
                         .AllowAnyHeader()
                         .AllowCredentials());
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// CORS must be before authentication and authorization
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();