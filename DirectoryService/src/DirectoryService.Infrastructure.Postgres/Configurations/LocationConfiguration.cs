using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id).HasName("pk_locations");

        builder.Property(l => l.Id)
            .HasColumnName("location_id")
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => new LocationId(value));

        builder.OwnsOne(l => l.LocationName, ln =>
        {
            ln.Property(x => x.Value)
                .HasColumnName("location_name")
                .HasMaxLength(LengthConstants.LENGTH120)
                .IsRequired();

            ln.HasIndex(x => x.Value)
                .HasDatabaseName("ix_locations_name")
                .IsUnique();
        });

        builder.OwnsOne(l => l.LocationAddress, la =>
        {
            la.ToJson("address");
        });

        builder.OwnsOne(l => l.Timezone, tz =>
        {
            tz.Property(x => x.Value)
                .HasColumnName("timezone")
                .IsRequired();
        });

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasQueryFilter(l => l.IsActive);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasMany(l => l.DepartmentLocations)
            .WithOne()
            .HasForeignKey(l => l.LocationId);
    }
}