using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    // this is used to consume faulty creations that are created from AuctionCreatedConsumer.cs with Item.Model == "Foo" from SearchService
    public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
    {
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("--> Consuming faulty creation");

            var exception = context.Message.Exceptions.First();

            if (exception.ExceptionType == "System.ArgumentException")
            {
                context.Message.Message.Model = "FooBar";
                await context.Publish(context.Message.Message);
                // usually you want to log this as an error message in another database to keep record of the error
            }
            else
            {
                Console.WriteLine("Not an argument exception - update error dashboard somewhere");
            }
        }
    }
}
