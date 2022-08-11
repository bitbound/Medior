﻿using MessagePack;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medior.Shared.Dtos.Wrapped;

namespace Medior.Shared.Helpers
{
    public static class DtoChunker
    {
        private static readonly MemoryCache _cache = new(new MemoryCacheOptions());

        public static IEnumerable<DtoWrapper> ChunkDto<T>(T dto, DtoType dtoType, Guid requestId = default, int chunkSize = 50_000)
        {
            var dtoBytes = MessagePackSerializer.Serialize(dto);
            var responseId = Guid.NewGuid();
            var chunks = dtoBytes.Chunk(chunkSize).ToArray();
            
            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];

                yield return new DtoWrapper()
                {
                    DtoChunk = chunk,
                    DtoType = dtoType,
                    SequenceId = i,
                    IsFirstChunk = i == 0,
                    IsLastChunk = i == chunks.Length - 1,
                    RequestId = requestId,
                    ResponseId = responseId
                };
            }
        }

        public static bool TryComplete<T>(DtoWrapper wrapper, out T? result)
        {
            result = default;

            var chunks = _cache.GetOrCreate(wrapper.ResponseId, entry => {
                entry.SlidingExpiration = TimeSpan.FromMinutes(1);
                return new List<DtoWrapper>();
            });

            lock (chunks)
            {
                chunks.Add(wrapper);

                if (!wrapper.IsLastChunk)
                {
                    return false;
                }

                _cache.Remove(wrapper.ResponseId);

                chunks.Sort((a, b) => {
                    return a.SequenceId - b.SequenceId;
                });

                var buffer = chunks.SelectMany(x => x.DtoChunk).ToArray();

                result = MessagePackSerializer.Deserialize<T>(buffer);
                return true;
            }
        }
    }
}
