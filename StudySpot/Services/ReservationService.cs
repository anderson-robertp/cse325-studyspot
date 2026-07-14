using System.Net.Http.Json;
using StudySpot.Models;
using StudySpot.DTOs;
using Microsoft.EntityFrameworkCore;
using StudySpot.Data;

namespace StudySpot.Services;
public class ReservationService
{
    private readonly StudySpotContext _context;

    public ReservationService(StudySpotContext context)
    {
        _context = context;
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

    public async Task<Reservation> CreateReservationAsync(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
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
        var today = DateTime.Now;

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
}