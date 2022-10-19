using CarRentalApi.DataAccessLayer.Configurations.Common;
using CarRentalApi.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalApi.DataAccessLayer.Configurations;

public class PersonConfiguration : BaseEntityConfiguration<Person>
{
    public override void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");
        builder.Property(p => p.FirstName).HasMaxLength(256).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(256).IsRequired();
        builder.Property(p => p.DateOfBirth).IsRequired();

        base.Configure(builder);
    }
}