// Holds health for anything the spear can hit, and plays a flinch on a hit or a roll-and-sink on death.
using System.Collections;
using UnityEngine;

public class SpearTarget : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int health = 2;

    [Header("Hit Reaction")]
    [SerializeField] private float flinchScale = 1.25f;
    [SerializeField] private float flinchTime = 0.18f;

    [Header("Death")]
    [SerializeField] private float deathTime = 0.8f;
    [SerializeField] private float sinkDistance = 1.4f;
    [SerializeField] private bool deactivateOnDeath = true;

    private Vector3 startScale;
    private bool dead;
    private Coroutine flinchRoutine;

    public bool IsDead { get { return dead; } }
    public int Health { get { return health; } }

    private void Awake()
    {
        startScale = transform.localScale;
    }

    public void TakeHit(int damage, Vector3 hitPoint, Vector3 direction)
    {
        if (dead) return;

        health -= Mathf.Max(1, damage);

        Debug.Log("[SpearTarget] " + name + " was hit for " + Mathf.Max(1, damage) +
                  ". Health left: " + Mathf.Max(0, health), this);

        if (health <= 0)
        {
            Die();
            return;
        }

        if (flinchRoutine != null)
        {
            StopCoroutine(flinchRoutine);
            transform.localScale = startScale;
        }
        flinchRoutine = StartCoroutine(FlinchRoutine());
    }

    private IEnumerator FlinchRoutine()
    {
        float elapsed = 0f;

        while (elapsed < flinchTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / flinchTime);

            // Sin rises to 1 and back to 0, so the body always returns to exactly its original size.
            float punch = Mathf.Sin(progress * Mathf.PI);

            transform.localScale = startScale * (1f + (flinchScale - 1f) * punch);

            yield return null;
        }

        transform.localScale = startScale;
        flinchRoutine = null;
    }

    private void Die()
    {
        dead = true;
        Debug.Log("[SpearTarget] " + name + " died.", this);
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // One frame's delay: disabling a Behaviour graph agent inside its own tick upsets the package.
        yield return null;

        // FishMotor and the graph agent both write position and rotation every frame, so they have to
        // stop before this script rolls the body over.
        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour == this) continue;
            behaviour.enabled = false;
        }

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, 180f);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * sinkDistance;

        float elapsed = 0f;

        while (elapsed < deathTime)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / deathTime);
            float eased = Mathf.SmoothStep(0f, 1f, progress);

            transform.rotation = Quaternion.Slerp(startRotation, endRotation, eased);
            transform.position = Vector3.Lerp(startPosition, endPosition, eased);

            yield return null;
        }

        transform.rotation = endRotation;
        transform.position = endPosition;

        if (deactivateOnDeath) gameObject.SetActive(false);
    }
}
