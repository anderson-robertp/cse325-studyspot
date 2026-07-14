using StudySpot.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudySpot.DTOs;
using StudySpot.Models;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly StudySpotContext _context;

    public RoomsController(StudySpotContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Room>> Get()
    {
        return await _context.Rooms.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> Get(long id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }
        return room;
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        var count = await _context.Rooms.CountAsync();
        return count;
    }

    [HttpPost]
    public async Task<ActionResult<Room>> Post(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = room.RoomId }, room);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Room>> Put(long id, Room room)
    {
        if (id != room.RoomId)
        {
            return BadRequest();
        }

        _context.Entry(room).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(room);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Room>> Delete(long id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}