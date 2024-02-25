namespace Contracts
// to use masstransit AuctionCreated needs to be from the same namespace from both the AuctionService and the SearchService

//copied from UpdateAuctionDto.cs from AuctionService.DTOs
//Event Emitted Types from auctionSvcSpec.pdf
{
    public class AuctionUpdated
    {
        public string Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
    }
}
