namespace StudySpot.DTOs;

public class UpcomingReservationsDto
{
    public Guid ReservationId { get; set; }
    public string Email { get; set; } = "";
    public string RoomName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}