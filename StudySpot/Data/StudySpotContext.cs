using Microsoft.EntityFrameworkCore;
using StudySpot.Models;

namespace StudySpot.Data;

public class StudySpotContext : DbContext
{
    public StudySpotContext(DbContextOptions<StudySpotContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Amenity> Amenity => Set<Amenity>();
    public DbSet<RoomAmenity> RoomAmenity => Set<RoomAmenity>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Room>(entity =>
        {

            entity.HasKey(r => r.RoomId);

            entity.Property(r => r.RoomId).HasColumnName("room_id");
            entity.Property(r => r.RoomName).HasColumnName("room_name");
            entity.Property(r => r.Location).HasColumnName("location");
            entity.Property(r => r.Capacity).HasColumnName("capacity");
            entity.Property(r => r.Description).HasColumnName("description");
            entity.Property(r => r.IsActive).HasColumnName("is_active");
            entity.Property(r => r.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<Reservation>(entity =>
        {

            entity.HasKey(r => r.ReservationId);

            entity.Property(r => r.ReservationId).HasColumnName("reservation_id");
            entity.Property(r => r.RoomId).HasColumnName("room_id");
            entity.Property(r => r.UserId).HasColumnName("user_id");
            entity.Property(r => r.StartTime).HasColumnName("start_time");
            entity.Property(r => r.EndTime).HasColumnName("end_time");
            entity.Property(r => r.Status).HasColumnName("status");
            entity.Property(r => r.CreatedAt).HasColumnName("created_at");
            entity.Property(r => r.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(r => r.Room)
                .WithMany(r => r.Reservations)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Amenity>(entity =>
        {
            entity.ToTable("Amenities");
            
            entity.HasKey(a => a.AmenityId);

            entity.Property(a => a.AmenityId).HasColumnName("amenity_id");
            entity.Property(a => a.Name).HasColumnName("name");
            entity.Property(a => a.Description).HasColumnName("description");
            entity.Property(a => a.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            
            entity.HasKey(u => u.UserId);

            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.FirstName).HasColumnName("first_name");
            entity.Property(u => u.LastName).HasColumnName("last_name");
            entity.Property(u => u.Email).HasColumnName("email");
            entity.Property(u => u.Role).HasColumnName("role");
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<RoomAmenity>(entity =>
        {
            entity.ToTable("Room_amenities");

            entity.HasKey(ra => new { ra.RoomId, ra.AmenityId });
            entity.Property(ra => ra.RoomAmenityId).HasColumnName("room_amenity_id");
            entity.Property(ra => ra.RoomId).HasColumnName("room_id");
            entity.Property(ra => ra.AmenityId).HasColumnName("amenity_id");
        });
    }
}