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

    public async Task<RoomAmenity?> CreateRoomAmenityAsync(RoomAmenity roomAmenity)
    {
        var exists = await _context.RoomAmenity.AnyAsync(x =>
            x.RoomId == roomAmenity.RoomId &&
            x.AmenityId == roomAmenity.AmenityId);

        if (exists)
        {
            return null;
        }

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
        foreach (var item in amenities)
        {
            Console.WriteLine(item);
        }
        return amenities ?? new List<long>();
    }

    public async Task DeleteRoomAmenityAsync(long roomId, long amenityId)
    {
        
    }
}