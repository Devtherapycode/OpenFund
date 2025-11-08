using System.Reflection;
using OpenFund.API.Infrastructure.Extensions;
using OpenFund.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(opt => opt.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

builder.Services.ConfigureAuthentication(builder.Configuration);

builder.Services.RegisterInfrastructureServices(builder.Configuration);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();