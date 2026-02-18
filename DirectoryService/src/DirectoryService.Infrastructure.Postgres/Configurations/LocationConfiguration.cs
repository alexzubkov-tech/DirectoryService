using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration: IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id).HasName("pk_locations");

        builder.Property(l => l.Id)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(l => l.LocationName)
            .HasConversion(ln => ln.Value, name => LocationName.Create(name).Value)
            .IsRequired()
            .HasColumnName("location_name")
            .HasMaxLength(LengthConstants.LENGTH120);

        builder.HasIndex(l => l.LocationName)
            .IsUnique();

        builder.OwnsOne(l => l.LocationAddress, la =>
        {
            la.ToJson("address");
        });

        builder.Property(l => l.Timezone)
            .HasConversion(lt => lt.Value, timezone => LocationTimeZone.Create(timezone).Value)
            .IsRequired()
            .HasColumnName("timezone");

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasQueryFilter(l => l.IsActive);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

    }
}