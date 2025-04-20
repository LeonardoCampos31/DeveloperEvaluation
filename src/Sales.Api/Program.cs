using Sales.Application.Interfaces;
using Sales.Application.Mappings;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Sales.Application.Commands.Sales;
using Sales.Application.Validators;
using Sales.Infrastructure;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Sales.Api.Middleware;
using Sales.Api.Models;
using Sales.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IEventPublisher, LoggingEventPublisher>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateSaleCommand).Assembly));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSaleCommandValidator>();

builder.Services.AddControllers()
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(er => new { Field = e.Key, Message = er.ErrorMessage }))
            .ToList();

        var result = new ApiResponse<object>(null, "Validation Failed", "error")
        {
            Errors = errors // Add an Errors property to ApiResponse if desired
        };

        return new BadRequestObjectResult(result);
    };
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new() { Title = "Sales API", Version = "v1" });
});

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales API v1"));
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

// app.UseAuthorization(); // Add if authentication/authorization is needed

app.MapControllers();

app.Run();