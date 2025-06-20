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
    public float spacingX = 32f;
    public float spacingY = 32f;
    public float fadeDuration = 0.25f;

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

        // Flip the defender group horizontally before animation starts
        if (rightGroup != null)
            rightGroup.localScale = new Vector3(-1, 1, 1);

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
            var obj = CreateSprite(commanderSpritePrefab, parent, unit.GetComponent<SpriteRenderer>().sprite);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            int count = Mathf.Clamp(Mathf.CeilToInt((float)unit.currentHP / unit.MaxHP * 10), 0, 10);
            for (int i = 0; i < count; i++)
            {
                var obj = CreateSprite(soldierSpritePrefab, parent, unit.GetComponent<SpriteRenderer>().sprite);
                var rt = obj.GetComponent<RectTransform>();
                int row = i / 5;
                int col = i % 5;
                rt.anchoredPosition = new Vector2(col * spacingX, -row * spacingY);
            }
        }
    }

    GameObject CreateSprite(GameObject prefab, Transform parent, Sprite sprite)
    {
        GameObject go;
        if (prefab != null)
        {
            go = Instantiate(prefab, parent);
        }
        else
        {
            go = new GameObject("Sprite", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
        }
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.SetNativeSize();
        StartCoroutine(FadeInRoutine(go, fadeDuration));
        return go;
    }

    void ApplyDamage(Transform group, Unit unit, int dmg)
    {
        int newHP = Mathf.Max(0, unit.currentHP - dmg);
        int before = Mathf.Clamp(Mathf.CeilToInt((float)unit.currentHP / unit.MaxHP * 10), 0, 10);
        int after = Mathf.Clamp(Mathf.CeilToInt((float)newHP / unit.MaxHP * 10), 0, 10);
        int toHide = before - after;
        for (int i = 0; i < toHide && group.childCount > 0; i++)
        {
            Transform child = group.GetChild(group.childCount - 1 - i);
            StartCoroutine(FadeOutRoutine(child.gameObject, fadeDuration));
        }
    }

    IEnumerator FadeInRoutine(GameObject obj, float duration)
    {
        if (obj == null) yield break;
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = obj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        Vector3 startScale = Vector3.one * 0.8f;
        obj.transform.localScale = startScale;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / duration);
            cg.alpha = pct;
            obj.transform.localScale = Vector3.Lerp(startScale, Vector3.one, pct);
            yield return null;
        }
        cg.alpha = 1f;
        obj.transform.localScale = Vector3.one;
    }

    IEnumerator FadeOutRoutine(GameObject obj, float duration)
    {
        if (obj == null) yield break;
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = obj.AddComponent<CanvasGroup>();

        cg.alpha = 1f;
        Vector3 startScale = obj.transform.localScale;
        Vector3 endScale = startScale * 0.8f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / duration);
            cg.alpha = 1f - pct;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, pct);
            yield return null;
        }
        cg.alpha = 0f;
        obj.SetActive(false);
    }
}
