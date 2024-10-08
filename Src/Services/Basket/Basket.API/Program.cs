using BuildingBlocks.Behaviors;
using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Basket.API.Data;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Discount.Grpc;
using MassTransit;


var builder = WebApplication.CreateBuilder(args);

#region Config Internal Service
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CacheBasketRepository>();
#endregion

#region Config MediatR
builder.Services
    .AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblies(typeof(Program).Assembly);
    configuration.AddOpenBehavior(typeof(ValidationBehaviors<,>));
});
#endregion

#region API Configuration
builder.Services
    .AddEndpointsApiExplorer();

builder.Services
    .AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new() { Title = "Eshop Contact Api", Version = "1" });
});
#endregion

#region MessageBroker

builder.Services.AddMessageBroker(builder.Configuration, assembly: Assembly.GetExecutingAssembly());

#endregion

#region Db Configration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

#region Postgres
builder.Services
    .AddMarten(configuration =>
{
    configuration.Connection(builder.Configuration.GetConnectionString("DataBase")!);
    configuration.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();


// Health Check
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DataBase")!)
    .AddRedis(redisConnectionString!);
#endregion

#region Redis


builder.Services.AddStackExchangeRedisCache(configuration =>
    configuration.Configuration = redisConnectionString);

#endregion
#endregion

#region Grpc Configuration
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    return handler;
});
#endregion

#region Behaviors Configration
builder.Services
    .AddValidatorsFromAssembly(typeof(Program).Assembly);
#endregion

#region Carter
builder.Services
    .AddCarter();
#endregion



var app = builder.Build();

app.MapCarter();



#region Exception Handling
if (app.Environment.IsDevelopment())
{

    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            if (exception is null)
            {
                return;
            }

            ProblemDetails problemDetails = new ProblemDetails
            {
                Title = exception.Message,
                Detail = exception.StackTrace,
                Status = StatusCodes.Status500InternalServerError,
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    });


}
#endregion

#region Swagger Configuration
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint(
    "/swagger/v1/swagger.json",
    "V1"));
#endregion


app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
