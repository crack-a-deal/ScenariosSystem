using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Networking
{
    public class MessageFramer
    {
        private const int HEADER_SIZE = sizeof(int);
        private const int MAX_MESSAGE_SIZE = 1024 * 1024;

        public async Task SendAsync(NetworkStream stream, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            if (data.Length <= 0)
            {
                throw new InvalidDataException("Message cannot be empty");
            }

            if (data.Length > MAX_MESSAGE_SIZE)
            {
                throw new InvalidDataException("Message exceeds maximum size");
            }

            byte[] header = new byte[HEADER_SIZE];
            BinaryPrimitives.WriteInt32BigEndian(header, data.Length);

            await stream.WriteAsync(header, cancellationToken);
            await stream.WriteAsync(data, cancellationToken);
        }

        public async Task<byte[]> ReadAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            byte[] header = new byte[HEADER_SIZE];

            await ReadExactAsync(stream, header, cancellationToken);

            int length = BinaryPrimitives.ReadInt32BigEndian(header);

            if (length <= 0)
            {
                throw new InvalidDataException("Message cannot be empty");
            }

            if (length > MAX_MESSAGE_SIZE)
            {
                throw new InvalidDataException("Message exceeds maximum size");
            }

            byte[] data = new byte[length];

            await ReadExactAsync(stream, data, cancellationToken);

            return data;
        }

        private async Task ReadExactAsync(NetworkStream stream, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer[offset..], cancellationToken);

                if (read == 0)
                {
                    throw new EndOfStreamException("Stream closed before reading expected data");
                }

                offset += read;
            }
        }
    }
}
