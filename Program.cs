using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Monopoly.Application.Services;
using Monopoly.Core;
using Monopoly.Core.Interfaces.IServices;
using Monopoly.DataAccess.Postgres;
using Monopoly.DataAccess.Postgres.Repositories;
using Monopoly.Core.Interfaces.IRepositories;
using Monopoly.Core.Interfaces.IUnitsOfWork;
using Monopoly.DataAccess.Postgres.UnitsOfWork;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constants.JwtKey))
    };
});
builder.Services.AddAuthorization();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
builder.Services.AddScoped<IRoomsRepository, RoomsRepository>();
builder.Services.AddScoped<IGamesRepository, GamesRepository>();
builder.Services.AddScoped<IBoardsRepository, BoardsRepository>();
builder.Services.AddScoped<IPlayersRepository, PlayersRepository>();
builder.Services.AddScoped<ITurnStatesRepository, TurnStatesRepository>();

builder.Services.AddScoped<IAccountsUnitOfWork, AccountsUnitOfWork>();
builder.Services.AddScoped<IRoomsUnitOfWork, RoomsUnitOfWork>();
builder.Services.AddScoped<IGamesUnitOfWork, GamesUnitOfWork>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "¬вед≥ть JWT токен доступу",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddDbContext<MonopolyDbContext>(
    options =>
    {
        options.UseNpgsql(Constants.Connect);
    });

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
