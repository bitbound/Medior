using Medior.Shared.Dtos;

namespace Medior.Shared.Interfaces;

public interface IDtoHandler
{
    Task ReceiveDto(DtoWrapper dto);
}
