using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace EvilElectricCorpMod
{
    public class ToggleableReactorLogic
    {
        private readonly IMyReactor _reactor;
        private readonly IMyInventory _inventory;
        private readonly MyObjectBuilder_Ingot _energyBuilder;
        private readonly SerializableDefinitionId _energyId;
        private bool _running = false;

        public bool Running
        {
            get { return _running; }
            set
            {
                if (_running != value)
                {
                    _running = value;
                    SetRunning(_running);
                }
            }
        }

        public ToggleableReactorLogic(IMyReactor reactor, string energySubtypeName)
        {
            _reactor = reactor;
            _inventory = reactor.GetInventory();
            _energyBuilder = new MyObjectBuilder_Ingot() { SubtypeName = energySubtypeName };
            _energyId = new SerializableDefinitionId(typeof(MyObjectBuilder_Ingot), energySubtypeName);
        }

        private void SetRunning(bool running)
        {
            if (running)
            {
                Run();
            }
            else
            {
                Stop();
            }
        }

        private void Run()
        {
            if (Globals.IsServer)
            {
                int deficit = 1000 - (int)_inventory.GetItemAmount(_energyId);
                _inventory.AddItems(deficit, _energyBuilder);
            }
        }

        private void Stop()
        {
            if (Globals.IsServer)
            {
                _inventory.RemoveItemsOfType(_inventory.GetItemAmount(_energyId), _energyBuilder);
            }
        }
    }
}
