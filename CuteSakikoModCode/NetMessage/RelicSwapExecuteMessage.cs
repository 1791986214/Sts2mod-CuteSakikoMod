using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace CuteSakikoMod.CuteSakikoModCode.NetMessage
{
    public sealed class RelicSwapExecuteMessage : INetMessage
    {
        public List<string> Moves { get; set; } = new();

        public NetTransferMode Mode => NetTransferMode.Reliable;
        public LogLevel LogLevel => LogLevel.Info;
        public bool ShouldBuffer => false;
        public bool ShouldBroadcast => true;

        public void Serialize(PacketWriter writer)
        {
            writer.WriteString(string.Join(";", Moves ?? new List<string>()));
        }

        public void Deserialize(PacketReader reader)
        {
            var raw = reader.ReadString();
            Moves = string.IsNullOrEmpty(raw)
                ? new List<string>()
                : raw.Split(';').ToList();
        }

        public int ToId() => 206;
    }
}