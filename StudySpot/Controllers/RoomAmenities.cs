using StudySpot.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudySpot.DTOs;
using StudySpot.Models;

[ApiController]
[Route("api/[controller]")]
public class RoomAmenitiesController : ControllerBase
{
    private readonly StudySpotContext _context;

    public RoomAmenitiesController(StudySpotContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<RoomAmenity>> Get()
    {
        return await _context.RoomAmenity.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomAmenity>> Get(long id)
    {
        var roomAmenity = await _context.RoomAmenity.FindAsync(id);
        if (roomAmenity == null)
        {
            return NotFound();
        }
        return roomAmenity;
    }

    [HttpPost]
    public async Task<ActionResult<RoomAmenity>> Post(RoomAmenity roomAmenity)
    {
        _context.RoomAmenity.Add(roomAmenity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = roomAmenity.RoomAmenityId }, roomAmenity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoomAmenity>> Put(long id, RoomAmenity roomAmenity)
    {
        if (id != roomAmenity.RoomAmenityId)
        {
            return BadRequest();
        }

        _context.Entry(roomAmenity).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<RoomAmenity>> Delete(long id)
    {
        var roomAmenity = await _context.RoomAmenity.FindAsync(id);
        if (roomAmenity == null)
        {
            return NotFound();
        }

        _context.RoomAmenity.Remove(roomAmenity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("/api/rooms/{roomId}/amenities")]
    public async Task<ActionResult<IEnumerable<long>>> GetAmenitiesByRoomId(long roomId)
    {
        var amenities = await _context.RoomAmenity
            .Where(ra => ra.RoomId == roomId)
            .Select(ra => ra.AmenityId)
            .ToListAsync();
        return amenities;
    }
}