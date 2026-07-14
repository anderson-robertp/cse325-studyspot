namespace StudySpot.Models;

public class AppointmentSlot
{
    public long RoomId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsAvailable { get; set; }

    public Guid? ReservationId { get; set; }
}
