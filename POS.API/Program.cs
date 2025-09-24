using Microsoft.AspNetCore.Authorization;
using POS.Application.Constants;
using POS.API.Authorization;
using POS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Permission:user:create", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permissions.UserCreate)));
    // Agrega políticas para cada permiso necesario
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS - allow all for development to enable local frontend testing
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // Show detailed exceptions in development so we can debug 500 errors
    app.UseDeveloperExceptionPage();
}
else
{
    // In non-development enforce HTTPS
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();