﻿using System.IO;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Sensemaking.Persistence.Cosmos
{
    public class CosmosJsonNetSerializer : CosmosSerializer
    {
        private static readonly Encoding defaultEncoding = new UTF8Encoding(false, true);
        private readonly JsonSerializer serializer;

        public CosmosJsonNetSerializer()
            : this(new JsonSerializerSettings())
        {
        }

        public CosmosJsonNetSerializer(JsonSerializerSettings serializerSettings)
        {
            this.serializer = JsonSerializer.Create(serializerSettings);
        }

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)(stream);
                }

                using (StreamReader sr = new StreamReader(stream))
                {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(sr))
                    {
                        return serializer.Deserialize<T>(jsonTextReader)!;
                    }
                }
            }
        }

        public override Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            using (StreamWriter streamWriter = new StreamWriter(streamPayload, encoding: defaultEncoding,
                bufferSize: 1024, leaveOpen: true))
            {
                using (JsonWriter writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;
                    serializer.Serialize(writer, input);
                    writer.Flush();
                    streamWriter.Flush();
                }
            }

            streamPayload.Position = 0;
            return streamPayload;
        }
    }
}