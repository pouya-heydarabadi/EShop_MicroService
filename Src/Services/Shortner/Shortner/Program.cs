using System.Reflection;
using Carter;
using Microsoft.EntityFrameworkCore;
using Shortner.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Carter

builder.Services.AddCarter();

#endregion

#region MediatR

builder.Services.AddMediatR(configuration: configuration =>
{
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

#endregion

#region Database

string connectionString = builder.Configuration.GetConnectionString("Mongo")!;
string dataBaseName = builder.Configuration.GetSection("DatabaseName").Value!;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMongoDB(connectionString, dataBaseName));

#endregion



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapCarter();

app.Run();
