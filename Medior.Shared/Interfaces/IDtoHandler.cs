using Medior.Shared.Dtos.Wrapped;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Interfaces
{
    public interface IDtoHandler
    {
        Task ReceiveDto(DtoWrapper dto);
    }
}
