// One conversation: who is talking, the pages of text, and the choices offered at the end.
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue")]

public class DialogueData : ScriptableObject
{
    // Speaker name shown above the lines. Empty falls back to the asset's file name.
    public string SpeakerName;

    // One entry per page, revealed one character at a time. Empty means the dialogue cannot start.
    [TextArea] public string[] lines;

    // Buttons shown after the last page. Leave empty to simply end the conversation.
    public Choice[] choices;

}

[System.Serializable] 

public class Choice
{
    // Label drawn on the button.
    public string text;

    // Conversation to open when clicked. Null ends the dialogue.
    public DialogueData next;

}
