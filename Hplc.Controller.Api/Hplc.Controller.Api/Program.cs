using Hplc.Controller.Api.Controllers;
using Hplc.Controller.Api.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hplc.Controller.Api.Stores;


var builder = WebApplication.CreateBuilder(args);

/* =========================
   Services
   ========================= */


builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Your services

builder.Services.AddSingleton<ChromatogramService>();
builder.Services.AddSingleton<BatchFileService>();
builder.Services.AddSingleton<InstrumentStatusStore>();

builder.Services.Configure<ChromatogramSimulationOptions>(
    builder.Configuration.GetSection("ChromatogramSimulation"));



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
