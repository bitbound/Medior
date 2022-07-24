using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Interfaces
{
    internal interface IBackgroundService
    {
        Task Start(CancellationToken cancellationToken);
    }
}
