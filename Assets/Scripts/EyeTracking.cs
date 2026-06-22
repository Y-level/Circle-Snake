using UnityEngine;

public class EyeTracking : MonoBehaviour
{
    [Header("Ссылки на зрачки")]
    public Transform leftPupil;
    public Transform rightPupil;

    [Header("Настройки")]
    public float followSpeed = 20f;
    public string foodTag = "Food";

    // Твой отступ для Scale зрачка 0.6
    private float maxOffset = 0.3f;

    void Update()
    {
        Transform target = FindClosestFood();

        if (target != null)
        {
            LookAtWorldTarget(leftPupil, target.position);
            LookAtWorldTarget(rightPupil, target.position);
        }
    }

    void LookAtWorldTarget(Transform pupil, Vector3 worldTargetPos)
    {
        // 1. Берем мировые позиции
        Vector3 eyePos = pupil.parent.position;
        Vector3 targetPos = worldTargetPos;

        // 2. ВАЖНО: Приравниваем их Z, чтобы глаза не смотрели "вглубь" экрана
        targetPos.z = eyePos.z;

        // 3. Вычисляем направление
        Vector3 directionToTarget = targetPos - eyePos;

        // Если еда слишком близко к центру глаза, не дергаемся
        if (directionToTarget.magnitude < 0.01f) return;

        // 4. Переводим направление в локальное пространство глаза
        // Это нужно, чтобы зрачки не вращались вместе с головой
        Vector3 localDir = pupil.parent.InverseTransformDirection(directionToTarget);

        // 5. Ограничиваем движение и плавно перемещаем
        Vector3 targetLocalPos = localDir.normalized * maxOffset;
        targetLocalPos.z = 0; // На всякий случай для локальной позиции

        pupil.localPosition = Vector3.Lerp(pupil.localPosition, targetLocalPos, Time.deltaTime * followSpeed);
    }



    Transform FindClosestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag(foodTag);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject food in foods)
        {
            float dist = Vector3.Distance(transform.position, food.transform.position);
            if (dist < minDist)
            {
                closest = food.transform;
                minDist = dist;
            }
        }
        return closest;
    }
}
