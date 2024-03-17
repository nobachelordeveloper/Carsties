using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    //this line refers to appsettings.Development.json
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// this line refers to 'RequestHelpers/MappingProfiles.cs'

//initialize masstransit
builder.Services.AddMassTransit(x =>
{
    // Setup the outbox for failed messages to RabbitMq
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        // The query will check for messages in the outbox every 10 seconds
        o.QueryDelay = TimeSpan.FromSeconds(10);

        // https://masstransit.io/documentation/configuration/middleware/outbox
        // Outboxes for mass transit can only be configurd with postgres, sqlserver, and mysql
        o.UsePostgres();
        o.UseBusOutbox();
    });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["IdentityServiceUrl"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.NameClaimType = "username";
        });

    builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

    // this is to consume fault consumers from AuctionCreatedFaultConsumer.cs and every other consumer in the same namespace
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    // this is to set a custom endpoint on RabbitMq
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    // this creates "auction-auction-created-fault" exchange on RabbitMq

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
    //this line refers to 'Data/DbInitializer.cs'
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();

public partial class Program { }
