using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        //refer to https://mongodb-entities.com/wiki/Queries-Find.html for more advanced setup options
        await DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();

        // Below code snippet gets data from local json file rather than http request
        /*
        if (count == 0)
        {
            Console.WriteLine("No data - will attempt to seed");
            var itemData = await File.ReadAllTextAsync("Data/auctions.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
            await DB.SaveAsync(items);
        }
        */

        using var scope = app.Services.CreateScope();

        // use http communication to exchange data between two services and get the data from the auction service
        // this create a dependency between the two services (SearchService and AuctionService)
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        // if there is no response from Auction Service we need to periodically send this request to get the data after Auction Service is up and running
        // we need the nuget package "Microsoft.Extensions.Http.Polly" to implement the retry policy

        var items = await httpClient.GetItemsForSearchDb();

        Console.WriteLine(items.Count + " returned from the auction service");

        if (items.Count > 0) await DB.SaveAsync(items);
    }
}
