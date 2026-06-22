using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(EdgeCollider2D), typeof(RectTransform))]
public class SquareBorders : MonoBehaviour
{
    private EdgeCollider2D edgeCollider;
    private RectTransform rectTransform;

    void Awake()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Берем 4 угла UI-квадрата
        Vector3[] corners = new Vector3[4];
        rectTransform.GetLocalCorners(corners);

        // Замыкаем их в линию для коллайдера
        Vector2[] points = new Vector2[5];
        points[0] = corners[0]; // Лево-низ
        points[1] = corners[1]; // Лево-верх
        points[2] = corners[2]; // Право-верх
        points[3] = corners[3]; // Право-низ
        points[4] = corners[0]; // Снова лево-низ (замыкаем)

        edgeCollider.points = points;
    }
}
