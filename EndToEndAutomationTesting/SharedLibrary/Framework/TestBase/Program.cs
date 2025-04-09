using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Framework.Logger.Interface;
using SharedLibrary.Framework.Logger.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILogging, Logging>();

var app = builder.Build();
app.Run();
