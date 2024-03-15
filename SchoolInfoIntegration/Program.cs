using ECC.Institute.CRM.IntegrationAPI.Models;
using ECC.Institute.CRM.IntegrationAPI;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using ECC.Institute.CRM.IntegrationAPI.Filters;
using ECC.Institute.CRM.IntegrationAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddControllers(options =>
{
    // Removing: Will use custom validation middleware to validate 
    //options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<ID365AuthenticationService, AuthenticationServiceMSAL>();
builder.Services.AddTransient<ID365WebAPIService, D365WebAPIService>();
builder.Services.AddScoped<IAuthoritiesService, AuthoritiesService>();
builder.Services.AddScoped<ValidationFilterAttribute>();

builder.Services.Configure<List<D365AppSettings>>(builder.Configuration.GetSection("D365AppSettings"));

// Disable default validation, sending 422 with model data validation error
builder.Services.Configure<ApiBehaviorOptions>(options
    => options.SuppressModelStateInvalidFilter = true);

// Add request body middlewar

//services.Configure<List<IDP>>(Configuration.GetSection("IDP"));

var app = builder.Build();

app.UseRawRequestBodyMiddleware();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
