using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRageMath;

namespace EvilElectricCorpMod.Blocks
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false, new string[] { "HandCrank" })]
    public class HandCrank : MyGameLogicComponent
    {
        public bool ProductionEnabled
        {
            get { return _toggleableReactorLogic.Running; }
            set { _toggleableReactorLogic.Running = value; }
        }

        public bool Spin
        {
            get { return _spinningSubpartLogic.ShouldSpin; }
            set { _spinningSubpartLogic.ShouldSpin = value; }
        }

        private ToggleableReactorLogic _toggleableReactorLogic;
        private SpinningSubpartLogic _spinningSubpartLogic;

        public IMyReactor Reactor { get { return (IMyReactor)Entity; } }

        public HandCrank()
        {
            _spinningSubpartLogic = new SpinningSubpartLogic("HandCrankRotor_section");
            _spinningSubpartLogic.RotationAxis = Vector3.Up;
        }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            _spinningSubpartLogic.Init(this);
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            Reactor.ShowInInventory = false;

            _toggleableReactorLogic = new ToggleableReactorLogic(Reactor, "HandCrankEnergy");

            _spinningSubpartLogic.UpdateOnceBeforeFrame();

            NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation()
        {
            _spinningSubpartLogic.UpdateBeforeSimulation();
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();

            Reactor.ShowInInventory = false;
        }
    }
}
