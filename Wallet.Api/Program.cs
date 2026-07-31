
using Microsoft.AspNetCore.Diagnostics;
using Wallet.Application;
using Wallet.Infrastructure;
using Wallet.Infrastructure.DataAccessManager.Persistence;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(); 
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(error => error.Run(async context =>
{
    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (ex is ValidationException validation)
    {
        var errors = validation.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(x => x.ErrorMessage).ToArray());
        await Results.ValidationProblem(errors, statusCode: StatusCodes.Status400BadRequest).ExecuteAsync(context);
        return;
    }
    var (status, title) = ex is DomainException domain ? (domain.StatusCode, domain.Message) : (500, "An unexpected error occurred.");
    await Results.Problem(statusCode: status, title: title).ExecuteAsync(context);
}));

app.UseSwagger(); 
app.UseSwaggerUI();
using (var scope = app.Services.CreateScope()) 
    await scope.ServiceProvider.GetRequiredService<WalletDbContext>().Database.EnsureCreatedAsync();
app.MapControllers();
app.Run();
