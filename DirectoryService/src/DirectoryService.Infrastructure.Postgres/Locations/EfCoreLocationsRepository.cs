using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Infrastructure.Locations;

public class EfCoreLocationsRepository: ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public EfCoreLocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Error>> AddAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success<Guid, Error>(location.Id);

    }

}