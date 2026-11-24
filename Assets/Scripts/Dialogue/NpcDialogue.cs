// Shows a "press E" prompt while the player stands inside this NPC's trigger, and starts the conversation.
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcDialogue : MonoBehaviour
{
    public DialogueData dialogue;

    // The prompt object is optional: if this is empty the prompt never appears, but talking still works.
    public GameObject prompt;

    private bool playerNear;

    // Trigger callbacks below only fire when this object has an Is Trigger collider, so playerNear stays false otherwise.
    void Update()
    {
        // Without the manager there is nothing to talk to, and reading Instance would throw every frame.
        if (DialogueManager.Instance == null) return;

        bool canTalk = playerNear && !DialogueManager.Instance.IsTalking;
        if (prompt) prompt.SetActive(canTalk);

        if (canTalk && dialogue != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerNear = false;
    }
}
