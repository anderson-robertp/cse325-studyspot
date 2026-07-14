namespace StudySpot.Models;
public class Room
{
    public long RoomId { get; set; }

    public string RoomName { get; set; } = "";

    public string Location { get; set; } = "";

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();
}