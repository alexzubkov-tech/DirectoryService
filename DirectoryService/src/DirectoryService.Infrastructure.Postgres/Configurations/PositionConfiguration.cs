using DirectoryService.Domain;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("position_id")
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => new PositionId(value));

        builder.OwnsOne(p => p.PositionName, pn =>
        {
            pn.Property(x => x.Value)
                .HasColumnName("position_name")
                .HasMaxLength(LengthConstants.LENGTH100)
                .IsRequired();

            pn.HasIndex(x => x.Value)
                .HasDatabaseName("ix_positions_name_active")
                .IsUnique()
                .HasFilter("\"is_active\" = true");
        });

        builder.OwnsOne(p => p.PositionDescription, pd =>
        {
            pd.Property(x => x.Value)
                .HasColumnName("position_description")
                .HasMaxLength(LengthConstants.LENGTH1000);
        });

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasQueryFilter(p => p.IsActive);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasMany(p => p.DepartmentPositions)
            .WithOne()
            .HasForeignKey(d => d.PositionId);
    }
}