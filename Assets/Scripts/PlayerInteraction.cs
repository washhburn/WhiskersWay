using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    private List<IInteractable> currentInteractables = new List<IInteractable>();

    public GameObject interactPrompt;
    public TMP_Text interactPromptText;

    private bool uiOpen = false;

    void Start()
    {
        Transform ui = GameObject.Find("UI").transform;
        interactPrompt = ui.Find("InteractPrompt").gameObject;
        if (interactPrompt != null) interactPromptText = interactPrompt.GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        if (currentInteractables.Count == 0) return;

        if (uiOpen)
        {
            //stäng UI popup, prioritera Sign interaktion
            foreach (var interactable in currentInteractables)
            {
                Sign sign = (interactable as MonoBehaviour)?.GetComponent<Sign>();

                if (sign != null && sign.IsShowing)
                {
                    sign.Interact();
                    return;
                }
            }
            return;
        }

        //UI popup stängd, prioritera FishingSign interaktion
        foreach (var interactable in currentInteractables)
        {
            if (interactable is FishingSign fishing)
            {
                fishing.Interact();
                return;
            }
        }

        //vanligt interaktion med signs
        foreach (var interactable in currentInteractables)
        {
            interactable.Interact();
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactables = other.GetComponentsInParent<IInteractable>();

        foreach (var interactable in interactables)
        {
            if (currentInteractables.Contains(interactable)) continue;

            currentInteractables.Add(interactable);

            if (interactable is not FishingSign) SetPromptText("Press F to interact");

            Sign sign = interactable as Sign
                ?? (interactable as MonoBehaviour)?.GetComponent<Sign>();

            if (sign != null)
            { 
                sign.onUIStateChanged += SetUIState;
                sign.ShowText();
            }
        }
        UpdatePromptUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var interactables = other.GetComponentsInParent<IInteractable>();

        foreach (var interactable in interactables)
        {
            if (!currentInteractables.Contains(interactable)) continue;

            currentInteractables.Remove(interactable);

            Sign sign = interactable as Sign
                ?? (interactable as MonoBehaviour)?.GetComponent<Sign>();

            if (sign != null)
            { 
                sign.onUIStateChanged -= SetUIState;
                if (sign.closeOnExit)
                {
                    sign.CloseText();
                    sign.ResetClosed();
                }
            }
        }
        if (currentInteractables.Count == 0) uiOpen = false;
        UpdatePromptUI();
    }

    private void SetUIState (bool open)
    {
        uiOpen = open;
        UpdatePromptUI();
    }

    public void UpdatePromptUI()
    {
        if (interactPrompt == null)
        {
            return;
        }
        interactPrompt.SetActive(currentInteractables.Count > 0 && !uiOpen);
    }

    public void SetPromptText(string text)
    {
        if (interactPromptText != null) interactPromptText.text = text;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject ui = GameObject.Find("UI");

        if (ui != null)
        {
            Transform t = ui.transform.Find("InteractPrompt");
            if (t != null)
            {
                interactPrompt = t.gameObject;
                interactPromptText = interactPrompt.GetComponentInChildren<TMP_Text>();
            }
        }

        currentInteractables.Clear();
    }
}
