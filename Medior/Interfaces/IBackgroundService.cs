using System.Threading;
using System.Threading.Tasks;

namespace Medior.Interfaces
{
    public interface IBackgroundService
    {
        Task Start(CancellationToken cancellationToken);
    }
}
