namespace Medior.Web.Server.Services
{
    public interface IAppSettings
    {
        int FileRetentionDays { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration _configuration;

        public AppSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int FileRetentionDays => _configuration.GetValue<int>(nameof(FileRetentionDays));
    }
}
