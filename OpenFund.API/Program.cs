using System.Reflection;
using OpenFund.API.Infrastructure.Extensions;
using OpenFund.Core.Extensions;
using OpenFund.Infrastructure;
using OpenFund.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(opt => opt.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

builder.Services.ConfigureAuthentication(builder.Configuration);

builder.Services.RegisterInfrastructureServices(builder.Configuration);
builder.Services.AddGoogleAuthenticationHttpClient(builder.Configuration);
builder.Services.RegisterCoreServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await Seed.Initialize(app.Services);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(opt =>
{
    opt.AllowAnyHeader()
        .AllowAnyOrigin()
        .AllowAnyMethod();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();