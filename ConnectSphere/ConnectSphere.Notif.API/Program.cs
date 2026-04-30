using ConnectSphere.Notif.API.Consumers; 
using ConnectSphere.Notif.API.Data; 
using ConnectSphere.Notif.API.Repositories; 
using ConnectSphere.Notif.API.Services; 
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
  
builder.Services.AddDbContext<NotifDbContext>(o => o.UseSqlServer(connStr)); 
  
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
  
builder.Services.AddScoped<INotifRepository, NotifRepository>(); 
builder.Services.AddScoped<INotifService, NotifService>(); 
  
builder.Services.AddMassTransit(x => 
{ 
    // All SAGA consumers for Notif service 
    x.AddConsumer<NotifLikeToggledConsumer>(); 
    x.AddConsumer<NotifCommentAddedConsumer>(); 
    x.AddConsumer<NotifFollowRequestedConsumer>(); 
    x.AddConsumer<NotifFollowAcceptedConsumer>(); 
    x.AddConsumer<BroadcastNotifConsumer>(); 
    x.AddConsumer<NotifMentionConsumer>();
    x.AddConsumer<NotifRepostedConsumer>();
  
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
builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddHttpClient("PostService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PostService"]!));

builder.Services.AddHttpClient("CommentService", c => 
    c.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CommentService"]!));
builder.Services.AddSwaggerGen(c => 
{ 
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notif API", Version = "v1" }); 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = 
SecuritySchemeType.Http, Scheme = "bearer" }); 
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { 
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = 
ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } 
    }); 
}); 
  
var allowedOrigin = builder.Configuration["AllowedOrigins"]!; 
builder.Services.AddCors(o => o.AddPolicy("AllowAngular", p => 
    p.WithOrigins(allowedOrigin).AllowAnyMethod().AllowAnyHeader())); 
  
var app = builder.Build(); 
app.UseSwagger(); app.UseSwaggerUI(); 
app.UseCors("AllowAngular"); 
app.UseAuthentication(); app.UseAuthorization(); 
app.MapControllers(); 
app.MapHealthChecks("/health"); 
app.Run(); 