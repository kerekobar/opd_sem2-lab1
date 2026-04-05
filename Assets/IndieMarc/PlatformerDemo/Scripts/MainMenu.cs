using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "GameLevel"; 

    public void StartGame()
    {

        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ¬ собранной игре закрываем приложение
            Application.Quit();
#endif
    }
}