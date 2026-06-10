using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitGameController : MonoBehaviour
{
    public void BacktoMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Exit()
    {
        Application.Quit();

        // Untuk testing di Unity Editor
        Debug.Log("Game Closed");
    }
}