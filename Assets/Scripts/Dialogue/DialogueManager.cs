// Runs one conversation: typewriter text, page advance on Space, and the choice buttons at the end.
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // Filled in Awake so the NPC can find the single manager without a scene reference.
    public static DialogueManager Instance;

    public GameObject panel;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab;
    public float typespeed = 0.03f;

    // Read by the NPC to keep the prompt hidden while a conversation is open.
    public bool IsTalking { get; private set; }

    private DialogueData current;
    private int index;
    private bool typing;
    private bool choosing;

    private void Awake()
    {
        Instance = this;

        // The panel is saved visible in the scene, so it is closed here before the first frame.
        panel.SetActive(false);
    }

    void Update()
    {
        // Space is also bound to Jump in the input actions, so jumping advances the dialogue too.
        if (!IsTalking || choosing) return;
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (typing) SkipTyping();
            else NextLine();
        }
    }

    public void StartDialogue(DialogueData data)
    {
        // Opening the panel with nothing to type would leave the placeholder text on screen.
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            EndDialogue();
            return;
        }

        StopAllCoroutines();
        current = data;
        index = 0;
        IsTalking = true;
        panel.SetActive(true);

        // Falls back to the asset name so an unnamed speaker still shows something.
        nameText.text = string.IsNullOrEmpty(data.SpeakerName) ? data.name : data.SpeakerName;

        // Choices are clicked with the mouse, so the cursor cannot stay locked during a conversation.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        typing = true;
        dialogueText.text = "";

        // With an empty lines array this throws, leaving the panel open on whatever text it already had.
        foreach (char c in current.lines[index])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typespeed);
        }

        FinishLine();
    }

    void FinishLine()
    {
        typing = false;
        if (index == current.lines.Length - 1) ShowChoices();
    }

    void SkipTyping()
    {
        StopAllCoroutines();
        dialogueText.text = current.lines[index];
        FinishLine();
    }

    void ShowChoices()
    {
        if (current.choices == null || current.choices.Length == 0) return;
        choosing = true;

        foreach (Choice choice in current.choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choiceContainer);
            button.GetComponentInChildren<TMP_Text>().text = choice.text;
            button.onClick.AddListener(() => PickChoice(choice));
        }
    }

    void PickChoice(Choice choice)
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);
        choosing = false;

        // next is what decides whether the conversation continues; text is only the button label.
        if (choice.next != null) StartDialogue(choice.next);
        else EndDialogue();
    }

    void EndDialogue()
    {
        StopAllCoroutines();
        typing = false;
        choosing = false;

        IsTalking = false;
        panel.SetActive(false);

        // Give the mouse back to the player controller, which locked it in Start.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void NextLine()
    {
        index++;
        if (index < current.lines.Length) StartCoroutine(TypeLine());
        else EndDialogue();
    }
}
