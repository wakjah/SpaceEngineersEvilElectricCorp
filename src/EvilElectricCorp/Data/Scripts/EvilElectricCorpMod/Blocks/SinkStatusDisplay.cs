using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using System;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace EvilElectricCorpMod.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), true, new string[] { "SinkLCDPanel" })]
    public class SinkStatusDisplay : MyGameLogicComponent
    {
        private long _lastPrintedValue = -1;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            if (!Globals.IsServer)
            {
                return;
            }

            Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            if (!Globals.IsServer)
            {
                return;
            }

            double joules = Globals.PREPAID_POWER.AvailableJoules;
            if (joules == _lastPrintedValue)
            {
                return;
            }

            var textPanel = Entity as IMyTextPanel;
            textPanel.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            textPanel.WriteText("Available:\n");
            double kWh = joules / 1000 / 3600;
            textPanel.WriteText(String.Format("{0:F2} kWh", kWh), true);
        }
    }
}
