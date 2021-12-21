global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Net.Http.Json;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Diagnostics;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using Npgsql;
global using Blog.Models;
global using Blog.ViewModels;
using Blog;
using Blog.Utils;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

string connection = builder.Configuration.GetConnectionString("PgsqlConnection");
services.AddSingleton(new DatabaseConnectionStrings()
{
    Pgsql = connection
});

services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connection));

services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { options.LoginPath = new PathString("/Login"); });

services.AddScoped<IScopedData, ScopedData>();


services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseNamingPolicy();
        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            return new BadRequestObjectResult(new
            {
                Errors = actionContext.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(k => k.Key, e => e.Value!.Errors.Select(e => e.ErrorMessage))
            });
        };
    });

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<VerifyAuthAndInitUserDataMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action}/{id?}");
});

app.Run();