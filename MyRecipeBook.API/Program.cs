using MyRecipeBook;
using MyRecipeBook.Application;
using MyRecipeBook.Communication.Binders.RequestRecipeJsonInstructionBinder;
using MyRecipeBook.Filters;
using MyRecipeBook.Infrastructure;
using MyRecipeBook.Middleware;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options => options.ModelBinderProviders.Insert(0, new JsonModelBinderProvider()));
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new StringConverter()));
builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddApi(builder.Configuration);
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
app.UseCors("AllowFrontend");
app.UseMiddleware<CultureMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallback(context => throw new Exception($"No route with the path {context.Request.Path}"));

await app.RunAsync();

public partial class Program
{
    private Program(){}
}