// Swims the fish directly away from the threat for a timed burst, staying inside its swimming bubble.
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace FishAI
{
    [Serializable]
    [NodeDescription(
        name: "Flee From Threat",
        description: "Swims away from the Threat for a short burst, staying inside its swimming area.",
        story: "Flee from [Threat] for [Duration] s",
        category: "Fish",
        id: "7d4b2a9c6e1f3b5d8a0c2e4f6b9d1a37")]
    public partial class FishFleeAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Threat;

        [SerializeReference] public BlackboardVariable<float> SwimSpeed = new BlackboardVariable<float>(1.8f);

        [SerializeReference] public BlackboardVariable<float> Duration = new BlackboardVariable<float>(1.5f);

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

            // A fixed burst rather than "until safe", so there is only one flee distance in the whole
            // graph and no chance of it disagreeing with the branch condition.
            m_TimeLeft -= Time.deltaTime;

            if (m_TimeLeft <= 0f)
            {
                return Status.Success;
            }

            Vector3 fishPosition = GameObject.transform.position;

            Vector3 awayFromThreat = fishPosition - Threat.Value.transform.position;

            // Flattened so a player standing on the bank does not make the fish bolt straight up.
            awayFromThreat.y = 0f;

            if (awayFromThreat.sqrMagnitude < 0.0001f)
            {
                Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
                awayFromThreat = new Vector3(randomDirection.x, 0f, randomDirection.y);
            }

            Vector3 escapePoint = fishPosition + awayFromThreat.normalized * 5f;

            escapePoint = FishSwimArea.Clamp(escapePoint, m_Home, LeashRadius.Value, VerticalRange.Value);

            m_Motor.MoveToward(escapePoint, SwimSpeed.Value);

            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (m_Motor != null) m_Motor.Stop();
        }
    }
}
