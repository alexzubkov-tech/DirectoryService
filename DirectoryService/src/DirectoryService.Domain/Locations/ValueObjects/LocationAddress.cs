using CSharpFunctionalExtensions;
using Shared;
using Shared.Extensions;

namespace DirectoryService.Domain.Locations.ValueObjects;

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
            return GeneralErrors.ValueIsRequired("Country");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            return GeneralErrors.ValueIsRequired("City");
        }

        if (string.IsNullOrWhiteSpace(street))
        {
            return GeneralErrors.ValueIsRequired("Street");
        }

        if (string.IsNullOrWhiteSpace(buildingNumber))
        {
            return GeneralErrors.ValueIsRequired("BuildingNumber");
        }

        var address = new LocationAddress(
            country.NormalizeSpaces(),
            city.NormalizeSpaces(),
            street.NormalizeSpaces(),
            buildingNumber.NormalizeSpaces()
        );

        return address;
    }
}