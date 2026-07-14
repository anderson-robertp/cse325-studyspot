using System.Net.Http.Json;
using StudySpot.Models;
using StudySpot.DTOs;
using Microsoft.EntityFrameworkCore;
using StudySpot.Data;

namespace StudySpot.Services;

public class RoomService
{
    private readonly StudySpotContext _context;

    public RoomService(StudySpotContext context)
    {
        _context = context;
    }

    public async Task<List<Room>> GetRoomsAsync()
    {
        var rooms = await _context.Rooms.ToListAsync();
        return rooms ?? new List<Room>();
    }

    public async Task<Room?> GetRoomByIdAsync(long roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        return room;
    }

    public async Task<Room> CreateRoomAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> UpdateRoomAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task DeleteRoomAsync(long roomId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<RoomAmenity>> GetRoomAmenitiesByRoomIdAsync(long roomId)
    {
        var roomAmenities = await _context.RoomAmenity
            .Where(ra => ra.RoomId == roomId)
            .ToListAsync();
        return roomAmenities ?? new List<RoomAmenity>();
    }

    public async Task<int> GetRoomCountAsync()
    {
        return await _context.Rooms.CountAsync();
    }

    public async Task<List<PopularRoomDto>> GetMostPopularRoomsAsync()
    {
        return await _context.Rooms
            .Select(room => new PopularRoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                ReservationCount = room.Reservations.Count()
            })
            .OrderByDescending(room => room.ReservationCount)
            .Take(5)
            .ToListAsync();
    }

    
}