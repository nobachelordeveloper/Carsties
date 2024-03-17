using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        // AuctionDbContext is a concrete class
        // IMapper interface is from automapper package
        // IPublishEndpoint interface is from MassTransit package

        //private readonly AuctionDbContext _context;
        // AuctionDbContext is inside IAuctionRepository
        private readonly IAuctionRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        //public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        public AuctionsController(IAuctionRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        // Get all auctions after a certain date
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            return await _repo.GetAuctionsAsync(date);

            // BELOW IS NO LONGER NECESSARY AS IT GOT REPLACED BY THE ABOVE CODE
            /*
            // Using string "date" argument
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();

            // With no argument for GetAllAuctions
            /*
            var auctions = await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
            */
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _repo.GetAuctionByIdAsync(id);

            if (auction == null) return NotFound();

            return auction;
            // BELOW IS NO LONGER NECESSARY AS IT GOT REPLACED BY THE ABOVE CODE
            /*
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
            */
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);

            auction.Seller = User.Identity.Name;

            _repo.AddAuction(auction);

            var newAuction = _mapper.Map<AuctionDto>(auction);

            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            var result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetAuctionById),
                new { auction.Id }, newAuction);
            // BELOW IS NO LONGER NECESSARY AS IT GOT REPLACED BY THE ABOVE CODE
            /*
            var auction = _mapper.Map<Auction>(auctionDto);
            // The below line from Program.cs enables us to use User.Identity.Name
            // "options.TokenValidationParameters.NameClaimType = "username";"
            auction.Seller = User.Identity.Name;

            _context.Auctions.Add(auction);
            var newAuction = _mapper.Map<AuctionDto>(auction);
            // create AuctionDto from auction using MappingProfile.cs
            // PUBLISH TO MASS TRANSIT OUTBOX
            // IF WE CANT PUBLISH TO OUTBOX THE WHOLE TRANSACTION FAILS
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));
            // create AuctionCreated from AuctionDto using MappingProfile.cs

            // save to POSTGRES
            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetAuctionById),
                new { auction.Id }, _mapper.Map<AuctionDto>(auction));
            */
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _repo.GetAuctionEntityById(id);

            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _repo.SaveChangesAsync();

            if (result) return Ok();

            return BadRequest("Problem saving changes");
            // BELOW IS NO LONGER NECESSARY AS IT GOT REPLACED BY THE ABOVE CODE
            /*
            var auction = await _context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            // check if the person using the update endpoint is the same as the one who created the auction
            if (auction.Seller != User.Identity.Name) return Forbid();

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            // PUBLISH UPDATED AUCTION TO MASS TRANSIT OUTBOX
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem saving changes");
            */
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _repo.GetAuctionEntityById(id);

            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();

            _repo.RemoveAuction(auction);

            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not update DB");

            return Ok();
            // BELOW IS NO LONGER NECESSARY AS IT GOT REPLACED BY THE ABOVE CODE
            /*
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            // check if the person using the delete endpoint is the same as the one who created the auction
            if (auction.Seller != User.Identity.Name) return Forbid();

            _context.Auctions.Remove(auction);

            // PUBLISH DELETED AUCTION TO MASS TRANSIT OUTBOX
            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not update DB");

            return Ok();
            */
        }
    }
}
