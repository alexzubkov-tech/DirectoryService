using DirectoryService.Domain;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration: IEntityTypeConfiguration<Position>
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

        builder.Property(p => p.PositionName)
            .HasConversion(pn => pn.Value, name => PositionName.Create(name).Value)
            .IsRequired()
            .HasColumnName("position_name")
            .HasMaxLength(LengthConstants.LENGTH100);

        builder.HasIndex(p => p.PositionName)
            .IsUnique();

        builder.Property(p => p.PositionDescription)
            .HasConversion(
                p => p.Value,
                description => PositionDescription.Create(description).Value)
            .IsRequired(false)
            .HasColumnName("position_description")
            .HasMaxLength(LengthConstants.LENGTH1000);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasQueryFilter(p => p.IsActive);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasMany(p => p.DepartmentPositions)
            .WithOne()
            .HasForeignKey(d => d.PositionId);

    }
}