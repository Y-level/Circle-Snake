using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnakeMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    public float rotationSpeed = 200f;
    public float rotationRadius = 0.8f;

    [Header("Настройки хвоста")]
    public GameObject bodyPrefab;
    public float segmentDistance = 0.45f; // Расстояние между сегментами

    private List<GameObject> bodyParts = new List<GameObject>();
    private List<Vector3> positionsHistory = new List<Vector3>();
    private int direction = 1;
    private Vector3 pivotPoint;
    private TrailRenderer trail; // Тот самый пульт управления шлейфом

    // Переменные для алгоритма задержки спавна
    private int pendingSegments = 0;
    private float distanceWalkedSinceEating = 0f;
    private Vector3 lastPosition;
    
    [Header("UI")]
    public TMP_Text scoreText;
    private int score = 0;

    [Header("Пауза")]
    public GameObject pauseMenu; // Сюда перетащи панель в инспекторе

    [Header("Меню поражения")]
    public GameObject gameOverPanel;

    public void TogglePause()
    {
        if (Time.timeScale > 0f) // Если игра идет
        {
            Time.timeScale = 0f;       // Остановить время
            pauseMenu.SetActive(true); // Показать меню
        }
        else // Если на паузе
        {
            Time.timeScale = 1f;       // Запустить время
            pauseMenu.SetActive(false); // Скрыть меню
        }
    }


    void AdjustCamera()
    {
        Camera cam = Camera.main;

        // 1. Укажи здесь РЕАЛЬНУЮ ширину твоего поля (расстояние от левой до правой стенки)
        // Например, если левая стена на -2.5, а правая на 2.5, то ширина = 5.0
        float fieldWidth = 5.0f;

        // 2. Добавь множитель отступа (1.1 = +10% пустого места по бокам)
        float margin = 1.1f;

        float targetWidth = fieldWidth * margin;

        // 3. Магия: подгоняем высоту камеры под ширину экрана
        float screenAspect = (float)Screen.width / Screen.height;

        // Формула: Размер камеры = (Желаемая Ширина / Соотношение сторон) / 2
        cam.orthographicSize = (targetWidth / screenAspect) / 2f;
    }

    void Start()
    {
        // Инициализируем компоненты
        trail = GetComponent<TrailRenderer>();
        lastPosition = transform.position;

        // Важно: Заполняем историю, чтобы первый сегмент не "улетал"
        for (int i = 0; i < 100; i++)
        {
            positionsHistory.Add(transform.position);
        }

        UpdatePivot();

        AdjustCamera();
    }

    void Update()
    {

        if (Time.timeScale == 0f) return;


        // 1. Вращение головы
        transform.RotateAround(pivotPoint, Vector3.forward, direction * rotationSpeed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                direction *= -1;
                UpdatePivot();
            }
        }

        // 2. Считаем пройденный путь в этом кадре
        float frameDistance = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        // 3. АЛГОРИТМ: Проверка очереди на спавн
        if (pendingSegments > 0)
        {
            distanceWalkedSinceEating += frameDistance;

            // Только когда проползли нужное расстояние, добавляем физический шар
            if (distanceWalkedSinceEating >= segmentDistance)
            {
                addBodyPart();
                pendingSegments--;
                distanceWalkedSinceEating = 0f;
            }
        }

        // 4. Запись истории позиций (каждый кадр для точности хвоста)
        positionsHistory.Insert(0, transform.position);

        if (positionsHistory.Count > 3000)
            positionsHistory.RemoveAt(positionsHistory.Count - 1);

        // 5. Двигаем существующие сегменты
        MoveTail();
    }

    void MoveTail()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            // Каждый сегмент ищет свою точку в истории
            bodyParts[i].transform.position = GetPointAtDistance((i + 1) * segmentDistance);
        }
    }

    // Метод для точного поиска точки в истории
    Vector3 GetPointAtDistance(float targetDist)
    {
        float currentDist = 0f;
        for (int i = 0; i < positionsHistory.Count - 1; i++)
        {
            float d = Vector3.Distance(positionsHistory[i], positionsHistory[i + 1]);
            if (currentDist + d >= targetDist)
            {
                float t = (targetDist - currentDist) / d;
                return Vector3.Lerp(positionsHistory[i], positionsHistory[i + 1], t);
            }
            currentDist += d;
        }
        return positionsHistory[positionsHistory.Count - 1];
    }

    void UpdatePivot() => pivotPoint = transform.position + transform.right * direction * rotationRadius;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            score++;
            scoreText.text = score.ToString();

            // 1. Увеличиваем время жизни шлейфа (визуал растет сразу)
            Grow();

            // 2. Ставим задачу заспавнить физическое тело позже
            pendingSegments++;

            Destroy(other.gameObject);

            // Спавним новую еду
            FoodSpawner spawner = FindFirstObjectByType<FoodSpawner>();
            if (spawner != null) spawner.SpawnFood();
        }

        if (other.CompareTag("Body"))
        {
            RestartGame();
        }

        if (other.GetComponent<SquareBorders>() != null || other.CompareTag("Border"))
        {
            RestartGame();
        }
    }

    void Grow()
    {
        if (trail != null)
        {
            float angularSpeedRad = rotationSpeed * Mathf.Deg2Rad;
            float linearSpeed = rotationRadius * angularSpeedRad;

            // Рассчитываем время жизни шлейфа
            float totalTailLength = (bodyParts.Count + pendingSegments + 1) * segmentDistance;
            trail.time = (totalTailLength / linearSpeed) + 0.05f; // + небольшой запас
        }
    }

    void addBodyPart()
    {
        // Находим точку, где визуально закончился шлейф
        Vector3 spawnPos = GetPointAtDistance((bodyParts.Count + 1) * segmentDistance);
        GameObject body = Instantiate(bodyPrefab, spawnPos, Quaternion.identity);
        body.tag = "Body";
        bodyParts.Add(body);
    }

    void RestartGame()
    {
        Time.timeScale = 0f; // Останавливаем время
        gameOverPanel.SetActive(true);
    }
}
