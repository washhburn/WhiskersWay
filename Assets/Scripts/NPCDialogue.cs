using JetBrains.Annotations;
using UnityEngine;


[System.Serializable]
public class DialogueLine
{
    public string text;
    public bool autoProgress;
}

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;

    [Header("Dialogue Lines")]
    public DialogueLine[] beforeQuest;
    public DialogueLine[] duringQuest;
    public DialogueLine[] afterQuest;

    [Header("Settings")]
    public float autoProgressDelay = 2f; // Time to wait before auto-progressing to the next line
    public float typingSpeed = 0.05f; // Time between each character appearing
    public AudioClip voiceSound;
}
