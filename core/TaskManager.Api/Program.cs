using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Exceptions.Handlers;
using TaskManager.Api.Extensions;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Authentication;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<TaskManager.Application.Validators.CreateUserDtoValidator>();

var jwtSettings = builder.Configuration.GetSection("AuthConfiguration");
builder.Services.Configure<AuthConfiguration>(jwtSettings);

builder.Services.AddDbContext<TaskManagerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

//JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
//Repositories
builder.Services.AddScoped<IManagedTaskRepository, ManagedTaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
//Services
builder.Services.AddScoped<ManagedTaskService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
//Authentication
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
    })
    .AddPolicy("User", policy =>
    {
        policy.RequireRole("User");
    })
    .AddPolicy("TaskEditor", policy =>
    {
        policy.RequireRole("Admin", "User");
    });

// Register exception handlers
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ForbiddenExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();
app.UseExceptionHandler(o => { });

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
