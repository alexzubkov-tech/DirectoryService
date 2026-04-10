namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(string Name, AddressDto AddressDto, string Timezone);