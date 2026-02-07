using DirectoryService.Domain;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration: IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_department");

        builder.Property(d => d.Id)
            .HasColumnName("department_id")
            .IsRequired();

        builder.Property(d => d.DepartmentName)
            .HasConversion(dn => dn.Value, name => DepartmentName.Create(name).Value)
            .IsRequired()
            .HasColumnName("department_name")
            .HasMaxLength(LengthConstants.LENGTH150);

        builder.Property(d => d.DepartmentIdentifier)
            .HasConversion(
                di => di.Value,
                identifier => DepartmentIdentifier.Create(identifier).Value)
            .IsRequired()
            .HasColumnName("department_identifier")
            .HasMaxLength(LengthConstants.LENGTH150);

        builder.Property(d => d.ParentId)
            .HasColumnName("parent_id")
            .IsRequired(false);

        builder.Property(d => d.DepartmentPath)
            .HasConversion(dp => dp.Value, path => DepartmentPath.FromString(path))
            .HasColumnName("department_path")
            .IsRequired();

        builder.Property(d => d.Depth)
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        builder.HasQueryFilter(d => d.IsActive);

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasMany(d => d.Children)
            .WithOne(d => d.Parent)
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}