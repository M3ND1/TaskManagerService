using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Exceptions.Handlers;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TaskManagerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

//Repositories
builder.Services.AddScoped<IManagedTaskRepository, ManagedTaskRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
//Services
builder.Services.AddScoped<ManagedTaskService>();
builder.Services.AddScoped<UserService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register exception handlers
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ForbiddenExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
