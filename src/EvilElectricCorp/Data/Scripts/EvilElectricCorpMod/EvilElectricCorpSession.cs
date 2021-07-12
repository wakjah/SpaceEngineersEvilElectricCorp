using EvilElectricCorpMod.Blocks;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;

namespace EvilElectricCorpMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class EvilElectricCorpSession : MySessionComponentBase
    {
        private HandCrank _usingHandCrank = null;

        public override void BeforeStart()
        {
            base.BeforeStart();

            MyDefinitionManager definitions = MyDefinitionManager.Static;
            foreach (var definition in definitions.GetDefinitionsOfType<MyBatteryBlockDefinition>())
            {
                definition.InitialStoredPowerRatio = 0;
            }

            foreach (var definition in definitions.GetDefinitionsOfType<MyPowerProducerDefinition>())
            {
                bool shouldRemainWorking = definition is MyBatteryBlockDefinition
                    || IsBlockDefinitionFromThisMod(definition);

                if (shouldRemainWorking)
                {
                    continue;
                }

                definition.MaxPowerOutput = 0;
            }
        }

        public bool IsBlockDefinitionFromThisMod(MyDefinitionBase definition)
        {
            return definition.Context.ModId == ModContext.ModId
                && definition.Context.ModName == ModContext.ModName;
        }

        public override void HandleInput()
        {
            base.HandleInput();

            var input = MyAPIGateway.Input;

            bool usePressed = input.IsNewGameControlPressed(MyControlsSpace.USE);
            bool useReleased = input.IsNewGameControlReleased(MyControlsSpace.USE);
            HandCrank handCrank = GetCurrentUseObjectHandCrank();

            if (usePressed && handCrank != null)
            {
                Use(handCrank);
            }
            else if (useReleased)
            {
                Use(null);
            }
        }

        private HandCrank GetCurrentUseObjectHandCrank()
        {
            var detectorComp = MyAPIGateway.Session?.Player?.Character?.Components?.Get<MyCharacterDetectorComponent>();
            if (detectorComp?.UseObject != null)
            {
                var owner = detectorComp.UseObject.Owner;
                foreach (var c in owner.Components)
                {
                    if (c is HandCrank)
                    {
                        return c as HandCrank;
                    }
                }
            }
            return null;
        }

        private void Use(HandCrank crank)
        {
            if (_usingHandCrank == crank)
            {
                return;
            }

            StopCrank();
            StartCrank(crank);
        }

        private void StopCrank()
        {
            if (_usingHandCrank != null)
            {
                _usingHandCrank.ProductionEnabled = false;
            }
            _usingHandCrank = null;
        }

        private void StartCrank(HandCrank crank)
        {
            if (crank != null)
            {
                crank.ProductionEnabled = true;
            }
            _usingHandCrank = crank;
        }
    }
}
