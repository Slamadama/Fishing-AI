// Keeps a destination inside a fish's home bubble: horizontal leash radius and vertical range.
using UnityEngine;

namespace FishAI
{
    internal static class FishSwimArea
    {
        public static Vector3 Clamp(Vector3 point, Vector3 home, float leashRadius, float verticalRange)
        {
            Vector3 offset = point - home;
            offset.y = 0f;

            float squareRadius = leashRadius * leashRadius;

            if (offset.sqrMagnitude > squareRadius)
            {
                offset = offset.normalized * leashRadius;

                point.x = home.x + offset.x;
                point.z = home.z + offset.z;
            }

            float range = Mathf.Abs(verticalRange);

            point.y = Mathf.Clamp(point.y, home.y - range, home.y + range);

            return point;
        }

        public static Vector3 RandomPoint(Vector3 home, float leashRadius, float verticalRange)
        {
            Vector2 flat = Random.insideUnitCircle * leashRadius;

            float height = Random.Range(-verticalRange, verticalRange);

            Vector3 point = home + new Vector3(flat.x, height, flat.y);

            return Clamp(point, home, leashRadius, verticalRange);
        }
    }
}
