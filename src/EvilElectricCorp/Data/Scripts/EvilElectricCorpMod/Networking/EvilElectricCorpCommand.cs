using ProtoBuf;

namespace EvilElectricCorpMod.Networking
{
    [ProtoContract]
    public enum CommandType
    {
        [ProtoEnum]
        RequestHandCrankStart,
        [ProtoEnum]
        RequestHandCrankStop,
        [ProtoEnum]
        HandCrankStarted,
        [ProtoEnum]
        HandCrankStopped
    }

    [ProtoContract]
    public class EvilElectricCorpCommand
    {
        [ProtoMember(1)]
        public CommandType Command;

        [ProtoMember(2)]
        public long EntityId;

        public EvilElectricCorpCommand() { } // Empty constructor required for deserialization

        public EvilElectricCorpCommand(long entityId, CommandType command)
        {
            EntityId = entityId;
            Command = command;
        }

        /*public override bool Received()
        {
            var msg = $"PacketSimpleExample received: Text='{Text}'; Number={Number}";
            MyLog.Default.WriteLineAndConsole(msg);
            MyAPIGateway.Utilities.ShowNotification(msg, Number);

            return true; // relay packet to other clients (only works if server receives it)
        }*/
    }
}
