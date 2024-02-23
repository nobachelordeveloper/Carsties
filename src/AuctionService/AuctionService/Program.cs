using AuctionService.Data;
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

var app = builder.Build();

// Configure the HTTP request pipeline.

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
