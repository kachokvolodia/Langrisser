using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Unit targetUnit;
    private SpriteRenderer spriteRenderer;
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
        UpdateBar();
    }

    public void UpdateBar()
    {
        if (targetUnit == null || spriteRenderer == null) return;
        float percent = Mathf.Clamp01((float)targetUnit.currentHP / targetUnit.unitData.maxHP);
        transform.localScale = new Vector3(fullScale.x * percent, fullScale.y, fullScale.z);
    }
}