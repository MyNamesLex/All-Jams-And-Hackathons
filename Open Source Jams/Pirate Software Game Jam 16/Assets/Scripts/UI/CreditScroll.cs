using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

public class CreditScroll : MonoBehaviour
{
    public float Speed = 30f;
    public RectTransform CreditsRect;
    public List<RectTransform> UIRects = new();
    [Header("DVD Bouncing Settings")]
    public float BounceSpeed = 200f;
    private Vector2 _velocity;
    private void Start()
    {
        StartCoroutine(ScaleCredits());
        InitializeBounce();
    }

    private void InitializeBounce()
    {
        float angle = Random.Range(30f, 60f);
        _velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                              Mathf.Sin(angle * Mathf.Deg2Rad)) * BounceSpeed;
    }

    private void Update()
    {
        if (CreditsRect == null) return;

        Vector2 newPos = CreditsRect.anchoredPosition + _velocity * Time.deltaTime;
        CreditsRect.anchoredPosition = newPos;

        // Check for collisions
        RectTransform parentRect = CreditsRect.parent as RectTransform;
        if (parentRect == null) return;

        // Calculate boundaries
        Bounds parentBounds = GetParentBounds(parentRect);
        Bounds creditBounds = GetRectBounds(CreditsRect);

        // Horizontal collision
        if (newPos.x + creditBounds.extents.x > parentBounds.extents.x ||
            newPos.x - creditBounds.extents.x < -parentBounds.extents.x)
        {
            _velocity.x *= -1;
            newPos.x = Mathf.Clamp(newPos.x,
                                 -parentBounds.extents.x + creditBounds.extents.x,
                                 parentBounds.extents.x - creditBounds.extents.x);
        }

        // Vertical collision
        if (newPos.y + creditBounds.extents.y > parentBounds.extents.y ||
            newPos.y - creditBounds.extents.y < -parentBounds.extents.y)
        {
            _velocity.y *= -1;
            newPos.y = Mathf.Clamp(newPos.y,
                                 -parentBounds.extents.y + creditBounds.extents.y,
                                 parentBounds.extents.y - creditBounds.extents.y);
        }

        CreditsRect.anchoredPosition = newPos;
    }

    Bounds GetParentBounds(RectTransform parent)
    {
        return new Bounds(Vector3.zero, parent.rect.size);
    }

    Bounds GetRectBounds(RectTransform rect)
    {
        return new Bounds(rect.rect.center, rect.rect.size);
    }


    private IEnumerator ScaleCredits()
    {
        foreach (RectTransform rect in UIRects)
        {
            rect.localScale = Vector3.zero;
        }

        foreach (RectTransform rect in UIRects)
        {
            rect.DOScale(1, 0.25f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(3f);
        }
    }
}