using AiService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IAiOpdTriageService, AiOpdTriageService>();
builder.Services.AddHttpClient<IAiAnalysisService, AiAnalysisService>();

builder.Services.AddHttpClient<IDoctorScheduleClient, DoctorScheduleClient>(client =>
{
    // same placeholder as HealthCampMicroservice — point at real DoctorScheduleService once built
    client.BaseAddress = new Uri(builder.Configuration["Services:DoctorSchedule:BaseUrl"] ?? "http://localhost:5000");
});


builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Authrization button in Swagger ke liye
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HIS API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();