using System.Net.Http.Json;
using StudySpot.Models;
using StudySpot.DTOs;
using Microsoft.EntityFrameworkCore;
using StudySpot.Data;
using Microsoft.AspNetCore.Components.Authorization;

namespace StudySpot.Services;
public class ReservationService
{
    private readonly StudySpotContext _context;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public ReservationService(StudySpotContext context, AuthenticationStateProvider authenticationStateProvider)
    {
        _context = context;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<List<Reservation>> GetReservationsAsync()
    {
        var reservations = await _context.Reservations.ToListAsync();
        return reservations;
    }

    public async Task<Reservation?> GetReservationByIdAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        return reservation;
    }

    public async Task<bool> CreateReservationAsync(Reservation reservation)
    {
        var conflict = await _context.Reservations
        .AnyAsync(r =>
            r.RoomId == reservation.RoomId &&
            r.Status != "Canceled" &&
            r.StartTime < reservation.EndTime &&
            r.EndTime > reservation.StartTime);

        if (conflict)
        {
            return false;
        }

        reservation.ReservationId = Guid.NewGuid();
        reservation.CreatedAt = DateTime.UtcNow;

        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Reservation> UpdateReservationAsync(Reservation reservation)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task DeleteReservationAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation != null)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetReservationCountAsync()
    {
        return await _context.Reservations.CountAsync();
    }

    public async Task<List<RoomReservationCountDto>> GetReservationCountsAsync()
    {
        return await _context.Reservations
            .GroupBy(r => new
            {
                r.RoomId,
                r.Room.RoomName
            })
            .Select(group => new RoomReservationCountDto
            {
                RoomId = group.Key.RoomId,
                RoomName = group.Key.RoomName,
                ReservationCount = group.Count()
            })
            .OrderByDescending(r => r.ReservationCount)
            .ToListAsync();
    }

    public async Task<List<UpcomingReservationsDto>> GetUpcomingReservationsAsync()
    {
        var today = DateTime.UtcNow;

        return await _context.Reservations
            .Where(r => r.StartTime >= today)
            .OrderBy(r => r.StartTime)
            .Select(r => new UpcomingReservationsDto
            {
                ReservationId = r.ReservationId,
                Email = r.User.Email,
                RoomName = r.Room.RoomName,
                StartTime = r.StartTime,
                EndTime = r.EndTime
            })
            .ToListAsync();
    }

    public async Task<List<ReservationManagerDto>> GetReservationsForManagerAsync()
    {
        return await _context.Reservations
            .OrderByDescending(r => r.StartTime)
            .Select(r => new ReservationManagerDto
            {
                ReservationId = r.ReservationId,
                Email = r.User != null ? r.User.Email : "",
                RoomName = r.Room != null ? r.Room.RoomName : "",
                StartTime = r.StartTime,
                EndTime = r.EndTime
            })
            .ToListAsync();
    }
    public async Task<List<Reservation>> GetMyReservationsAsync()
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authenticationState.User;
        var currentUserId = user.FindFirst("sub")?.Value;
        var currentUserGuid = Guid.Parse(currentUserId);
        return await _context.Reservations
            .Where(r => r.UserId == currentUserGuid)
            .OrderBy(r => r.StartTime)
            .ToListAsync();
    }

    public async Task<List<AppointmentSlot>> GetSlotsAsync(long roomId, DateTime? weekStart)
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

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;

        return date.AddDays(-diff).Date;
    }
}