using System.Text.Json.Serialization;
using Hplc.Controller.Api.Services;

var builder = WebApplication.CreateBuilder(args);

/* =========================
   Services
   ========================= */

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ CRITICAL FIX:
        // Serialize enums as strings instead of numbers
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Your services
builder.Services.AddSingleton<BatchFileService>();

// CORS (if you already had this, keep rules as-is)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var app = builder.Build();

/* =========================
   Middleware
   ========================= */

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();
app.MapControllers();

app.Run();
