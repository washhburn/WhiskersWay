using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => ActivateTab(index));
        }
        ActivateTab(0); //start with the first tab active
    }

    public void ActivateTab(int tabNum)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;
        }
        pages[tabNum].SetActive(true);
        tabImages[tabNum].color = Color.white;

    }
}
