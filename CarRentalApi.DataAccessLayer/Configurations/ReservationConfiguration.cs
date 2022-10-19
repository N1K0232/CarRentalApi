using CarRentalApi.DataAccessLayer.Configurations.Common;
using CarRentalApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalApi.DataAccessLayer.Configurations;

public class ReservationConfiguration : DeletableEntityConfiguration<Reservation>
{
    public override void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.Property(r => r.ReservationStart).IsRequired();
        builder.Property(r => r.ReservationEnd).IsRequired();

        builder.HasOne(r => r.Person)
            .WithMany(p => p.Reservations)
            .HasForeignKey(r => r.PersonId)
            .IsRequired();

        builder.HasOne(r => r.Vehicle)
            .WithMany(p => p.Reservations)
            .HasForeignKey(r => r.VehicleId)
            .IsRequired();

        base.Configure(builder);
    }
}