using Shared;

namespace DirectoryService.Application.Locations.Fails;

public partial class Errors
{
    public static class Locations
    {
        // тут пишем заготовки для типовых ошибок - ПРИМЕР КИРИЛЛА - НАДО ПОМЕНЯТЬ!!!
        public static Error ToManyLocations() =>
            Error.Failure("locations.to.many", "Пользователь не может создать больше 3 локаций");
    }
}