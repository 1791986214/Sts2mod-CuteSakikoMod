using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace CuteSakikoMod.CuteSakikoModCode.NetMessage;

public sealed class RelicSwapChoiceMessage : INetMessage
{
    public ulong SourceNetId { get; set; }
    public List<string> RelicIds { get; set; } = new();

    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Info;
    public bool ShouldBuffer => false;
    public bool ShouldBroadcast => true;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(SourceNetId);
        writer.WriteString(string.Join("|", RelicIds ?? new List<string>()));
    }

    public void Deserialize(PacketReader reader)
    {
        SourceNetId = reader.ReadULong();
        var raw = reader.ReadString();
        RelicIds = string.IsNullOrEmpty(raw)
            ? new List<string>()
            : raw.Split('|').ToList();
    }

    public int ToId()
    {
        return 204;
    }
}