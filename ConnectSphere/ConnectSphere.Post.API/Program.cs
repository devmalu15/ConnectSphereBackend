using CloudinaryDotNet; 
using ConnectSphere.Post.API.Consumers; 
using ConnectSphere.Post.API.Data; 
using ConnectSphere.Post.API.Repositories; 
using ConnectSphere.Post.API.Services; 
using MassTransit; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi.Models; 
using Serilog; 
using System.Text; 
  
var builder = WebApplication.CreateBuilder(args); 
builder.Host.UseSerilog(new 
LoggerConfiguration().WriteTo.Console().CreateLogger()); 
  
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!; 
var jwt = builder.Configuration.GetSection("JwtSettings"); 
  
builder.Services.AddDbContext<PostDbContext>(o => o.UseSqlServer(connStr)); 
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
  
builder.Services.AddSingleton(new 
Cloudinary(builder.Configuration["Cloudinary:Url"]!)); 
builder.Services.AddScoped<IPostRepository, PostRepository>(); 
builder.Services.AddScoped<IPostService, PostService>(); 
  
builder.Services.AddMassTransit(x => 
{ 
    x.AddConsumer<PostFeedFanoutFailedConsumer>(); 
    x.AddConsumer<PostCommentCountUpdatedConsumer>();
    x.AddConsumer<LikeToggledConsumer>(); 
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
builder.Services.AddSwaggerGen(c => 
{ 
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Post API", Version = "v1" }); 
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = 
SecuritySchemeType.Http, Scheme = "bearer" }); 
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new 
OpenApiSecurityScheme { Reference = new OpenApiReference { Type = 
ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } }); 
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