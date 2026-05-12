using UnityEngine;
using TMPro;

public class Sign : MonoBehaviour, IInteractable
{
    [TextArea(3, 10)]
    public string text;

    public GameObject textBoxUI;
    public TMP_Text textBoxText;

    public bool closeOnExit = true;

    private bool isShowing = false;
    private bool hasBeenClosed = false;

    public bool IsShowing => isShowing;

    public System.Action<bool> onUIStateChanged;

    public void Interact()
    {
        if (isShowing)
        {
            CloseText();
        }
    }

    public void ShowText()
    {
        if (hasBeenClosed || textBoxUI == null) return;

        textBoxUI.SetActive(true);
        textBoxText.text = text;
        isShowing = true;
        onUIStateChanged?.Invoke(true);
    }

    public void CloseText()
    {
        if (textBoxUI == null) return;

        textBoxUI.SetActive(false);
        isShowing = false;
        hasBeenClosed = true;
        onUIStateChanged?.Invoke(false);
    }

    public void ResetClosed()
    {
        hasBeenClosed = false;
    }
}
