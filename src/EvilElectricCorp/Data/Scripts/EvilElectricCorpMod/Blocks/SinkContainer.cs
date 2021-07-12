using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace EvilElectricCorpMod.Blocks
{

    struct PowerCreditDescriptor
    {
        public SerializableDefinitionId Id;
        public double JoulesPerCredit;

        public PowerCreditDescriptor(string subtypeId, double kilowattHoursPerCredit)
        {
            Id = new SerializableDefinitionId(typeof(MyObjectBuilder_PhysicalObject), subtypeId);
            JoulesPerCredit = joulesFromKWh(kilowattHoursPerCredit);
        }

        private static double joulesFromKWh(double kWh)
        {
            return kWh * 3600 * 1000;
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CargoContainer), true, new string[] { "SinkContainer" })]
    public class SinkCargoContainer : MyGameLogicComponent
    {
        private static readonly PowerCreditDescriptor[] POWER_CREDITS = {
            new PowerCreditDescriptor("PowerCredit1", 2.0),
            new PowerCreditDescriptor("PowerCredit2", 20.0),
            new PowerCreditDescriptor("PowerCredit3", 200.0)
        };

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            if (!Globals.IsServer)
            {
                return;
            }

            Entity.NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();

            if (!Globals.IsServer)
            {
                return;
            }

            foreach (var descriptor in POWER_CREDITS)
            {
                FindAndRemoveCredits(descriptor);
            }
        }

        private void FindAndRemoveCredits(PowerCreditDescriptor ofType)
        {
            var cargoContainer = Entity as IMyCargoContainer;
            var inventoryItem = cargoContainer.GetInventory().FindItem(ofType.Id);
            if (inventoryItem != null)
            {
                cargoContainer.GetInventory().RemoveItemAmount(inventoryItem, inventoryItem.Amount);
                Globals.PREPAID_POWER.AvailableJoules += 1e-6 * inventoryItem.Amount.RawValue * ofType.JoulesPerCredit;
            }
        }
    }
}
