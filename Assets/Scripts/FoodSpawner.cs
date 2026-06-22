//using UnityEngine;

//public class FoodSpawner : MonoBehaviour
//{
//    public GameObject foodPrefab;

//    [Header("Динамические диапазоны")]
//    public float xRange;
//    public float yRange;

//    [Header("Настройки")]
//    public float cornerSafetyBuffer = 1.3f;
//    [Tooltip("Отступ от края экрана в процентах (0.1 = 10%)")]
//    public float screenMargin = 0.1f;

//    void Start()
//    {
//        // 1. Получаем координаты края экрана
//        Vector2 topRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

//        // 2. Делаем квадрат на основе ШИРИНЫ (topRight.x)
//        // Вычитаем отступ, чтобы еда не лезла на самые края
//        float squareHalfSize = topRight.x - (topRight.x * screenMargin);

//        xRange = squareHalfSize;
//        yRange = squareHalfSize;

//        // 3. Спавним еду
//        SpawnFood();
//    }

//    public void SpawnFood()
//    {
//        Vector3 randomPos;
//        bool isInsideCorner;

//        do
//        {
//            float randomX = UnityEngine.Random.Range(-xRange, xRange);
//            float randomY = UnityEngine.Random.Range(-yRange, yRange);
//            randomPos = new Vector3(randomX, randomY, 0);

//            bool nearXEdge = Mathf.Abs(randomPos.x) > (xRange - cornerSafetyBuffer);
//            bool nearYEdge = Mathf.Abs(randomPos.y) > (yRange - cornerSafetyBuffer);

//            isInsideCorner = nearXEdge && nearYEdge;

//        } while (isInsideCorner);

//        Instantiate(foodPrefab, randomPos, Quaternion.identity);
//    }
//}
using System;
using System.Diagnostics;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

    [Header("Ссылка на рамку")]
    public RectTransform gameFieldRect; // Перетащи сюда объект GameField

    [Header("Настройки")]
    public float cornerSafetyBuffer = 1.3f;
    [Range(0, 0.5f)]
    public float edgePadding = 0.2f; // Отступ от самих палок рамки, чтобы еда не была полу-в стене

    public void SpawnFood()
    {
        Vector3 randomPos;
        bool isInsideCorner;

        // 1. Получаем РЕАЛЬНЫЙ размер квадрата в мировых координатах
        // rect.width берется с учетом всех масштабирований под телефон
        float halfSize = (gameFieldRect.rect.width * gameFieldRect.lossyScale.x) / 2f;

        // Учитываем небольшой отступ от самой палки границы
        float effectiveRange = halfSize - (halfSize * edgePadding);

        int safetyBreak = 0; // На случай если буфер слишком большой
        do
        {
            float randomX = UnityEngine.Random.Range(-effectiveRange, effectiveRange);
            float randomY = UnityEngine.Random.Range(-effectiveRange, effectiveRange);
            randomPos = gameFieldRect.position + new Vector3(randomX, randomY, 0);

            // 2. Проверка углов (где змейка не может развернуться)
            bool nearXEdge = Mathf.Abs(randomX) > (effectiveRange - cornerSafetyBuffer);
            bool nearYEdge = Mathf.Abs(randomY) > (effectiveRange - cornerSafetyBuffer);

            isInsideCorner = nearXEdge && nearYEdge;

            safetyBreak++;
        } while (isInsideCorner && safetyBreak < 100);

        Instantiate(foodPrefab, randomPos, Quaternion.identity);
    }

    void Start()
    {
        SpawnFood();
    }
}
