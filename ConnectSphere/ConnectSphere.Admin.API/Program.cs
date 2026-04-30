using ConnectSphere.Admin.API.Data; 
using ConnectSphere.Admin.API.Services; 
using MassTransit; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi.Models; 
using Serilog; 
using System.Text; 
using Polly;
using System.Text.Json.Serialization;
  
var builder = WebApplication.CreateBuilder(args); 
builder.Host.UseSerilog(new 
LoggerConfiguration().WriteTo.Console().CreateLogger()); 
  
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!; 
var jwt = builder.Configuration.GetSection("JwtSettings"); 
  
builder.Services.AddDbContext<AdminDbContext>(o => o.UseSqlServer(connStr)); 
  
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
  
builder.Services.AddScoped<IAdminService, AdminService>(); 
  

builder.Services.AddHttpClient("AuthService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:AuthService"]!)) 
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => 
TimeSpan.FromSeconds(1))); 
  
builder.Services.AddHttpClient("PostService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PostService"]!)) 
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => 
TimeSpan.FromSeconds(1))); 
  
builder.Services.AddHttpClient("CommentService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CommentService"]!)) 
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => 
TimeSpan.FromSeconds(1))); 
  
builder.Services.AddHttpClient("FeedService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:FeedService"]!)) 
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => 
TimeSpan.FromSeconds(1))); 

builder.Services.AddHttpClient("LikeService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:LikeService"]!)) 
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => 
TimeSpan.FromSeconds(1))); 

builder.Services.AddHttpContextAccessor();
  
builder.Services.AddMassTransit(x => 
{ 
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
  
builder.Services.AddHealthChecks().AddSqlServer(connStr); 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(c => 
{ 
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin API", Version = "v1" }); 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = 
SecuritySchemeType.Http, Scheme = "bearer" }); 
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { 
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = 
ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } 
    }); 
}); 
  

var adminOrigin = builder.Configuration["AdminAllowedOrigins"] ?? 
builder.Configuration["AllowedOrigins"]!; 
builder.Services.AddCors(o => o.AddPolicy("AllowAdmin", p => 
    p.WithOrigins(adminOrigin).AllowAnyMethod().AllowAnyHeader())); 
  
var app = builder.Build(); 
app.UseSwagger(); app.UseSwaggerUI(); 
app.UseCors("AllowAdmin"); 
app.UseAuthentication(); app.UseAuthorization(); 
app.MapControllers(); 
app.MapHealthChecks("/health"); 
app.Run(); 