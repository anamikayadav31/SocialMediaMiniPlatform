using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── 1. YARP Reverse Proxy ────────────────────────────────────────────
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── 2. JWT Authentication ────────────────────────────────────────────
var jwtKey      = builder.Configuration["Jwt:Key"]!;
var jwtIssuer   = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtIssuer,
            ValidAudience            = jwtAudience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── 3. CORS ──────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:4173",
                "https://socialmediaminiplatform-frontend.onrender.com"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── 4. Swagger UI (aggregated — shows all 7 services in one UI) ───────
// NOTE: Gateway itself has no controllers — SwaggerUI here just acts as
// a viewer that loads each downstream service's /swagger/v1/swagger.json
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // One SwaggerDoc per service — shown as separate dropdowns in the UI
    options.SwaggerDoc("auth",         new OpenApiInfo { Title = "Auth Service",         Version = "v1" });
    options.SwaggerDoc("post",         new OpenApiInfo { Title = "Post Service",         Version = "v1" });
    options.SwaggerDoc("like",         new OpenApiInfo { Title = "Like Service",         Version = "v1" });
    options.SwaggerDoc("comment",      new OpenApiInfo { Title = "Comment Service",      Version = "v1" });
    options.SwaggerDoc("follow",       new OpenApiInfo { Title = "Follow Service",       Version = "v1" });
    options.SwaggerDoc("notification", new OpenApiInfo { Title = "Notification Service", Version = "v1" });
    options.SwaggerDoc("feed",         new OpenApiInfo { Title = "Feed Service",         Version = "v1" });

    // JWT Bearer button in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token here"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
    }});
});

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ── Swagger UI ────────────────────────────────────────────────────────
// Gateway exposes one Swagger UI page at /swagger
// Each dropdown entry loads JSON directly from the downstream service
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Each downstream service's swagger.json is loaded via the YARP proxy
    // So the Gateway must be running + each service must be running
    c.SwaggerEndpoint("/api/users/swagger/v1/swagger.json",         "Auth Service");
    c.SwaggerEndpoint("/api/posts/swagger/v1/swagger.json",         "Post Service");
    c.SwaggerEndpoint("/api/likes/swagger/v1/swagger.json",         "Like Service");
    c.SwaggerEndpoint("/api/comments/swagger/v1/swagger.json",      "Comment Service");
    c.SwaggerEndpoint("/api/follows/swagger/v1/swagger.json",       "Follow Service");
    c.SwaggerEndpoint("/api/notifications/swagger/v1/swagger.json", "Notification Service");
    c.SwaggerEndpoint("/api/feed/swagger/v1/swagger.json",          "Feed Service");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "ConnectSphere — API Gateway";
});

// YARP handles all proxying — must be last
app.MapReverseProxy();

app.Run();