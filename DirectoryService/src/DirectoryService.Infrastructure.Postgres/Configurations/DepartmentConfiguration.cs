using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_department");

        builder.Property(d => d.Id)
            .HasColumnName("department_id")
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => new DepartmentId(value));

        builder.OwnsOne(d => d.DepartmentName, dn =>
        {
            dn.Property(x => x.Value)
                .HasColumnName("department_name")
                .HasMaxLength(LengthConstants.LENGTH150)
                .IsRequired();
        });

        builder.OwnsOne(d => d.DepartmentIdentifier, di =>
        {
            di.Property(x => x.Value)
                .HasColumnName("department_identifier")
                .HasMaxLength(LengthConstants.LENGTH150)
                .IsRequired();

            di.HasIndex(x => x.Value)
                .HasDatabaseName("ix_department_identifier")
                .IsUnique();
        });

        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .IsRequired(false)
            .HasConversion(
                value => value!.Value,
                value => new DepartmentId(value));

        builder.OwnsOne(d => d.DepartmentPath, dp =>
        {
            dp.Property(x => x.Value)
                .HasColumnName("department_path")
                .HasColumnType("ltree");

            dp.HasIndex(x => x.Value)
                .HasMethod("gist")
                .HasDatabaseName("idx_department_path");
        });

        builder.Property(d => d.Depth)
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasQueryFilter(d => d.IsActive);

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasMany(d => d.ChildrenDepartments)
            .WithOne()
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.DepartmentLocations)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);

        builder.HasMany(d => d.DepartmentPositions)
            .WithOne()
            .HasForeignKey(d => d.DepartmentId);
    }
}