using EvilElectricCorpMod.Blocks;
using EvilElectricCorpMod.Networking;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities.Character.Components;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Utils;

namespace EvilElectricCorpMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class EvilElectricCorpSession : MySessionComponentBase
    {
        private HandCrank _usingHandCrank = null;
        private Transceiver<EvilElectricCorpCommand> _transceiver;

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

            _transceiver = new Transceiver<EvilElectricCorpCommand>(63961, HandleCommand);
            _transceiver.Register();
        }

        protected override void UnloadData()
        {
            _transceiver?.Unregister();
            _transceiver = null;
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
                return GetHandCrankFromEntity(detectorComp.UseObject.Owner);
            }
            return null;
        }

        private static HandCrank GetHandCrankFromEntity(IMyEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            foreach (var c in entity.Components)
            {
                if (c is HandCrank)
                {
                    return c as HandCrank;
                }
            }
            return null;
        }

        // Runs client-side to keep track of currently in-use crank by a player and request that the 
        // server start / stop a crank
        private void Use(HandCrank crank)
        {
            if (_usingHandCrank == crank)
            {
                return;
            }

            if (_usingHandCrank != null) 
            {
                RequestStopCrank(_usingHandCrank);
            }
            if (crank != null)
            {
                RequestStartCrank(crank);
            }
            _usingHandCrank = crank;
        }

        private void RequestStartCrank(HandCrank crank)
        {
            _transceiver.SendToServer(new EvilElectricCorpCommand(crank.Entity.EntityId, CommandType.RequestHandCrankStart));
        }

        private void RequestStopCrank(HandCrank crank)
        {
            _transceiver.SendToServer(new EvilElectricCorpCommand(crank.Entity.EntityId, CommandType.RequestHandCrankStop));
        }

        private void NotifyCrankStarted(HandCrank crank)
        {
            _transceiver.Broadcast(new EvilElectricCorpCommand(crank.Entity.EntityId, CommandType.HandCrankStarted));
        }

        private void NotifyCrankStopped(HandCrank crank)
        {
            _transceiver.Broadcast(new EvilElectricCorpCommand(crank.Entity.EntityId, CommandType.HandCrankStopped));
        }

        bool HandleCommand(EvilElectricCorpCommand command)
        {
            /*MyLog.Default.WriteLineAndConsole("" + command.Command);
            MyAPIGateway.Utilities.ShowNotification("" + command.Command);*/

            switch (command.Command)
            {
                case CommandType.RequestHandCrankStart:
                case CommandType.RequestHandCrankStop:
                    HandleHandCrankRequest(command.EntityId, command.Command == CommandType.RequestHandCrankStart);
                    break;
                case CommandType.HandCrankStarted:
                case CommandType.HandCrankStopped:
                    HandleHandCrankResponse(command.EntityId, command.Command == CommandType.HandCrankStarted);
                    break;
            }

            return false; // do not relay
        }

        private void HandleHandCrankRequest(long entityId, bool start)
        {
            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);
            HandCrank crank = GetHandCrankFromEntity(entity);

            if (crank == null)
            {
                return;
            }

            if (start)
            {
                crank.ProductionEnabled = true;
                NotifyCrankStarted(crank);
            }
            else
            {
                crank.ProductionEnabled = false;
                NotifyCrankStopped(crank);
            }
        }

        private void HandleHandCrankResponse(long entityId, bool start)
        {
            IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);
            HandCrank crank = GetHandCrankFromEntity(entity);
            if (crank == null)
            {
                return;
            }

            if (start)
            {
                crank.Spin = true;
            }
            else
            {
                crank.Spin = false;
            }
        }
    }
}
