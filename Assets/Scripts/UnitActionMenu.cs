using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UnitActionMenu : MonoBehaviour
{
    public static UnitActionMenu Instance;

    public GameObject menuPanel;
    public Button moveButton;
    public Button attackButton;
    public Button endTurnButton;
    public Button closeButton;

    private Unit currentUnitForMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        menuPanel.SetActive(false);
    }

    public void OnMoveButtonPressed()
    {
        UnitManager.Instance.OnMovePressed();
        HideMenu();
    }
    public void OnAttackButtonPressed()
    {
        UnitManager.Instance.OnAttackPressed();
        HideMenu();
    }
    public void OnEndTurnButtonPressed()
    {
        UnitManager.Instance.OnEndTurnPressed();
        HideMenu();
    }

    public void ShowMenu(Vector3 position, Unit unit)
    {
        Debug.Log($"[DEBUG] ShowMenu вызван для: {(unit != null ? unit.name : "NULL")}");
        currentUnitForMenu = unit;
        menuPanel.SetActive(true);

        menuPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();

        bool isPlayer = (unit.faction == FactionManager.PlayerFaction);
        bool canMove = isPlayer && !unit.hasMoved && !unit.hasActed;
        bool canAttack = isPlayer && !unit.hasAttacked && !unit.hasActed;
        bool canEndTurn = isPlayer && !unit.hasActed;

        moveButton.gameObject.SetActive(canMove);
        attackButton.gameObject.SetActive(canAttack);
        endTurnButton.gameObject.SetActive(canEndTurn);
        UnitInfoPanel.Instance.ShowInfo(unit);



        // <<< ФРИЗИМПУТ >>>
        SetButtonsInteractable(false);
        StartCoroutine(EnableButtonsAfterDelay(0.2f));
    }

    private void SetButtonsInteractable(bool state)
    {
        moveButton.interactable = state;
        attackButton.interactable = state;
        endTurnButton.interactable = state;
        if (closeButton != null)
            closeButton.interactable = state;
    }

    private IEnumerator EnableButtonsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetButtonsInteractable(true);
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
        if (UnitInfoPanel.Instance != null)
        {
            UnitInfoPanel.Instance.HidePanel();
        }
    }
}


