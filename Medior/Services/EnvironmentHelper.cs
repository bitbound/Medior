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
