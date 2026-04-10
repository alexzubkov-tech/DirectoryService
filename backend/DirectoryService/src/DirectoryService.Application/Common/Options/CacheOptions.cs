namespace DirectoryService.Application.Common.Options;

public class CacheOptions
{
    public int LocalCacheExpirationMinutes { get; set; }

    public int DistributedCacheExpirationMinutes { get; set; }
}