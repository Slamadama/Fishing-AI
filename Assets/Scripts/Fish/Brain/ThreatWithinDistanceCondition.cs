// Answers true while the Threat object is closer than Distance metres to the fish.
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace FishAI
{
    [Serializable]
    [GeneratePropertyBag]
    [Condition(
        name: "Threat Within Distance",
        description: "True while the Threat object is closer than Distance metres to this agent.",
        story: "[Threat] is within [Distance] m",
        category: "Fish",
        id: "b1f7c2d4a9e64f0d8c3b5a7e1d2f6c90")]
    public partial class ThreatWithinDistanceCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<GameObject> Threat;

        [SerializeReference] public BlackboardVariable<float> Distance = new BlackboardVariable<float>(3.5f);

        public override bool IsTrue()
        {
            // A missing threat must never count as "close", or the fish reacts to nothing.
            if (Threat == null) return false;
            if (Threat.Value == null) return false;

            Vector3 fishPosition = GameObject.transform.position;
            Vector3 threatPosition = Threat.Value.transform.position;

            float distance = Vector3.Distance(fishPosition, threatPosition);

            return distance <= Distance.Value;
        }
    }
}
