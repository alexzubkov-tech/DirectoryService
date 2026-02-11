using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.ValueObjects;

public record LocationAddress
{
    // Для EF Core
    private LocationAddress() { }

    private LocationAddress(string country, string city, string street, string buildingNumber)
    {
        Country = country;
        City = city;
        Street = street;
        BuildingNumber = buildingNumber;
    }

    public string Country { get; private set; }

    public string City { get; private set; }

    public string Street { get; private set; }

    public string BuildingNumber { get; private set; }

    public static Result<LocationAddress, Error> Create(
        string country,
        string city,
        string street,
        string buildingNumber)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return Result.Failure<LocationAddress, Error>(
                Error.Validation("country.empty","Country cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return Result.Failure<LocationAddress, Error>(
                Error.Validation("city.empty","City cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return Result.Failure<LocationAddress, Error>(
                Error.Validation("street.empty","Street cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(buildingNumber))
        {
            return Result.Failure<LocationAddress, Error>(
                Error.Validation("buildingNumber.empty","BuildingNumber cannot be empty"));
        }

        var address = new LocationAddress(
            country,
            city,
            street,
            buildingNumber
        );

        return Result.Success<LocationAddress, Error>(address);
    }
}