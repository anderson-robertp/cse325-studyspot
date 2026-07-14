namespace StudySpot.Models;
public class Amenity
{
    public long AmenityId { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; }
}