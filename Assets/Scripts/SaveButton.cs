using UnityEngine;

public class SaveButton : MonoBehaviour
{
    public void SaveGame()
    {
        SaveController.Instance.SaveGame();
    }
}
