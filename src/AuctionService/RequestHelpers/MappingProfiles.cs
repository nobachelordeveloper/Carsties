using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionDto>();
            CreateMap<CreateAuctionDto, Auction>()
                .ForMember(d => d.Item, o => o.MapFrom(s => s));
            CreateMap<CreateAuctionDto, Item>();
            // hook up to the AuctionCreated event from Contracts
            // SearchService knows about AuctionCreated event from Contracts
            CreateMap<AuctionDto, AuctionCreated>();

            // Used for line at Program.cs
            // await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
            CreateMap<Auction, AuctionUpdated>().IncludeMembers(a => a.Item);
            CreateMap<Item, AuctionUpdated>();
        }
    }
}
