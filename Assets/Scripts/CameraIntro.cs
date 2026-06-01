using UnityEngine;
using DG.Tweening;

public class CameraIntro : MonoBehaviour
{
    public float zoomOffset = 5f;
    public float duration = 0.8f;
    public Ease ease = Ease.OutCubic;

    void Start()
    {
        Vector3 targetPos = transform.position;

        transform.position = targetPos - transform.forward * zoomOffset;

        transform.DOMove(targetPos, duration).SetEase(ease);
    }
}