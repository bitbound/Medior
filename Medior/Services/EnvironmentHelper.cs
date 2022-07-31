using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IEnvironmentHelper
    {
        bool IsDebug { get; }
    }

    public class EnvironmentHelper : IEnvironmentHelper
    {
        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
