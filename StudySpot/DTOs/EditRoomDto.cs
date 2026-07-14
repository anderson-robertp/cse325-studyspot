namespace StudySpot.DTOs;
public class EditRoomDto
{
    public long RoomId { get; set; }
    public string RoomName { get; set; } = "";
    public string Location { get; set; } = "";
    public int Capacity { get; set; }
    public string Description { get; set; } = "";

    public List<AmenitySelectionDto> Amenities { get; set; } = new();
}

public class AmenitySelectionDto
{
    public long AmenityId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsSelected { get; set; }
}