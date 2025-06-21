using UnityEngine;
using TMPro;

public class HealthBar : MonoBehaviour
{
    private Unit targetUnit;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro hpText;
    private float fontSize;
    private Vector3 textOffset;
    private Vector3 fullScale;

    public void Initialize(Unit unit, Sprite sprite, Color color, Vector3 scale, float hpFontSize, Vector3 hpOffset)
    {
        targetUnit = unit;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingLayerID = unit.GetComponent<SpriteRenderer>().sortingLayerID;
        spriteRenderer.sortingOrder = unit.GetComponent<SpriteRenderer>().sortingOrder + 1;
        fullScale = scale;
        transform.localScale = fullScale;


        fontSize = hpFontSize;
        textOffset = hpOffset;

        hpText = new GameObject("HPText").AddComponent<TextMeshPro>();
        hpText.transform.SetParent(transform, false);
        hpText.rectTransform.pivot = new Vector2(0f, 1f);
        hpText.fontSize = fontSize;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.transform.localPosition = textOffset;
        hpText.color = Color.black;
        var mr = hpText.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerID = spriteRenderer.sortingLayerID;
            mr.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

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
