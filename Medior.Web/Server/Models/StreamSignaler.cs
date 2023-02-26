using Medior.Shared.Models;

namespace Medior.Web.Server.Models
{
    public class StreamSignaler
    {
        public StreamSignaler(Guid streamId)
        {
            StreamId = streamId;
        }

        public static StreamSignaler Empty { get; } = new(Guid.Empty);

        public SemaphoreSlim EndSignal { get; } = new(0, 1);
        public SemaphoreSlim ReadySignal { get; } = new(0, 1);
        public IAsyncEnumerable<VideoChunk>? Stream { get; set; }
        public Guid StreamId { get; init; }
    }
}
