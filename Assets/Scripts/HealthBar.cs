using UnityEngine;
using TMPro;

public class HealthBar : MonoBehaviour
{
    private Unit targetUnit;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro hpText;
    private Vector3 fullScale;

    public void Initialize(Unit unit, Sprite sprite, Color color, Vector3 scale)
    {
        targetUnit = unit;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingLayerID = unit.GetComponent<SpriteRenderer>().sortingLayerID;
        spriteRenderer.sortingOrder = unit.GetComponent<SpriteRenderer>().sortingOrder + 1;
        fullScale = scale;
        transform.localScale = fullScale;

        hpText = new GameObject("HPText").AddComponent<TextMeshPro>();
        hpText.transform.SetParent(transform, false);
        hpText.rectTransform.pivot = new Vector2(0f, 1f);
        hpText.fontSize = 3;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.transform.localPosition = new Vector3(-0.2f, -0.2f, 0f);

        UpdateBar();
    }

    public void UpdateBar()
    {
        if (targetUnit == null || spriteRenderer == null) return;
        float percent = Mathf.Clamp01((float)targetUnit.currentHP / targetUnit.MaxHP);
        transform.localScale = new Vector3(fullScale.x * percent, fullScale.y, fullScale.z);
        if (hpText != null)
            hpText.text = targetUnit.currentHP.ToString();
    }
}
