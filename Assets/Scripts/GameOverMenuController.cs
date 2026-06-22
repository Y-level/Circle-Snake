using UnityEngine;
using UnityEngine.SceneManagement; // Обязательно для работы со сценами

public class GameOverMenuController : MonoBehaviour
{
    // Для кнопки "Играть снова"
    public void RestartGame()
    {
        // ВАЖНО: возвращаем время в норму, иначе игра будет на паузе
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Для кнопки "В меню"
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        // Замени "MainMenu" на точное название твоей сцены с меню
        SceneManager.LoadScene("MainMenu");
    }
}
