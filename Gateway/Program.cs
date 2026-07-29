var builder = WebApplication.CreateBuilder(args);

// Reverse proxy — routes come from appsettings.json "ReverseProxy" section
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS - only the Gateway needs this; individual services can lock CORS down later
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5025") // put your ACTUAL frontend URL/port here
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

app.MapReverseProxy();

app.Run();