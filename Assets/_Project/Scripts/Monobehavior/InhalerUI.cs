using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InhalerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image icon;

    [Header("Settings")]
    [SerializeField] private float activeAlpha = 1f;
    [SerializeField] private float inactiveAlpha = 0.5f;

    private Image _radial;

    private float _fillPercentage;
    
    public bool IsAvailable => _fillPercentage >= 1f;

    private void Awake()
    {
        _radial = GetComponent<Image>();
    }

    private void Update()
    {
        if (IsAvailable)
        {
            SetImageAlpha(_radial, activeAlpha);

            if (icon != null)
            {
                SetImageAlpha(icon, activeAlpha);
            }
        }
        else
        {
            SetImageAlpha(_radial, inactiveAlpha);

            if (icon != null)
            {
                SetImageAlpha(icon, inactiveAlpha);
            }
        }

        _radial.fillAmount = _fillPercentage;
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    public void SetFillPercentage(float percentage)
    {
        // Flip
        _fillPercentage = 1f - percentage;
    }
}
