using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CombatDisplay : MonoBehaviour
{
    public static CombatDisplay Instance;

    public GameObject panel;
    public Transform leftGroup;
    public Transform rightGroup;

    public GameObject commanderSpritePrefab;
    public GameObject soldierSpritePrefab;

    public float approachDistance = 200f;
    public float moveDuration = 0.5f;
    public float pauseBeforeRetreat = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (panel != null)
            panel.SetActive(false);
    }

    public IEnumerator PlayBattle(Unit attacker, Unit defender, int dmgToDefender, int dmgToAttacker)
    {
        if (panel == null) yield break;

        panel.SetActive(true);
        ClearChildren(leftGroup);
        ClearChildren(rightGroup);

        PopulateGroup(leftGroup, attacker);
        PopulateGroup(rightGroup, defender);

        Vector3 leftStart = leftGroup.localPosition;
        Vector3 rightStart = rightGroup.localPosition;
        Vector3 leftTarget = leftStart + Vector3.right * approachDistance;
        Vector3 rightTarget = rightStart + Vector3.left * approachDistance;

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / moveDuration);
            leftGroup.localPosition = Vector3.Lerp(leftStart, leftTarget, pct);
            rightGroup.localPosition = Vector3.Lerp(rightStart, rightTarget, pct);
            yield return null;
        }

        ApplyDamage(rightGroup, defender, dmgToDefender);
        if (dmgToAttacker > 0)
            ApplyDamage(leftGroup, attacker, dmgToAttacker);

        yield return new WaitForSeconds(pauseBeforeRetreat);

        t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / moveDuration);
            leftGroup.localPosition = Vector3.Lerp(leftTarget, leftStart, pct);
            rightGroup.localPosition = Vector3.Lerp(rightTarget, rightStart, pct);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        panel.SetActive(false);
    }

    void ClearChildren(Transform t)
    {
        if (t == null) return;
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }

    void PopulateGroup(Transform parent, Unit unit)
    {
        if (parent == null || unit == null) return;

        if (unit.isCommander)
        {
            CreateSprite(commanderSpritePrefab, parent, unit.GetComponent<SpriteRenderer>().sprite);
        }
        else
        {
            int count = Mathf.Clamp(unit.currentHP, 0, 10);
            for (int i = 0; i < count; i++)
                CreateSprite(soldierSpritePrefab, parent, unit.GetComponent<SpriteRenderer>().sprite);
        }
    }

    void CreateSprite(GameObject prefab, Transform parent, Sprite sprite)
    {
        GameObject go;
        if (prefab != null)
            go = Instantiate(prefab, parent);
        else
            go = new GameObject("Sprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
    }

    void ApplyDamage(Transform group, Unit unit, int dmg)
    {
        int newHP = Mathf.Max(0, unit.currentHP - dmg);
        int before = Mathf.Clamp(unit.currentHP, 0, 10);
        int after = Mathf.Clamp(newHP, 0, 10);
        int toHide = before - after;
        for (int i = 0; i < toHide && group.childCount > 0; i++)
        {
            Transform child = group.GetChild(group.childCount - 1 - i);
            child.gameObject.SetActive(false);
        }
    }
}
