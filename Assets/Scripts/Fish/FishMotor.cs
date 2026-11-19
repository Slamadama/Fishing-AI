// Swims this transform toward a requested world point with acceleration and smooth turning; nothing else in the project moves a fish.
using UnityEngine;

public class FishMotor : MonoBehaviour
{
    [Header("Swim Settings")]
    [SerializeField] private float maxSpeed = 3.0f;
    [SerializeField] private float acceleration = 4.0f;
    [SerializeField] private float turnSpeed = 4.0f;

    private Vector3 desiredVelocity;
    private Vector3 currentVelocity;

    public void MoveToward(Vector3 worldPoint, float speedMultiplier = 1.0f)
    {
        Vector3 toTarget = worldPoint - transform.position;

        if (toTarget.sqrMagnitude < 0.0001f)
        {
            desiredVelocity = Vector3.zero;
            return;
        }

        desiredVelocity = toTarget.normalized * (maxSpeed * speedMultiplier);
    }

    public void Stop()
    {
        desiredVelocity = Vector3.zero;
    }

    public bool HasArrived(Vector3 worldPoint, float tolerance = 0.5f)
    {
        return Vector3.Distance(transform.position, worldPoint) <= tolerance;
    }

    private void Update()
    {
        currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, acceleration * Time.deltaTime);

        transform.position += currentVelocity * Time.deltaTime;

        // Yaw only: pitching into the travel vector makes the fish nose-dive.
        if (currentVelocity.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(currentVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}
