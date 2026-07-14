namespace StudySpot.Models;

public class Reservation
{
    public Guid ReservationId { get; set; }

    public long RoomId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Status { get; set; }

    public Room? Room { get; set; }
    public User? User { get; set; }
}
