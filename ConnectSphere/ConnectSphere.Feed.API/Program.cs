using ConnectSphere.Feed.API.Consumers;  
using ConnectSphere.Feed.API.Data; 
using ConnectSphere.Feed.API.Services; 
using ConnectSphere.Feed.API.Repositories;
using MassTransit; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi.Models; 
using Serilog; 
using System.Text; 
using Polly;
  
var builder = WebApplication.CreateBuilder(args); 
builder.Host.UseSerilog(new 
LoggerConfiguration().WriteTo.Console().CreateLogger()); 
  
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!; 
var jwt = builder.Configuration.GetSection("JwtSettings"); 
  
builder.Services.AddDbContext<FeedDbContext>(o => o.UseSqlServer(connStr)); 
  
builder.Services.AddStackExchangeRedisCache(o => 
    o.Configuration = builder.Configuration.GetConnectionString("Redis")); 
  
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) 
    .AddJwtBearer(o => 
    { 
        o.TokenValidationParameters = new TokenValidationParameters 
        { 
            ValidateIssuer = true, ValidateAudience = true, 
            ValidateIssuerSigningKey = true, ValidateLifetime = true, 
            ValidIssuer = jwt["Issuer"], ValidAudience = jwt["Audience"], 
            IssuerSigningKey = new 
SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!)) 
        }; 
    }); 
  
builder.Services.AddScoped<IFeedService, FeedService>(); 
  

builder.Services.AddHttpClient("PostService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PostService"]!)) 
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => 
TimeSpan.FromSeconds(1))); 
  
builder.Services.AddHttpClient("FollowService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:FollowService"]!)) 
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => 
TimeSpan.FromSeconds(1))); 
  
builder.Services.AddMassTransit(x => 
{ 
    x.AddConsumer<FeedPostCreatedConsumer>(); 
    x.AddConsumer<FeedFollowAcceptedFeedConsumer>(); 
    x.AddConsumer<FeedPostDeletedFeedConsumer>(); 
    x.AddConsumer<FeedLikeToggledFeedConsumer>(); 
    x.AddConsumer<FeedUnfollowedConsumer>(); 
    x.UsingRabbitMq((ctx, cfg) => 
    { 
        cfg.Host(builder.Configuration["RabbitMQ:Host"], 
builder.Configuration["RabbitMQ:VHost"], h => 
        { 
            h.Username(builder.Configuration["RabbitMQ:Username"]!); 
            h.Password(builder.Configuration["RabbitMQ:Password"]!); 
        }); 
        cfg.ConfigureEndpoints(ctx); 
    }); 
}); 
  
builder.Services.AddHealthChecks() 
    .AddSqlServer(connStr) 
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!); 
  
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddScoped<IFeedRepository, FeedRepository>(); 
builder.Services.AddScoped<IUserTagPreferenceRepository, 
UserTagPreferenceRepository>();
builder.Services.AddSwaggerGen(c => 
{ 
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Feed API", Version = "v1" }); 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = 
SecuritySchemeType.Http, Scheme = "bearer" }); 
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { 
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = 
ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } 
    }); 
}); 
  
var app = builder.Build(); 
app.UseSwagger(); app.UseSwaggerUI(); 
app.UseAuthentication(); app.UseAuthorization(); 
app.MapControllers(); 
app.MapHealthChecks("/health"); 
app.Run();