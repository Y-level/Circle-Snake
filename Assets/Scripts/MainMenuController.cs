using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        // Загружаем сцену с игрой по её названию
        SceneManager.LoadScene("GameScene"); // Замени на имя своей игровой сцены
    }

    public void QuitGame()
    {
        // Закрыть приложение (работает только на собранном APK)
        UnityEngine.Application.Quit();
    }
}
