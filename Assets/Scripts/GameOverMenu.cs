using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public GameObject gameOverPanel;

    //pause game and show game over menu
    public void Show()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RespawnFromCheckpoint()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        SaveController.Instance.RestartFromCheckpoint();
    }

    public void RespawnFromBeginning()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Destroy(player);

        SaveController.Instance.ResetSave();
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}

