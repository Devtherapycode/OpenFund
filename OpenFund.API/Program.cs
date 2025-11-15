using System.Reflection;
using FluentValidation;
using MediatR;
using OpenFund.API.Infrastructure.Extensions;
using OpenFund.API.Infrastructure.Middlewares;
using OpenFund.Core.Behavior;
using OpenFund.Core.Extensions;
using OpenFund.Infrastructure;
using OpenFund.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(opt => opt.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

builder.Services.AddLogging();

builder.Services.ConfigureAuthentication(builder.Configuration);

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();