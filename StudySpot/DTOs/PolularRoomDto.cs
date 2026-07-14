namespace StudySpot.DTOs;
public class PopularRoomDto
{
    public long RoomId { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public int ReservationCount { get; set; }
}