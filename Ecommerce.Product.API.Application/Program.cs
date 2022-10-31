using Confluent.Kafka;
using Ecommerce.Product.API.Core.Context;
using Ecommerce.Product.API.Core.EventBus.Connection;
using Ecommerce.Product.API.Core.EventBus.Publisher;
using Ecommerce.Product.API.Core.EventBus.Subscriber;
using Ecommerce.Product.API.Core.Infrastructure;
using Ecommerce.Product.API.Core.Kafka.Connection;
using Ecommerce.Product.API.Core.Kafka.Consumer;
using Ecommerce.Product.API.Core.Kafka.Publisher;
using Ecommerce.Product.API.Core.Listener;
using Ecommerce.Product.API.Core.Manager;
using Ecommerce.Product.API.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Database
var connection = builder.Configuration.GetConnectionString("EcommerceConn");
builder.Services.AddDbContext<EcommerceDbContext>(opt => opt.UseSqlServer(connection));
#endregion

#region Managers
builder.Services.AddScoped<IProductManager, ProductManager>();
#endregion

#region Providers
builder.Services.AddScoped<IProductDAL, ProductDAL>();
#endregion

#region RabbitMQ
builder.Services.AddSingleton<IConnectionProvider>(new ConnectionProvider(builder.Configuration["RabbitMQHost"], int.Parse(builder.Configuration["RabbitMQPort"])));
builder.Services.AddSingleton<ISubscriber, Subscriber>(e => new Subscriber(e.GetService<IConnectionProvider>(), e.GetService<IServiceScopeFactory>(), "order_detail", ExchangeType.Direct));
builder.Services.AddSingleton<IPublisher, Publisher>(e => new Publisher(e.GetService<IConnectionProvider>(), "product", ExchangeType.Direct));
#endregion

#region Listeners
builder.Services.AddHostedService<OrderCreatedListener>();
#endregion

#region Kafka
builder.Services.AddSingleton<ProducerConfig>(new ProducerConfig { BootstrapServers = builder.Configuration["KafkaServer"] });
builder.Services.AddSingleton<ConsumerConfig>(new ConsumerConfig { BootstrapServers = builder.Configuration["KafkaServer"], GroupId = "ecommerce_product_consumer", AutoOffsetReset = AutoOffsetReset.Earliest });
builder.Services.AddSingleton<IKafkaConnectionProvider, KafkaConnectionProvider>();
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
#endregion



#region AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
#endregion

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
