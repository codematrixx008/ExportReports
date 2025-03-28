using API1.Filters;
using API1.Interface;
using API1.Model;
using API1.Repository;
using API1.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using API1.DapperDbConnection;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Enable CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")  // Allow React frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();  // Allow sending cookies & Authorization headers
        });
});

// Load JWT settings from appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        // Enable Cookie-based token validation
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Specify the endpoint that should read the header token
                var path = context.HttpContext.Request.Path;

                if (path.StartsWithSegments("/api/auth/redirectloginUser"))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader["Bearer ".Length..];
                        Console.WriteLine($"Token from Header for {path}: {context.Token}");
                    }
                }
                else if (context.Request.Cookies.ContainsKey("AccessToken"))
                {
                    // Fallback to cookie token for other endpoints
                    context.Token = context.Request.Cookies["AccessToken"];
                    Console.WriteLine($"Token from Cookie for {path}: {context.Token}");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register JwtService in DI
builder.Services.AddSingleton<IJWTService, JwtService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddScoped<RoleAuthorizationFilter>();
builder.Services.AddScoped<RoleActionFilter>();
builder.Services.AddScoped<IReports, ReportsRepository>();

builder.Services.AddScoped<IDapperDbConnection, APIDevelopmentUsingDapper>();

builder.Services.AddControllers();

// Enable Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}' (without quotes)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Use Cookie Policy Middleware
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict
});

// Use CORS Middleware
app.UseCors(MyAllowSpecificOrigins);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Show detailed errors
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
