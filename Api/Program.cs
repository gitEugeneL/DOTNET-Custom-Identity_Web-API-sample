using System.Reflection;
using Api.Data;
using Api.Extensions;
using Api.Services;
using Api.Services.Interfaces;
using FluentValidation;
using IdentityApi.Services.Interfaces;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddScoped<IPasswordService, PasswordService>()
    .AddScoped<ILockoutService, LockoutService>()
    .AddScoped<ITokenService, TokenService>()
    .AddScoped<IMessageService, MessageService>()
    .AddScoped<IConfirmationService, ConfirmationService>()
    .AddOpenApi()
    .AddSingleton<DapperDbContext>()
    .AddRepoServices()
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
    .AddRateLimitingServices()
    .AddEndpoints();

builder.Services.ConfigureAuthentication(builder.Configuration);
builder.Services.ConfigureAuthPolicy();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseRateLimiter();
app.MapEndpoints();
app.Run();

public partial class Program
{
}