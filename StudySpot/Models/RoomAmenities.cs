namespace StudySpot.Models;

public class RoomAmenity
{
    public long RoomAmenityId { get; set; }
    public long RoomId { get; set; }
    public long AmenityId { get; set; }
    public Room Room { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}