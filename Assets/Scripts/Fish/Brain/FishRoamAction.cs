// Swims the fish to random points inside a bubble around its starting position, pausing briefly at each one.
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace FishAI
{
    [Serializable]
    [NodeDescription(
        name: "Swim Around",
        description: "Swims to random points near where the fish started, pausing briefly at each one.",
        story: "Swim around near [RoamRadius] m of home",
        category: "Fish",
        id: "3c9a1e7b5d2f4a8c9b0e6d1f3a7c5b28")]
    public partial class FishRoamAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<float> RoamRadius = new BlackboardVariable<float>(5f);

        [SerializeReference] public BlackboardVariable<float> VerticalRange = new BlackboardVariable<float>(2f);

        [SerializeReference] public BlackboardVariable<float> SwimSpeed = new BlackboardVariable<float>(0.6f);

        [SerializeReference] public BlackboardVariable<float> PauseSeconds = new BlackboardVariable<float>(1.5f);

        [SerializeReference] public BlackboardVariable<float> ArriveTolerance = new BlackboardVariable<float>(0.6f);

        private FishMotor m_Motor;
        private Vector3 m_Home;
        private Vector3 m_Target;
        private float m_PauseTimer;

        protected override void OnSetup()
        {
            m_Motor = GameObject.GetComponent<FishMotor>();

            // Cached once so the bubble cannot drift across the lake over time.
            m_Home = GameObject.transform.position;
        }

        protected override Status OnStart()
        {
            m_Target = FishSwimArea.RandomPoint(m_Home, RoamRadius.Value, VerticalRange.Value);

            m_PauseTimer = 0f;

            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (m_Motor == null)
            {
                LogFailure("No FishMotor component found on this GameObject.");
                return Status.Failure;
            }

            if (m_PauseTimer > 0f)
            {
                m_PauseTimer -= Time.deltaTime;
                m_Motor.Stop();

                return Status.Running;
            }

            if (m_Motor.HasArrived(m_Target, ArriveTolerance.Value))
            {
                m_PauseTimer = PauseSeconds.Value;
                m_Target = FishSwimArea.RandomPoint(m_Home, RoamRadius.Value, VerticalRange.Value);

                m_Motor.Stop();

                return Status.Running;
            }

            m_Motor.MoveToward(m_Target, SwimSpeed.Value);

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (m_Motor != null) m_Motor.Stop();
        }
    }
}
