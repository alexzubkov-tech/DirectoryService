namespace DirectoryService.Contracts.Locations;

public record UpdateLocationRequest(
    string Name,
    AddressDto AddressDto,
    string Timezone,
    bool IsActive);
