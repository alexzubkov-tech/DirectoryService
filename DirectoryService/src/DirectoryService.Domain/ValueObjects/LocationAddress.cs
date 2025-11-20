using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects;

public record LocationAddress
{
    public const int MAX_LENGTH = 120;

    private LocationAddress(string country, string city, string street, string buildingNumber)
    {
        Country = country;
        City = city;
        Street = street;
        BuildingNumber = buildingNumber;
    }

    public string Country { get; }

    public string City { get; }

    public string Street { get; }

    public string BuildingNumber { get; }

    public static Result<LocationAddress> Create(
        string country,
        string city,
        string street,
        string buildingNumber)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<LocationAddress>("Country cannot be empty");

        if (country.Length > MAX_LENGTH)
            return Result.Failure<LocationAddress>($"Country must be less than {MAX_LENGTH} characters");

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<LocationAddress>("City cannot be empty");

        if (city.Length > MAX_LENGTH)
            return Result.Failure<LocationAddress>($"City must be less than {MAX_LENGTH} characters");

        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<LocationAddress>("Street cannot be empty");

        if (street.Length > MAX_LENGTH)
            return Result.Failure<LocationAddress>($"Street must be less than {MAX_LENGTH} characters");

        if (string.IsNullOrWhiteSpace(buildingNumber))
            return Result.Failure<LocationAddress>("Building number cannot be empty");

        if (buildingNumber.Length > MAX_LENGTH)
            return Result.Failure<LocationAddress>($"Building number must be less than {MAX_LENGTH} characters");

        var address = new LocationAddress(
            country,
            city,
            street,
            buildingNumber
        );

        return Result.Success(address);
    }
}