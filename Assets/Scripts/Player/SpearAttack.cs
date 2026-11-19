// Plays the "Thrust" state on this object's Animator when the player left-clicks, no faster than the cooldown allows.
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class SpearAttack : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Animator animator;

    [Header("Animation")]
    [SerializeField] private string attackStateName = "Thrust";
    [SerializeField] private int animatorLayer = 0;

    [Header("Tuning")]
    [SerializeField] private float cooldown = 0.55f;

    private int attackStateHash;
    private float nextAttackTime;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();

        attackStateHash = Animator.StringToHash(attackStateName);

        // A wrong state name would otherwise throw an error on every single click.
        if (!animator.HasState(animatorLayer, attackStateHash))
        {
            Debug.LogWarning("SpearAttack: the Animator has no state called '" + attackStateName + "'. " +
                             "Open Window > Animation > Animator and check the exact spelling of the state.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (Time.time < nextAttackTime) return;

        animator.Play(attackStateHash, animatorLayer, 0f);

        nextAttackTime = Time.time + cooldown;
    }
}