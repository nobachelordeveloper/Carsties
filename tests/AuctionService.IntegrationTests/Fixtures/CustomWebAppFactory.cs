using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests;

// IAsyncLifetime from XUnit is used to initialize and dispose the PostgreSqlContainer
public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        //this is run in DOCKER

        // goal is to create a fake database to use replacing the real database
        // builder.Services.AddDbContext<AuctionDbContext>(options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // moved to ServiceCollectionExtenions.cs
            //var descriptor = services.SingleOrDefault(d =>
            //d.ServiceType == typeof(DbContextOptions<AuctionDbContext>))
            //if (descriptor != null) services.Remove(descriptor);

            services.RemoveDbContext<AuctionDbContext>();
            //RemoveDbContext is coming from ServiceCollectionExtensions.cs

            services.AddDbContext<AuctionDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
                // use the fake container
            });

            services.AddMassTransitTestHarness();
            // add the fake mass transit test harness  from mass transit
            // remove the configuration from program.cs and replace it with this

            // below will set up the database with the fake container with the proper schema
            // below is also added to ServiceCollectionExtensions.cs
            // var sp = services.BuildServiceProvider();
            // using var scope = sp.CreateScope();
            // var scopedServices = scope.ServiceProvider;
            // var db = scopedServices.GetRequiredService<AuctionDbContext>();
            // db.Database.Migrate();

            services.EnsureCreated<AuctionDbContext>();
            //EnsureCreated is coming from ServiceCollectionExtensions.cs

            // Add below to configure the fake jwt bearer
            // this enables client to get a token to access the api without the need for a real identity server at CreateAuction_WithAuth_ShouldReturn201() in AuctionControllerTests.cs
            // Example: _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));
            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer(opt =>
                {
                    opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
                });
        });
    }

    Task IAsyncLifetime.DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();
}

internal class PostgresSqlContainer
{
}