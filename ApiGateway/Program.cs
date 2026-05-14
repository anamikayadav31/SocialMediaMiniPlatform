using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── 1. YARP Reverse Proxy ────────────────────────────────────────────
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ── 2. JWT Authentication (for Gateway-level auth if needed) ─────────
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

// NOTE: No named authorization policies — downstream services handle their own auth.
// YARP simply forwards the Authorization header; each microservice validates the JWT.
builder.Services.AddAuthorization();

// ── 3. CORS ──────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175",
                "http://localhost:4173",
                "http://localhost:4174"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── 4. Swagger UI ────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("auth",         new OpenApiInfo { Title = "Auth Service",         Version = "v1" });
    options.SwaggerDoc("post",         new OpenApiInfo { Title = "Post Service",         Version = "v1" });
    options.SwaggerDoc("like",         new OpenApiInfo { Title = "Like Service",         Version = "v1" });
    options.SwaggerDoc("comment",      new OpenApiInfo { Title = "Comment Service",      Version = "v1" });
    options.SwaggerDoc("follow",       new OpenApiInfo { Title = "Follow Service",       Version = "v1" });
    options.SwaggerDoc("notification", new OpenApiInfo { Title = "Notification Service", Version = "v1" });
    options.SwaggerDoc("feed",         new OpenApiInfo { Title = "Feed Service",         Version = "v1" });

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

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger-auth/v1/swagger.json",         "Auth Service");
    c.SwaggerEndpoint("/swagger-post/v1/swagger.json",         "Post Service");
    c.SwaggerEndpoint("/swagger-like/v1/swagger.json",         "Like Service");
    c.SwaggerEndpoint("/swagger-comment/v1/swagger.json",      "Comment Service");
    c.SwaggerEndpoint("/swagger-follow/v1/swagger.json",       "Follow Service");
    c.SwaggerEndpoint("/swagger-notification/v1/swagger.json", "Notification Service");
    c.SwaggerEndpoint("/swagger-feed/v1/swagger.json",          "Feed Service");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "ConnectSphere — API Gateway";
});

app.UseAuthorization();
app.MapReverseProxy();

app.Run();