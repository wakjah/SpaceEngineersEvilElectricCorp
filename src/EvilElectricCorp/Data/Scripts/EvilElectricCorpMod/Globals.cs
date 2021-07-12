using Sandbox.ModAPI;
using System;

namespace EvilElectricCorpMod
{
    public class Globals
    {
        public static Guid GUID = new Guid("681911ed-0178-487f-8b3d-654d64e76d98");
        public static string GUID_STR = GUID.ToString();
        public static GlobalPrepaidPower PREPAID_POWER = new GlobalPrepaidPower();

        public static bool IsServer
        {
            get
            {
                return MyAPIGateway.Multiplayer.IsServer || !MyAPIGateway.Multiplayer.MultiplayerActive;
            }
        }
    }
}
