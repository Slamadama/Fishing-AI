// Sweeps a sphere out of the camera during the thrust's strike window and damages each SpearTarget it touches once per swing.
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SpearHit : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Animator animator;
    [SerializeField] private Camera viewCamera;

    [Header("Attack State")]
    [SerializeField] private string attackStateName = "Thrust";
    [SerializeField] private int animatorLayer = 0;

    [Header("Strike Window (seconds into the clip)")]
    [SerializeField] private float strikeStart = 0.17f;
    [SerializeField] private float strikeEnd = 0.50f;

    [Header("Reach")]
    [SerializeField] private float reach = 2.2f;
    [SerializeField] private float radius = 0.16f;
    [SerializeField] private float castStart = 0.30f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    private bool swinging;
    private float lastClipTime;

    private readonly HashSet<SpearTarget> hitThisSwing = new HashSet<SpearTarget>();
    private readonly RaycastHit[] hits = new RaycastHit[32];

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (viewCamera == null) viewCamera = GetComponentInParent<Camera>();
    }

    private void Update()
    {
        if (viewCamera == null) return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(animatorLayer);

        if (!state.IsName(attackStateName))
        {
            swinging = false;
            return;
        }

        float clipTime = state.normalizedTime * state.length;

        // The clip time jumping backwards is the animator rewinding, i.e. the start of a new swing.
        if (!swinging || clipTime < lastClipTime - 0.0001f)
        {
            swinging = true;
            hitThisSwing.Clear();
        }
        lastClipTime = clipTime;

        if (clipTime < strikeStart) return;
        if (clipTime > strikeEnd) return;

        CastForTargets();
    }

    private void CastForTargets()
    {
        Vector3 origin = viewCamera.transform.position + viewCamera.transform.forward * castStart;
        float distance = Mathf.Max(0.05f, reach - castStart);

        // Filtering by component rather than layer means water, terrain and the player's own body
        // are ignored without any layer setup.
        int count = Physics.SphereCastNonAlloc(origin, radius, viewCamera.transform.forward,
                                               hits, distance, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Collider collider = hits[i].collider;
            if (collider == null) continue;

            SpearTarget target = collider.GetComponentInParent<SpearTarget>();
            if (target == null) continue;
            if (target.IsDead) continue;

            if (hitThisSwing.Contains(target)) continue;
            hitThisSwing.Add(target);

            target.TakeHit(damage, hits[i].point, viewCamera.transform.forward);
        }
    }

    // Orange stab volume, drawn in the Scene view while this object is selected.
    private void OnDrawGizmosSelected()
    {
        Camera cam = viewCamera != null ? viewCamera : GetComponentInParent<Camera>();
        if (cam == null) return;

        Vector3 origin = cam.transform.position + cam.transform.forward * castStart;
        Vector3 end = origin + cam.transform.forward * Mathf.Max(0.05f, reach - castStart);

        Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.35f);
        Gizmos.DrawWireSphere(origin, radius);
        Gizmos.DrawWireSphere(end, radius);
        Gizmos.DrawLine(origin, end);
    }
}
