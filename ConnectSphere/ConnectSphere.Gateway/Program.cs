using Ocelot.DependencyInjection; 
using Ocelot.Middleware; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens; 
using Serilog; 
using System.Text; 
  
var builder = WebApplication.CreateBuilder(args); 

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
  
Log.Logger = new LoggerConfiguration() 
    .WriteTo.Console() 
    .CreateLogger(); 
builder.Host.UseSerilog(); 
  
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: 
true); 
  
var jwtSettings = builder.Configuration.GetSection("JwtSettings"); 
var secret = jwtSettings["Secret"]!; 
  
builder.Services.AddAuthentication("Bearer") 
    .AddJwtBearer("Bearer", options => 
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
SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)) 
        }; 
    }); 
  
builder.Services.AddOcelot(); 
builder.Services.AddCors(o => o.AddPolicy("AllowAngular", p => 
    p.WithOrigins(builder.Configuration["AllowedOrigins"]!) 
     .AllowAnyMethod().AllowAnyHeader())); 
  
var app = builder.Build(); 
app.UseCors("AllowAngular"); 
app.UseAuthentication(); 
await app.UseOcelot(); 
app.Run();