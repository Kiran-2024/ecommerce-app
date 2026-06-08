using ECommerceAPI.Authorization; 
using ECommerceAPI.Data;
using ECommerceAPI.Helpers;
using Microsoft.OpenApi;
using ECommerceAPI.Middleware;
using ECommerceAPI.Repositories;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;  
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddSingleton<IAuthorizationHandler, RightsAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    var rights = new[]
    {
        "product.create", "product.edit", "product.delete",
        "order.view", "order.manage", "user.manage"
    };
    foreach (var right in rights)
    {
        options.AddPolicy($"Right:{right}", policy =>
            policy.Requirements.Add(new HasRightRequirement(right)));
    }
});


builder.Services.AddSingleton<DatabaseHelper>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddScoped<OtpRepository>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<RightsRepository>();       
builder.Services.AddScoped<RoleRightsRepository>();
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "http://localhost:4300",
            "http://localhost:51680",
            "http://localhost:61439"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();