using System.Buffers.Binary;
using System.Security.Cryptography;

namespace IotDashboard.Application.Util
{
    public static class DeviceCommandPacketEncoder
    {
        private const int PacketLength = 34;
        private const int CrcLength = sizeof(uint);

        private static readonly IReadOnlyDictionary<ushort, byte[]> AllowedActions =
            new Dictionary<ushort, byte[]>
            {
                [0x0001] = [0, 1, 3],
                [0x0100] = [0, 1, 3],
                [0x0101] = [0, 1, 3],
                [0x0102] = [0, 1, 3],
                [0x0103] = [0, 1, 3],
                [0x0104] = [0, 1, 2, 3],
                [0x0105] = [0, 1, 3],
                [0x0200] = [0, 1, 2],
                [0x0201] = [0, 1, 2],
                [0x0202] = [0, 1, 2],
                [0x0203] = [0, 1, 2],
                [0x0204] = [0, 1, 2],
                [0x0205] = [0, 1, 2],
                [0x0206] = [0, 1, 2],
                [0x0207] = [0, 1, 2],
                [0x0208] = [0, 1, 2],
                [0x0300] = [0, 1, 2],
                [0x0301] = [0, 1, 2],
                [0x0302] = [0, 1, 3],
                [0x0400] = [4],
                [0x0401] = [4],
                [0x0402] = [4],
                [0x0403] = [4],
                [0x0404] = [0, 1, 4],
                [0x0500] = [4],
                [0x0501] = [4]
            };

        public static bool IsSupportedCommand(ushort targetId, byte action) =>
            AllowedActions.TryGetValue(targetId, out var actions) && actions.Contains(action);

        public static (byte[] Payload, uint RequestId) Encode(
            ushort targetId,
            byte action,
            byte channel,
            ushort durationSeconds)
        {
            if (!IsSupportedCommand(targetId, action))
            {
                throw new ArgumentOutOfRangeException(nameof(targetId), "The target/action pair is not in the supported device command registry.");
            }

            if (action == 3 && durationSeconds == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Pulse commands require a nonzero duration.");
            }

            var packet = new byte[PacketLength];
            packet[0] = 0xA5;
            packet[1] = 0x5A;
            packet[2] = 0x02;
            packet[3] = 0x02;

            var requestId = CreateRequestId();
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(4, sizeof(uint)), requestId);

            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(24, sizeof(ushort)), targetId);
            packet[26] = action;
            packet[27] = channel;
            BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(28, sizeof(ushort)), durationSeconds);
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(PacketLength - CrcLength, CrcLength), ComputeCrc32(packet.AsSpan(0, PacketLength - CrcLength)));

            return (packet, requestId);
        }

        private static uint CreateRequestId()
        {
            Span<byte> bytes = stackalloc byte[sizeof(uint)];
            uint requestId;
            do
            {
                RandomNumberGenerator.Fill(bytes);
                requestId = BinaryPrimitives.ReadUInt32BigEndian(bytes);
            }
            while (requestId == 0);

            return requestId;
        }

        private static uint ComputeCrc32(ReadOnlySpan<byte> data)
        {
            var crc = uint.MaxValue;
            foreach (var value in data)
            {
                crc ^= value;
                for (var bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1) != 0
                        ? (crc >> 1) ^ 0xEDB88320
                        : crc >> 1;
                }
            }

            return ~crc;
        }
    }
}
