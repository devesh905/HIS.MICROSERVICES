using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using PatientAuthService.Data;
using PatientAuthService.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDbContextFactory<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HMSDb_MRT")));

// DbContexts — PortalDbContext ek fixed connection string use karta hai
builder.Services.AddDbContext<PortalDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PatientPortalDb")));

// Custom services
builder.Services.AddScoped<IHospitalQueryService, HospitalQueryService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddScoped<Shared.Authentication.JwtTokenFactory>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISmsService, AirtelSmsService>();
builder.Services.AddSingleton<ILoginActivityService, LoginActivityService>();

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