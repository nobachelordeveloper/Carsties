using AutoMapper;
using Contracts;
using SearchService.Models;

namespace SearchService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            //Item is created from AuctionCreated and AuctionUpdated
            CreateMap<AuctionCreated, Item>();
            CreateMap<AuctionUpdated, Item>();
        }

    }
}
