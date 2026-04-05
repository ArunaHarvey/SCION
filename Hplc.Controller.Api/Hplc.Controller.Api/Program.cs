using Hplc.Controller.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add controllers
builder.Services.AddControllers();

// ✅ Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register application services
builder.Services.AddScoped<BatchFileService>();

// ✅ CORS (for Angular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// ✅ Enable Swagger ONLY in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HPLC Controller API v1");
        c.RoutePrefix = "swagger"; // default
    });
}

app.UseCors("AllowAngular");

app.UseAuthorization();
app.MapControllers();

app.Run();
