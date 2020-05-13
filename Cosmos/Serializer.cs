using System.IO;
using System.Serialization;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Sensemaking.Cosmos
{
    internal class Serializer : CosmosSerializer
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);
        private readonly JsonSerializer serializer;
        private readonly JsonSerializerSettings serializerSettings;

        public Serializer()
        {
            this.serializerSettings = Serialization.GetSettings();
            this.serializer = JsonSerializer.Create(this.serializerSettings);
        }

        public override T FromStream<T>(Stream stream)
        {
            using (stream)
            {
                if (typeof(Stream).IsAssignableFrom(typeof(T)))
                    return (T)(object)(stream);

                using (var sr = new StreamReader(stream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                        return serializer.Deserialize<T>(jsonTextReader)!;
                }
            }
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
            {
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    writer.Formatting = Formatting.None;
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