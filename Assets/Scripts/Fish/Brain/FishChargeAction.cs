// Swims the fish straight at the threat for a timed burst, stopping short and staying inside its swimming bubble.
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace FishAI
{
    [Serializable]
    [NodeDescription(
        name: "Charge At Threat",
        description: "Swims straight at the Threat for a burst, then re-checks the graph.",
        story: "Charge at [Threat] for [Duration] s",
        category: "Fish",
        id: "9e5c3b1a8d7f2e4c6a0b3d5f7e9c1b48")]
    public partial class FishChargeAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Threat;

        [SerializeReference] public BlackboardVariable<float> SwimSpeed = new BlackboardVariable<float>(1.4f);

        [SerializeReference] public BlackboardVariable<float> Duration = new BlackboardVariable<float>(5f);

        [SerializeReference] public BlackboardVariable<float> StopDistance = new BlackboardVariable<float>(1.2f);

        [SerializeReference] public BlackboardVariable<float> LeashRadius = new BlackboardVariable<float>(6f);

        [SerializeReference] public BlackboardVariable<float> VerticalRange = new BlackboardVariable<float>(2.5f);

        private FishMotor m_Motor;
        private Vector3 m_Home;
        private float m_TimeLeft;

        protected override void OnSetup()
        {
            m_Motor = GameObject.GetComponent<FishMotor>();
            m_Home = GameObject.transform.position;
        }

        protected override Status OnStart()
        {
            m_TimeLeft = Duration.Value;
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (m_Motor == null)
            {
                LogFailure("No FishMotor component found on this GameObject.");
                return Status.Failure;
            }

            if (Threat == null || Threat.Value == null)
            {
                return Status.Success;
            }

            m_TimeLeft -= Time.deltaTime;

            if (m_TimeLeft <= 0f)
            {
                return Status.Success;
            }

            Vector3 fishPosition = GameObject.transform.position;
            Vector3 threatPosition = Threat.Value.transform.position;

            float distance = Vector3.Distance(fishPosition, threatPosition);

            // Ending the charge here stops the fish vibrating inside the player's model.
            if (distance <= StopDistance.Value)
            {
                return Status.Success;
            }

            Vector3 aimPoint = FishSwimArea.Clamp(threatPosition, m_Home, LeashRadius.Value, VerticalRange.Value);

            m_Motor.MoveToward(aimPoint, SwimSpeed.Value);

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (m_Motor != null) m_Motor.Stop();
        }
    }
}
