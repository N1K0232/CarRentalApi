using CarRentalApi.DataAccessLayer.Configurations.Common;
using CarRentalApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalApi.DataAccessLayer.Configurations;

public class VehicleConfiguration : BaseEntityConfiguration<Vehicle>
{
    public override void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.Property(v => v.Brand).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Model).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Plate).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Description).HasMaxLength(512).IsRequired(false);
        builder.Property(v => v.ReleaseDate).IsRequired();

        base.Configure(builder);
    }
}