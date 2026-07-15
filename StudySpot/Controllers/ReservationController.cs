using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudySpot.Models;
using StudySpot.Data;
using System.Security.Claims;
using StudySpot.DTOs;


[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly StudySpotContext _context;

    public ReservationController(StudySpotContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<Reservation>> Get()
    {
        return await _context.Reservations.ToListAsync();
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<List<Reservation>>> GetMine()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        return await _context.Reservations
            .Where(reservation =>
                reservation.UserId == userId.Value &&
                (reservation.Status == null || reservation.Status != "Canceled"))
            .OrderBy(reservation => reservation.StartTime)
            .ToListAsync();
    }

    [HttpGet("slots")]
    public async Task<ActionResult<List<AppointmentSlot>>> GetSlots(long roomId, DateTime? weekStart)
    {
        var pickedDate = weekStart?.Date ?? DateTime.UtcNow.Date;
        var firstDay = DateTime.SpecifyKind(StartOfWeek(pickedDate), DateTimeKind.Utc);
        var lastDay = firstDay.AddDays(5);

        var reservations = await _context.Reservations
            .Where(reservation =>
                reservation.RoomId == roomId &&
                reservation.StartTime < lastDay.AddHours(17) &&
                reservation.EndTime > firstDay.AddHours(10) &&
                (reservation.Status == null || reservation.Status != "Canceled"))
            .ToListAsync();

        var slots = new List<AppointmentSlot>();

        for (var day = 0; day < 5; day++)
        {
            var date = firstDay.AddDays(day);

            for (var hour = 10; hour < 17; hour++)
            {
                var start = date.AddHours(hour);
                var end = start.AddHours(1);

                var reservation = reservations.FirstOrDefault(item =>
                    item.StartTime < end && item.EndTime > start);

                slots.Add(new AppointmentSlot
                {
                    RoomId = roomId,
                    StartTime = start,
                    EndTime = end,
                    IsAvailable = reservation == null,
                    ReservationId = reservation?.ReservationId
                });
            }
        }

        return slots;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reservation>> Get(Guid id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }
        return reservation;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Reservation>> Post(Reservation reservation)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var start = DateTime.SpecifyKind(reservation.StartTime, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(reservation.EndTime, DateTimeKind.Utc);

        if (end <= start)
        {
            return BadRequest("Pick a valid time.");
        }

        var taken = await _context.Reservations.AnyAsync(item =>
            item.RoomId == reservation.RoomId &&
            item.StartTime < end &&
            item.EndTime > start &&
            (item.Status == null || item.Status != "Canceled"));

        if (taken)
        {
            return Conflict("That time is already reserved.");
        }

        var now = DateTime.UtcNow;
        reservation.UserId = userId.Value;
        reservation.StartTime = start;
        reservation.EndTime = end;
        reservation.CreatedAt = now;
        reservation.UpdatedAt = now;
        reservation.Status = string.IsNullOrWhiteSpace(reservation.Status)
            ? "Reserved"
            : reservation.Status;

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = reservation.ReservationId }, reservation);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<Reservation>> Put(Guid id, Reservation reservation)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var savedReservation = await _context.Reservations.FindAsync(id);
        if (savedReservation == null)
        {
            return NotFound();
        }

        if (savedReservation.UserId != userId.Value)
        {
            return Forbid();
        }

        var start = DateTime.SpecifyKind(reservation.StartTime, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(reservation.EndTime, DateTimeKind.Utc);

        if (end <= start)
        {
            return BadRequest();
        }

        var taken = await _context.Reservations.AnyAsync(item =>
            item.ReservationId != id &&
            item.RoomId == reservation.RoomId &&
            item.StartTime < end &&
            item.EndTime > start &&
            (item.Status == null || item.Status != "Canceled"));

        if (taken)
        {
            return Conflict("That time is already reserved.");
        }

        savedReservation.RoomId = reservation.RoomId;
        savedReservation.StartTime = start;
        savedReservation.EndTime = end;
        savedReservation.Status = string.IsNullOrWhiteSpace(reservation.Status)
            ? savedReservation.Status
            : reservation.Status;
        savedReservation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<Reservation>> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.UserId != userId.Value)
        {
            return Forbid();
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userId, out var id))
        {
            return id;
        }

        return null;
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        var count = await _context.Reservations.CountAsync();
        return count;
    }

    [HttpGet("counts")]
    public async Task<List<RoomReservationCountDto>> GetReservationCountsAsync()
    {
        return await _context.Reservations
            .GroupBy(r => r.RoomId)
            .Select(g => new RoomReservationCountDto
            {
                RoomId = g.Key,
                ReservationCount = g.Count()
            })
            .Join(_context.Rooms,
                g => g.RoomId,
                r => r.RoomId,
                (g, r) => new RoomReservationCountDto
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    ReservationCount = g.ReservationCount
                })
            .ToListAsync();
    }

    [HttpGet("upcoming")]
    public async Task<List<UpcomingReservationsDto>> GetUpcomingReservationsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Room)
            .Where(r => r.StartTime > now)
            .Select(r => new UpcomingReservationsDto
            {
                ReservationId = r.ReservationId,
                Email = r.User.Email ?? "",
                RoomName = r.Room.RoomName ?? "",
                StartTime = r.StartTime,
                EndTime = r.EndTime
            })
            .ToListAsync();
    }

    [HttpGet ("reseravtions/manager")]
    public async Task<List<ReservationManagerDto>> GetReservationsForManagementAsync()
    {
        return await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Room)
            .Select(r => new ReservationManagerDto
            {
                ReservationId = r.ReservationId,
                Email = r.User.Email ?? "",
                RoomName = r.Room.RoomName ?? "",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status ?? ""
            })
            .ToListAsync();
    }
}
