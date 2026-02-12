namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(string Name, AddressDto AddressDto, string Timezone);