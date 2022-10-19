using CarRentalApi.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarRentalApi.DataAccessLayer.Configurations.Common;

public abstract class DeletableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity> where TEntity : DeletableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.IsDeleted).IsRequired();
        builder.Property(e => e.DeletedDate).IsRequired(false);

        base.Configure(builder);
    }
}