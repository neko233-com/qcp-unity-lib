using System;

namespace Neko233.Qcp.Unity
{
    public enum QcpStream : byte
    {
        Critical = 0,
        Realtime = 1,
        Batch = 2
    }

    public enum QcpPriority : byte
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    [Flags]
    public enum QcpSendFlags : ushort
    {
        None = 0,
        LatestWins = 1 << 0,
        Reliable = 1 << 1
    }

    public readonly struct QcpSendOptions
    {
        public QcpStream Stream { get; set; }
        public int DeadlineMs { get; set; }
        public QcpPriority Priority { get; set; }
        public uint LatestKey { get; set; }
        public QcpSendFlags Flags { get; set; }

        public static QcpSendOptions Realtime(uint latestKey = 0, int deadlineMs = 16)
        {
            return new QcpSendOptions
            {
                Stream = QcpStream.Realtime,
                DeadlineMs = deadlineMs,
                Priority = QcpPriority.Normal,
                LatestKey = latestKey,
                Flags = QcpSendFlags.LatestWins
            };
        }

        public static QcpSendOptions Critical(int deadlineMs = 8)
        {
            return new QcpSendOptions
            {
                Stream = QcpStream.Critical,
                DeadlineMs = deadlineMs,
                Priority = QcpPriority.Critical,
                Flags = QcpSendFlags.Reliable
            };
        }

        public static QcpSendOptions Batch()
        {
            return new QcpSendOptions
            {
                Stream = QcpStream.Batch,
                DeadlineMs = 0,
                Priority = QcpPriority.Low,
                Flags = QcpSendFlags.Reliable
            };
        }
    }

    public readonly struct QcpMessage
    {
        public QcpMessage(byte[] payload, QcpStream stream, ulong sequence, byte pathId)
        {
            Payload = payload;
            Stream = stream;
            Sequence = sequence;
            PathId = pathId;
        }

        public byte[] Payload { get; }
        public QcpStream Stream { get; }
        public ulong Sequence { get; }
        public byte PathId { get; }
    }
}
