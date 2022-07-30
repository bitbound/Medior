using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface ISystemTime
    {
        DateTimeOffset Now { get; }
        DateTimeOffset UtcNow { get; }

        DateTimeOffset Offset(TimeSpan offset);
        void Restore();
        void Set(DateTimeOffset time);
    }

    public class SystemTime : ISystemTime
    {
        private TimeSpan? _offset;
        private DateTimeOffset? _time;

        public DateTimeOffset Now
        {
            get
            {
                var baseTime = _time ?? DateTimeOffset.Now;
                if (_offset.HasValue)
                {
                    return baseTime.Add(_offset.Value);
                }
                return baseTime;
            }
        }

        public DateTimeOffset UtcNow => _time ?? DateTimeOffset.UtcNow;

        public DateTimeOffset Offset(TimeSpan offset)
        {
            if (_offset.HasValue)
            {
                _offset = _offset.Value.Add(offset);
            }
            else
            {
                _offset = offset;
            }

            return Now;
        }

        public void Restore()
        {
            _offset = null;
            _time = null;
        }

        public void Set(DateTimeOffset time)
        {
            _offset = null;
            _time = time;
        }
    }
}
