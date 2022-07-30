using Medior.Shared.Entities;

namespace Medior.Web.Server.Models
{
    public class RetrievedFile
    {
        public static RetrievedFile Empty { get; } = new RetrievedFile();
        public bool Found { get; set; }
        public UploadedFile? UploadedFile { get; init; }
        public Stream? FileStream { get; init; }
    }
}
