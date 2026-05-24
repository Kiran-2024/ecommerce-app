using ECommerceAPI.Data;
using ECommerceAPI.Middleware;
using ECommerceAPI.Repositories;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Helpers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseHelper>();


builder.Services.AddScoped<IRolesRepository, RolesRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");  
app.UseAuthorization();
app.MapControllers();

app.Run();