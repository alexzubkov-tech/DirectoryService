using DirectoryService.Domain.Departments.ValueObjects;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.Departments;

[Collection("Sequential")]
public class DepartmentsCleanupBackgroundServiceTests : DirectoryServiceBaseTests
{
    public DepartmentsCleanupBackgroundServiceTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Cleanup_should_not_delete_active_department()
    {
        var locationId = await CreateLocation("TestLocActive");
        var deptId = await CreateDepartment("ActiveDept", "active-dept", locationIds: [locationId.Value]);

        await RunCleanupAsync();

        await ExecuteInDb(async db =>
        {
            var dept = await db.Departments.FirstOrDefaultAsync(d => d.Id == new DepartmentId(deptId));
            Assert.NotNull(dept);
            Assert.True(dept.IsActive);
        });
    }

    [Fact]
    public async Task Cleanup_should_not_delete_recently_deactivated_department()
    {
        var locationId = await CreateLocation("TestLocRecent");
        var deptId = await CreateDepartment("RecentDept", "recent-dept", locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(deptId, daysAgo: 10);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var dept = await db.Departments.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == new DepartmentId(deptId));
            Assert.NotNull(dept);
            Assert.False(dept.IsActive);
        });
    }

    [Fact]
    public async Task Cleanup_should_delete_department_inactive_longer_than_threshold()
    {
        var locationId = await CreateLocation("TestLocOld");
        var deptId = await CreateDepartment("OldDept", "old-dept", locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(deptId, 31);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var dept = await db.Departments.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == new DepartmentId(deptId));
            Assert.Null(dept);
        });
    }

    [Fact]
    public async Task Cleanup_should_delete_department_locations_when_department_deleted()
    {
        var locationId = await CreateLocation("TestLocLink");
        var deptId = await CreateDepartment("LinkedDept", "linked-dept", locationIds: [locationId.Value]);

        await ExecuteInDb(async db =>
        {
            var link = await db.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == new DepartmentId(deptId));
            Assert.NotNull(link);
        });

        await DeactivateDepartmentDaysAgo(deptId, 31);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var link = await db.DepartmentLocations
                .FirstOrDefaultAsync(dl => dl.DepartmentId == new DepartmentId(deptId));
            Assert.Null(link);
        });
    }

    [Fact]
    public async Task Cleanup_should_delete_department_positions_when_department_deleted()
    {
        var locationId = await CreateLocation("TestLocPos");
        var deptId = await CreateDepartment("PosDept", "pos-dept", locationIds: [locationId.Value]);

        await CreatePosition(deptId);

        await ExecuteInDb(async db =>
        {
            var link = await db.DepartmentPositions
                .FirstOrDefaultAsync(dp => dp.DepartmentId == new DepartmentId(deptId));
            Assert.NotNull(link);
        });

        await DeactivateDepartmentDaysAgo(deptId, 31);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var link = await db.DepartmentPositions
                .FirstOrDefaultAsync(dp => dp.DepartmentId == new DepartmentId(deptId));
            Assert.Null(link);
        });
    }

    [Fact]
    public async Task Cleanup_should_not_delete_children_when_parent_deleted()
    {
        var locationId = await CreateLocation("TestLocTree");
        var hqId = await CreateDepartment("HQDept", "hq-dept", locationIds: [locationId.Value]);
        var itId = await CreateDepartment("ITDept", "it-dept", parentId: hqId, locationIds: [locationId.Value]);
        var devId = await CreateDepartment("DevDept", "dev-dept", parentId: itId, locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(itId, 31);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var it = await db.Departments.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == new DepartmentId(itId));
            Assert.Null(it);

            var hq = await db.Departments.FirstOrDefaultAsync(d => d.Id == new DepartmentId(hqId));
            Assert.NotNull(hq);

            var dev = await db.Departments.FirstOrDefaultAsync(d => d.Id == new DepartmentId(devId));
            Assert.NotNull(dev);
            Assert.True(dev.IsActive);
        });
    }

    [Fact]
    public async Task Cleanup_should_update_children_path_when_parent_deleted()
    {
        var locationId = await CreateLocation("TestLocPath");
        var hqId = await CreateDepartment("HQDept", "hq-dept", locationIds: [locationId.Value]);
        var itId = await CreateDepartment("ITDept", "it-dept", parentId: hqId, locationIds: [locationId.Value]);
        var devId = await CreateDepartment("DevDept", "dev-dept", parentId: itId, locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(itId, 31);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var dev = await db.Departments.FirstAsync(d => d.Id == new DepartmentId(devId));
            Assert.Equal("hq-dept.dev-dept", dev.DepartmentPath.Value);
            Assert.Equal(1, dev.Depth);
            Assert.Equal(hqId, dev.ParentId!.Value);
        });
    }

    [Fact]
    public async Task Cleanup_should_update_deep_descendants_path()
    {
        var locationId = await CreateLocation("TestLocDeep");
        var hqId = await CreateDepartment("HQDept", "hq-dept", locationIds: [locationId.Value]);
        var itId = await CreateDepartment("ITDept", "it-dept", parentId: hqId, locationIds: [locationId.Value]);
        var devId = await CreateDepartment("DevDept", "dev-dept", parentId: itId, locationIds: [locationId.Value]);
        var backendId = await CreateDepartment("BackendDept", "backend-dept", parentId: devId, locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(itId, 31);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var dev = await db.Departments.FirstAsync(d => d.Id == new DepartmentId(devId));
            Assert.Equal("hq-dept.dev-dept", dev.DepartmentPath.Value);
            Assert.Equal(1, dev.Depth);

            var backend = await db.Departments.FirstAsync(d => d.Id == new DepartmentId(backendId));
            Assert.Equal("hq-dept.dev-dept.backend-dept", backend.DepartmentPath.Value);
            Assert.Equal(2, backend.Depth);
        });
    }

    [Fact]
    public async Task Cleanup_should_promote_children_to_root_when_root_deleted()
    {
        var locationId = await CreateLocation("TestLocRoot");
        var hqId = await CreateDepartment("HQDept", "hq-dept", locationIds: [locationId.Value]);
        var itId = await CreateDepartment("ITDept", "it-dept", parentId: hqId, locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(hqId, 31);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var it = await db.Departments.FirstAsync(d => d.Id == new DepartmentId(itId));
            Assert.Equal("it-dept", it.DepartmentPath.Value);
            Assert.Equal(0, it.Depth);
            Assert.Null(it.ParentId);
        });
    }

    [Fact]
    public async Task Cleanup_should_delete_multiple_departments_in_one_run()
    {
        var locationId = await CreateLocation("TestLocMulti");
        var dept1Id = await CreateDepartment("OldOne", "old-one", locationIds: [locationId.Value]);
        var dept2Id = await CreateDepartment("OldTwo", "old-two", locationIds: [locationId.Value]);
        var dept3Id = await CreateDepartment("ActiveDept", "active-dept", locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(dept1Id, 40);
        await DeactivateDepartmentDaysAgo(dept2Id, 35);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var d1 = await db.Departments.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == new DepartmentId(dept1Id));
            var d2 = await db.Departments.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == new DepartmentId(dept2Id));
            var d3 = await db.Departments
                .FirstOrDefaultAsync(d => d.Id == new DepartmentId(dept3Id));

            Assert.Null(d1);
            Assert.Null(d2);
            Assert.NotNull(d3);
        });
    }

    [Fact]
    public async Task Cleanup_should_not_throw_when_no_departments_to_delete()
    {
        var exception = await Record.ExceptionAsync(() => RunCleanupAsync());
        Assert.Null(exception);
    }

    [Fact]
    public async Task Cleanup_should_not_delete_department_deactivated_exactly_on_threshold()
    {
        var locationId = await CreateLocation("TestLocBoundary");
        var deptId = await CreateDepartment("BoundaryDept", "boundary-dept", locationIds: [locationId.Value]);

        await DeactivateDepartmentDaysAgo(deptId, 30);
        await RunCleanupAsync(30);

        await ExecuteInDb(async db =>
        {
            var dept = await db.Departments.IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == new DepartmentId(deptId));
            Assert.NotNull(dept);
        });
    }
}