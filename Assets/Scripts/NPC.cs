using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")] 
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;

    [Header("Quest Settings")] 
    public Quest questToGive;
    private DialogueLine[] currentDialogue;
    public string npcID;
    public bool isQuestGiver = true;

    [Header("Animations")]
    public AnimatedSpriteRenderer anim;
    public Sprite[] walkUp;
    public Sprite[] walkDown;
    public Sprite[] walkLeft;
    public Sprite[] walkRight;
    public Sprite[] idleUp;
    public Sprite[] idleDown;
    public Sprite[] idleLeft;
    public Sprite[] idleRight;

    private int dialogueIndex;
    private bool isDialogueActive;
    private bool isTyping;
    private bool isOnCooldown = false;
    private Coroutine typingCoroutine;
    private Coroutine autoCoroutine;
    private WaypointMover mover;
    private Vector2 lastMovement;

    void Start()
    {
        mover = GetComponent<WaypointMover>();
    }

    void Update()
    {
        if (isDialogueActive)
        {
            HandleDialogue();
            return;
        }

        UpdateAnimation();
    }

    public void Interact()
    {
        if (isOnCooldown) return;
        if (!isDialogueActive) StartDialogue();
    }

    void HandleDialogue()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentDialogue[dialogueIndex - 1].text;
                isTyping = false;
            }
            else
            {
                DisplayNextSentence();
            }
        }
    }

    void StartDialogue()
    {
        if (mover) mover.isFrozen = true; // Stop NPC movement while dialogue is running
        
        PlayerInteraction pi = FindAnyObjectByType<PlayerInteraction>();
        pi?.interactPrompt?.SetActive(false); //hide interact prompt when dialogue is running

        dialogueIndex = 0;
        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        nameText.text = dialogueData.npcName;

        //quest related
        if (!string.IsNullOrEmpty(npcID) && questToGive != null)
        {
            var qp = QuestManager.Instance.GetQuest(questToGive.questID);
            if (qp != null)
            {
                bool otherObjectivesComplete = true;
                foreach (var obj in qp.objectives)
                {
                    if (obj.type == ObjectiveType.CollectItem)
                    {
                        InventoryController inv = FindAnyObjectByType<InventoryController>();
                        int count = inv.GetQuestItemCount(obj.itemName);
                        if (count < obj.requiredAmount)
                        {
                            otherObjectivesComplete = false;
                            break;
                        }
                    }
                    else if (obj.type != ObjectiveType.TalkNPC && !obj.IsCompleted)
                    {
                        otherObjectivesComplete = false;
                        break;
                    }
                }
                if (otherObjectivesComplete)
                {
                    var inv = FindAnyObjectByType<InventoryController>();
                    foreach (var obj in qp.objectives)
                    {
                        if (obj.type == ObjectiveType.CollectItem)
                        {
                            obj.currentAmount = inv.GetQuestItemCount(obj.itemName);
                        }
                    }
                    QuestManager.Instance.RegisterNPCTalk(npcID);
                }
            }
        }
        else if (!string.IsNullOrEmpty(npcID) && questToGive == null) QuestManager.Instance.RegisterNPCTalk(npcID);

        if (questToGive == null)
        {
            currentDialogue = dialogueData.beforeQuest;
        }
        else if (!QuestManager.Instance.IsQuestActive(questToGive.questID))
        {
            currentDialogue = dialogueData.beforeQuest;
            if (isQuestGiver) QuestManager.Instance.AcceptQuest(questToGive);
        }
        else if (!QuestManager.Instance.IsQuestCompleted(questToGive.questID))
        {
            currentDialogue = dialogueData.duringQuest;
        }
        else
        {
            currentDialogue = dialogueData.afterQuest;
        }

        DisplayNextSentence();
    }

    void DisplayNextSentence()
    {
        if (dialogueIndex >= currentDialogue.Length)
        {
            EndDialogue();
            return;
        }
        DialogueLine line = currentDialogue[dialogueIndex];

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        typingCoroutine = StartCoroutine(TypeSentence(line.text));

        if (line.autoProgress) 
        {
            autoCoroutine = StartCoroutine(AutoNextLine(dialogueData.autoProgressDelay));
        }
        dialogueIndex++;
    }

    void EndDialogue()
    {
        if (mover) mover.isFrozen = false; // NPC-movement starts again when dialogue has ended

        isDialogueActive = false;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        if (autoCoroutine != null) StopCoroutine(autoCoroutine);
 
        dialoguePanel.SetActive(false);

        StartCoroutine(DialogueCooldown());

        PlayerInteraction pi = FindAnyObjectByType<PlayerInteraction>();
        if (pi != null) pi.UpdatePromptUI(); //Interact prompts visible again when dialogue has ended
    }

    private void UpdateAnimation()
    {
        if (!mover || !anim) return;
        Vector2 movement = mover.isFrozen ? Vector2.zero : mover.CurrentMovement;

        Sprite[] animToPlay;
        
        if (movement == Vector2.zero)
        {
            animToPlay = idleDown; //default 
            if (lastMovement.x > 0) animToPlay = idleRight;
            else if (lastMovement.x < 0) animToPlay = idleLeft;
            else if (lastMovement.y > 0) animToPlay = idleUp;
            else if (lastMovement.y < 0) animToPlay = idleDown;
            if (animToPlay == null || animToPlay.Length == 0) return;
        }
        else
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                animToPlay = movement.x > 0 ? walkRight : walkLeft;
            else
                animToPlay = movement.y > 0 ? walkUp : walkDown;
        }
        if (anim.IsPlaying(animToPlay)) return;
        anim.PlayAnimation(animToPlay, true);
    }

    IEnumerator AutoNextLine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isDialogueActive) yield break; // Om dialogen avslutats under väntetiden, avbryt)
        if (isTyping) yield break; // Om texten fortfarande skrivs, avbryt auto-progress
        DisplayNextSentence();
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;
    }

    private IEnumerator DialogueCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(1f);
        isOnCooldown = false;
    }
}