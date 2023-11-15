using ECC.Institute.CRM.IntegrationAPI.Models;
using ECC.Institute.CRM.IntegrationAPI;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Runtime.Intrinsics.Arm;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<ID365AuthenticationService, AuthenticationServiceMSAL>();
builder.Services.AddTransient<ID365WebAPIService, D365WebAPIService>();
builder.Services.AddScoped<IAuthoritiesService, AuthoritiesService>();
//builder.Services.AddScoped<D365AppSettings>();
builder.Services.Configure<List<D365AppSettings>>(builder.Configuration.GetSection("D365AppSettings"));

//services.Configure<List<IDP>>(Configuration.GetSection("IDP"));

var app = builder.Build();

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
