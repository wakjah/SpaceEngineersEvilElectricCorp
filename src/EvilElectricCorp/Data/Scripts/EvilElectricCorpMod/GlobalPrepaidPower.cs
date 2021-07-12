using Sandbox.ModAPI;

namespace EvilElectricCorpMod
{
    public class GlobalPrepaidPower
    {
        public double AvailableJoules
        {
            get
            {
                double storedCount;
                MyAPIGateway.Utilities.GetVariable(Globals.GUID_STR, out storedCount);
                return storedCount;
            }
            set
            {
                if (Globals.IsServer)
                {
                    MyAPIGateway.Utilities.SetVariable(Globals.GUID_STR, value);
                }
            }
        }
    }
}
