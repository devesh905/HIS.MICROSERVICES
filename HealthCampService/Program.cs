using HealthCampService.BackgroundJobs;
using HealthCampService.Data;
using HealthCampService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<PortalDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PortalDb")));

builder.Services.AddScoped<IHospitalQueryService, HospitalQueryService>();
builder.Services.AddScoped<IHealthCampService, HealthCampBookingService>();
builder.Services.AddScoped<HealthCampProcessingJob>();
builder.Services.AddHostedService<HealthCampProcessingHostedService>();

builder.Services.AddHttpClient<IDoctorScheduleClient, DoctorScheduleClient>(client =>
{
    // TODO: set once DoctorScheduleService exists, e.g. via Gateway or direct address
    client.BaseAddress = new Uri(builder.Configuration["Services:DoctorSchedule:BaseUrl"] ?? "http://localhost:5000");
});

builder.Services.AddScoped<Shared.Authentication.JwtTokenFactory>();
// JWT auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// CORS — Gateway se aane wali requests allow karne ke liye
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

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