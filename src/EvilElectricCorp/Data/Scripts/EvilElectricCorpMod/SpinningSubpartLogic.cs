using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace EvilElectricCorpMod
{
    public class SpinningSubpartLogic
    {
        private const float DEFAULT_DEGREES_PER_TICK = 1.5f;
        private const float DEFAULT_ACCELERATE_PERCENT_PER_TICK = 0.05f;
        private const float DEFAULT_DECELERATE_PERCENT_PER_TICK = 0.01f;
        private static readonly Vector3 DEFAULT_ROTATION_AXIS = Vector3.Forward;

        private readonly string _subpartName; // dummy name without the "subpart_" prefix
        public float DegreesPerTick { get; set; } = DEFAULT_DEGREES_PER_TICK; // rotation per tick in degrees (60 ticks per second)
        public float AcceleratePercentPerTick { get; set; } = DEFAULT_ACCELERATE_PERCENT_PER_TICK; // aceleration percent of "DEGREES_PER_TICK" per tick.
        public float DeceleratePercentPerTick { get; set; } = DEFAULT_DECELERATE_PERCENT_PER_TICK; // deaccleration percent of "DEGREES_PER_TICK" per tick.
        public Vector3 RotationAxis { get; set; } = DEFAULT_ROTATION_AXIS; // rotation axis for the subpart, you can do new Vector3(0.0f, 0.0f, 0.0f) for custom values

        private const float MAX_DISTANCE_SQ = 1000 * 1000; // player camera must be under this distance (squared) to see the subpart spinning

        private IMyFunctionalBlock block;
        private bool subpartFirstFind = true;
        private Matrix subpartLocalMatrix; // keeping the matrix here because subparts are being re-created on paint, resetting their orientations
        private float targetSpeedMultiplier; // used for smooth transition

        private MyGameLogicComponent _component;

        public bool ShouldSpin { get; set; }

        public SpinningSubpartLogic(string subpartName)
        {
            _subpartName = subpartName;
        }

        public void Init(MyGameLogicComponent component)
        {
            _component = component;
            component.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public void UpdateOnceBeforeFrame()
        {
            if (MyAPIGateway.Utilities.IsDedicated)
                return;

            block = (IMyFunctionalBlock)_component.Entity;

            if (block.CubeGrid?.Physics == null)
                return;

            _component.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        public void UpdateBeforeSimulation()
        {
            try
            {
                bool shouldSpin = ShouldSpin && block.IsWorking; // if block is functional and enabled

                if (!shouldSpin && Math.Abs(targetSpeedMultiplier) < 0.00001f)
                    return;

                if (shouldSpin && targetSpeedMultiplier < 1)
                {
                    targetSpeedMultiplier = Math.Min(targetSpeedMultiplier + AcceleratePercentPerTick, 1);
                }
                else if (!shouldSpin && targetSpeedMultiplier > 0)
                {
                    targetSpeedMultiplier = Math.Max(targetSpeedMultiplier - DeceleratePercentPerTick, 0);
                }

                var camPos = MyAPIGateway.Session.Camera.WorldMatrix.Translation; // local machine camera position

                if (Vector3D.DistanceSquared(camPos, block.GetPosition()) > MAX_DISTANCE_SQ)
                    return;

                MyEntitySubpart subpart;
                if (_component.Entity.TryGetSubpart(_subpartName, out subpart)) // subpart does not exist when block is in build stage
                {
                    if (subpartFirstFind) // first time the subpart was found
                    {
                        subpartFirstFind = false;
                        subpartLocalMatrix = subpart.PositionComp.LocalMatrixRef;
                    }

                    if (targetSpeedMultiplier > 0)
                    {
                        subpartLocalMatrix *= Matrix.CreateFromAxisAngle(RotationAxis, MathHelper.ToRadians(targetSpeedMultiplier * DegreesPerTick));
                        subpartLocalMatrix = Matrix.Normalize(subpartLocalMatrix); // normalize to avoid any rotation inaccuracies over time resulting in weird scaling
                    }

                    subpart.PositionComp.SetLocalMatrix(ref subpartLocalMatrix);
                }
            }
            catch (Exception e)
            {
                AddToLog(e);
            }
        }

        private void AddToLog(Exception e)
        {
            MyLog.Default.WriteLineAndConsole($"ERROR {GetType().FullName}: {e.ToString()}");

            if (MyAPIGateway.Session?.Player != null)
                MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
        }
    }
}
