using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;

    [SerializeField] private Text txtTitle;

    private UIMainManager m_mngr;

    public void SetResult(bool isWin)
    {
        string titleStr = isWin ? "YOU WIN!" : "GAME OVER";

        if (txtTitle != null)
        {
            txtTitle.text = titleStr;
            return;
        }

        Text[] texts = GetComponentsInChildren<Text>(true);
        foreach (var t in texts)
        {
            if (btnClose != null && t.transform.IsChildOf(btnClose.transform))
            {
                continue;
            }
            t.text = titleStr;
        }
    }

    private void Awake()
    {
        btnClose.onClick.AddListener(OnClickClose);
    }

    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
    }

    private void OnClickClose()
    {
        m_mngr.ShowMainMenu();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

}
