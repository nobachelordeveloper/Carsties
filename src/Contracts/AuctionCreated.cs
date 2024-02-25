namespace Contracts
// to use masstransit AuctionCreated needs to be from the same namespace from both the AuctionService and the SearchService
// cannot use AuctionService.Dtos namespace because 

//copied from AuctionDto.cs from AuctionService.DTOs because the AuctionDto is in a different namespace from the AuctionCreated and that is a law from MassTransit
{
    public class AuctionCreated
    {
        public Guid Id { get; set; }
        public int ReservePrice { get; set; }
        public string Seller { get; set; }
        public string Winner { get; set; }
        public int SoldAmount { get; set; }
        public int CurrentHighBid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime AuctionEnd { get; set; }
        public string Status { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string ImageUrl { get; set; }
    }

}