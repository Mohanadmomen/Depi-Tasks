using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using BookStore.Data;
using BookStore.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Controllers & API Explorer ───────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ─── Swagger with JWT Authorize button (Req 25) ───────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookStore API",
        Version = "v1"
    });

    // Adds the 🔒 Authorize button to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the field below (without the 'Bearer' prefix)."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>();

// ─── Auth Service (Req 39) ────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

// ─── CORS for local frontend (Req 16) ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ─── JWT Authentication (Req 25) ──────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretString = jwtSettings["SecretKey"] ?? "SuperSecretKey12345678901234567890";
var key = Encoding.ASCII.GetBytes(secretString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Global Error Handling (Req 37) ───────────────────────────────────────────
app.UseMiddleware<BookStore.API.Middleware.ExceptionMiddleware>();

// ─── Swagger (dev only) ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStore API v1");
        c.RoutePrefix = "swagger"; // UI lives at /swagger
    });

    // Redirect root → Swagger so localhost:PORT opens Swagger directly
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}

// ─── Middleware pipeline ───────────────────────────────────────────────────────
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();