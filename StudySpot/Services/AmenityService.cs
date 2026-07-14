using System.Net.Http.Json;
using StudySpot.Models;
using StudySpot.DTOs;
using Microsoft.EntityFrameworkCore;
using StudySpot.Data;

namespace StudySpot.Services;

public class AmenityService
{
    private readonly StudySpotContext _context;

    public AmenityService(StudySpotContext context)
    {
        _context = context;
    }

    public async Task<List<AmenitySelectionDto>> GetAmenitiesAsync()
    {
        var amenities = await _context.Amenity.Select(a => new AmenitySelectionDto
        {
            AmenityId = a.AmenityId,
            Name = a.Name
        }).ToListAsync();
        return amenities;
    }

    public async Task<Amenity?> GetAmenityByIdAsync(long amenityId)
    {
        var amenity = await _context.Amenity.FindAsync(amenityId);
        return amenity;
    }

    public async Task<Amenity> CreateAmenityAsync(Amenity amenity)
    {
        _context.Amenity.Add(amenity);
        await _context.SaveChangesAsync();
        return amenity;
    }

    public async Task<Amenity> UpdateAmenityAsync(Amenity amenity)
    {
        _context.Amenity.Update(amenity);
        await _context.SaveChangesAsync();
        return amenity;
    }

    public async Task DeleteAmenityAsync(long amenityId)
    {
        var amenity = await _context.Amenity.FindAsync(amenityId);
        if (amenity != null)
        {
            _context.Amenity.Remove(amenity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetAmenityCountAsync()
    {
        return await _context.Amenity.CountAsync();
    }
}