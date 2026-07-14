namespace StudySpot.DTOs;
public class RoomReservationCountDto
{
    public long RoomId { get; set; }
    public string RoomName { get; set; } = "";
    public int ReservationCount { get; set; }
}