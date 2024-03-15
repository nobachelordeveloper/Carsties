using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService;
using SearchService.Consumers;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
// Add the HttpClient that is injected in the AuctionSvcHttpClient class
//refer to line 37 for the implementation of GetPolicy()

//initialize masstransit
builder.Services.AddMassTransit(x =>
{
    // Any consumer that is in the same namespace as the AuctionCreatedConsumer will be automatically registered by mass transit
    x.AddConsumersFromNamespaceContaining<AuctionDeletedConsumer>();
    // Format the endpoint name to kebab case so that the we can recognize this particular AuctionCreatedConsumer came from Search Service
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    // this creates search-auction-created exchange on RabbitMq

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        // If MongoDB was down, we need to retry to pick up messages that failed from the outbox
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            // This line is used to configure the retry policy for the AuctionCreatedConsumer
            // Try five times and wait for five seconds before trying again

            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
            // This line is used to configure the consumer for the AuctionCreatedConsumer
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

// You need to add the DbInitializer.InitDb(app) method to the ApplicationStarted event so that in case there is no response from AuctionSvc from DbInitializer.InitDb(app), the application will not hang on the "await" and run "app.Run()" and the user can still use the search functionality.
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});

app.Run();

// Setup Polly retry policy
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));