using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.Domain.Positions.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id).HasName("pk_department_positions");

        builder.Property(dp => dp.Id)
            .HasColumnName("department_position_id")
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => new DepartmentPositionId(value));

        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => new DepartmentId(value));

        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id")
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => new PositionId(value));
    }
}