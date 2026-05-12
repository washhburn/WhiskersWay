using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image background; //empty beige slots
    public Image icon; //item-sprites
    public GameObject currentItem;

    public void SetItem(GameObject itemObject)
    {
        currentItem = itemObject;
        Item item = itemObject.GetComponent<Item>();
        icon.sprite = item.icon;
        icon.color = new Color(1, 1, 1, 1);
        icon.preserveAspect = true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0);
    }
}
