using CloudinaryDotNet; 
using ConnectSphere.Auth.API.Consumers; 
using ConnectSphere.Auth.API.Data; 
using ConnectSphere.Auth.API.Repositories; 
using ConnectSphere.Auth.API.Services; 
using MassTransit; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi.Models; 
using Serilog; 
using System.Text; 
using Microsoft.Extensions.DependencyInjection;
  
var builder = WebApplication.CreateBuilder(args); 
  
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger(); 
builder.Host.UseSerilog(); 
  
var connectionString = 
builder.Configuration.GetConnectionString("DefaultConnection")!; 
var jwtSettings = builder.Configuration.GetSection("JwtSettings"); 
var cloudinaryUrl = builder.Configuration["Cloudinary:Url"]!; 
  
builder.Services.AddDbContext<AuthDbContext>(o => 
o.UseSqlServer(connectionString)); 
  
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) 
    .AddJwtBearer(options => 
    { 
        options.TokenValidationParameters = new TokenValidationParameters 
        { 
            ValidateIssuer = true, 
            ValidateAudience = true, 
            ValidateLifetime = true, 
            ValidateIssuerSigningKey = true, 
            ValidIssuer = jwtSettings["Issuer"], 
            ValidAudience = jwtSettings["Audience"], 
            IssuerSigningKey = new 
SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)) 
        }; 
    }); 
  
builder.Services.AddStackExchangeRedisCache(o => 
    o.Configuration = builder.Configuration.GetConnectionString("Redis")); 
  
builder.Services.AddSingleton(new Cloudinary(cloudinaryUrl)); 
builder.Services.AddScoped<IUserRepository, UserRepository>(); 
builder.Services.AddScoped<IUserService, UserService>(); 
  
builder.Services.AddMassTransit(x => 
{ 
    x.AddConsumer<CountersUpdatedConsumer>(); 
    x.UsingRabbitMq((ctx, cfg) => 
    { 
        // Force the use of port 5671 for SSL
        cfg.Host(builder.Configuration["RabbitMQ:Host"], 5671, builder.Configuration["RabbitMQ:VHost"], h => 
        { 
            h.Username(builder.Configuration["RabbitMQ:Username"]!); 
            h.Password(builder.Configuration["RabbitMQ:Password"]!); 

            h.UseSsl(s => 
            {
                s.Protocol = System.Security.Authentication.SslProtocols.Tls12;
            });
        }); 

        cfg.ConfigureEndpoints(ctx); 
    }); 
});


builder.Services.AddHealthChecks() 
    .AddSqlServer(connectionString) 
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!); 
  
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(c => 
{ 
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" }); 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
    { 
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" 
    }); 
    c.AddSecurityRequirement(new OpenApiSecurityRequirement 
    { 
        { 
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = 
ReferenceType.SecurityScheme, Id = "Bearer" } }, 
            Array.Empty<string>() 
        } 
    }); 
}); 
  
var allowedOrigin = builder.Configuration["AllowedOrigins"]!; 
builder.Services.AddCors(o => o.AddPolicy("AllowAngular", p => 
    p.WithOrigins(allowedOrigin).AllowAnyMethod().AllowAnyHeader())); 
  
var app = builder.Build(); 
app.UseSwagger(); 
app.UseSwaggerUI(); 
app.UseCors("AllowAngular"); 
app.UseAuthentication(); 
app.UseAuthorization(); 
app.MapControllers(); 
app.MapHealthChecks("/health"); 
app.Run(); 