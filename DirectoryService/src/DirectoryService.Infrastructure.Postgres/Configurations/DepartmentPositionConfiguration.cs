using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration: IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");

        builder.HasKey(dp => dp.Id).HasName("pk_department_positions");

        builder.Property(dp => dp.Id)
            .HasColumnName("department_position_id")
            .IsRequired();

        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();

        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id")
            .IsRequired();

        builder.HasOne<Department>()
            .WithMany(d => d.DepartmentPositions)
            .HasForeignKey(dp => dp.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>()
            .WithMany(p => p.DepartmentPositions)
            .HasForeignKey(dp => dp.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(dp => new { dp.DepartmentId, dp.PositionId })
            .IsUnique();

    }
}