using Ecommerce.Product.API.Core.Context;
using Ecommerce.Product.API.Core.Infrastructure;
using Ecommerce.Product.API.Core.Manager;
using Ecommerce.Product.API.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connection = builder.Configuration.GetConnectionString("EcommerceConn");
builder.Services.AddDbContext<EcommerceDbContext>(opt => opt.UseSqlServer(connection));


builder.Services.AddScoped<IProductManager, ProductManager>();
builder.Services.AddScoped<IProductDAL, ProductDAL>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
