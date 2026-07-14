using System.Net.Http.Json;
using StudySpot.Models;
using StudySpot.DTOs;
using Microsoft.EntityFrameworkCore;
using StudySpot.Data;

namespace StudySpot.Services;

public class RoomAmenityService
{
    private readonly StudySpotContext _context;

    public RoomAmenityService(StudySpotContext context)
    {
        _context = context;
    }
    public async Task<List<RoomAmenity>> GetRoomAmenitiesAsync()
    {
        var roomAmenities = await _context.RoomAmenity.ToListAsync();
        return roomAmenities ?? new List<RoomAmenity>();
    }

    public async Task<RoomAmenity?> GetRoomAmenityByIdAsync(long roomAmenityId)
    {
        var roomAmenity = await _context.RoomAmenity.FindAsync(roomAmenityId);
        return roomAmenity;
    }

    public async Task<RoomAmenity> CreateRoomAmenityAsync(RoomAmenity roomAmenity)
    {
        _context.RoomAmenity.Add(roomAmenity);
        await _context.SaveChangesAsync();
        return roomAmenity;
    }

    public async Task<RoomAmenity> UpdateRoomAmenityAsync(RoomAmenity roomAmenity)
    {
        _context.RoomAmenity.Update(roomAmenity);
        await _context.SaveChangesAsync();
        return roomAmenity;
    }

    public async Task<List<long>> GetAmenitiesByRoomIdAsync(long roomId)
    {
        var amenities = await _context.RoomAmenity
            .Where(ra => ra.RoomId == roomId)
            .Select(ra => ra.AmenityId)
            .ToListAsync();
        return amenities ?? new List<long>();
    }
}