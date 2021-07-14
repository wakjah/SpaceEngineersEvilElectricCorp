using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace EvilElectricCorpMod.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), true, new string[] { "EvilCorpUplink" })]
    public class EvilCorpUplink : MyGameLogicComponent
    {
        private ToggleableReactorLogic _toggleableReactorLogic;

        public IMyReactor Reactor
        {
            get
            {
                return (IMyReactor)Entity;
            }
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            _toggleableReactorLogic = new ToggleableReactorLogic(Reactor, "RemoteEnergy");

            base.NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;

            ((IMyTerminalBlock)Entity).ShowInInventory = false;
        }

        public override void UpdateAfterSimulation100()
        {
            // TODO currently just assuming exactly 100 ticks
            float timeDelta_s = 100 * 0.0166666675f;

            if (Globals.IsServer)
            {
                TransferPower(timeDelta_s);
            }

            ((IMyTerminalBlock)Entity).ShowInInventory = false;
        }

        private void TransferPower(float timeDelta_s)
        {
            float watts = Reactor.CurrentOutput * 1e6f;
            float joules = timeDelta_s * watts;
            double availableJoules = Globals.PREPAID_POWER.AvailableJoules;
            if (joules > availableJoules || availableJoules < 1000)
            {
                _toggleableReactorLogic.Running = false;
                Globals.PREPAID_POWER.AvailableJoules = 0;
            }
            else
            {
                _toggleableReactorLogic.Running = true;
                Globals.PREPAID_POWER.AvailableJoules -= joules;
            }
        }
    }
}
