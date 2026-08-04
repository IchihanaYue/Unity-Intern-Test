using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private Button btnPlay;

    [SerializeField] private Button btnAutoWin;

    [SerializeField] private Button btnAutoLose;

    private UIMainManager m_mngr;

    private Text EnsureButtonText(Button btn, string labelText)
    {
        if (btn == null) return null;

        Text txt = btn.GetComponentInChildren<Text>();
        if (txt == null)
        {
            GameObject txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btn.transform, false);
            txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (txt.font == null) txt.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
        }

        txt.text = labelText;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        txt.fontSize = 22;
        txt.resizeTextForBestFit = true;
        txt.resizeTextMinSize = 10;
        txt.resizeTextMaxSize = 28;

        RectTransform rect = txt.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        return txt;
    }

    private void Awake()
    {
        if (btnPlay == null) btnPlay = btnMoves;

        if (btnPlay != null)
        {
            btnPlay.onClick.RemoveAllListeners();
            btnPlay.onClick.AddListener(OnClickPlay);
            btnPlay.gameObject.SetActive(true);

            RectTransform r1 = btnPlay.GetComponent<RectTransform>();
            if (r1 != null)
            {
                r1.anchoredPosition = new Vector2(0f, 120f);
                r1.sizeDelta = new Vector2(250f, 55f);
            }

            EnsureButtonText(btnPlay, "PLAY");
        }

        Button btnTimeAttack = btnTimer;
        if (btnTimeAttack == null && btnPlay != null)
        {
            GameObject go = Instantiate(btnPlay.gameObject, btnPlay.transform.parent);
            go.name = "btnTimeAttack";
            btnTimeAttack = go.GetComponent<Button>();
        }

        if (btnTimeAttack != null)
        {
            btnTimeAttack.onClick.RemoveAllListeners();
            btnTimeAttack.onClick.AddListener(OnClickTimeAttack);
            btnTimeAttack.gameObject.SetActive(true);

            RectTransform r2 = btnTimeAttack.GetComponent<RectTransform>();
            if (r2 != null)
            {
                r2.anchoredPosition = new Vector2(0f, 40f);
                r2.sizeDelta = new Vector2(250f, 55f);
            }

            EnsureButtonText(btnTimeAttack, "TIME ATTACK");
        }

        if (btnAutoWin == null && btnPlay != null)
        {
            GameObject go2 = Instantiate(btnPlay.gameObject, btnPlay.transform.parent);
            go2.name = "btnAutoWin";
            btnAutoWin = go2.GetComponent<Button>();
        }

        if (btnAutoWin != null)
        {
            btnAutoWin.onClick.RemoveAllListeners();
            btnAutoWin.onClick.AddListener(OnClickAutoWin);
            btnAutoWin.gameObject.SetActive(true);

            RectTransform r3 = btnAutoWin.GetComponent<RectTransform>();
            if (r3 != null)
            {
                r3.anchoredPosition = new Vector2(0f, -40f);
                r3.sizeDelta = new Vector2(250f, 55f);
            }

            EnsureButtonText(btnAutoWin, "AUTOPLAY (WIN)");
        }

        if (btnAutoLose == null && btnPlay != null)
        {
            GameObject go3 = Instantiate(btnPlay.gameObject, btnPlay.transform.parent);
            go3.name = "btnAutoLose";
            btnAutoLose = go3.GetComponent<Button>();
        }

        if (btnAutoLose != null)
        {
            btnAutoLose.onClick.RemoveAllListeners();
            btnAutoLose.onClick.AddListener(OnClickAutoLose);
            btnAutoLose.gameObject.SetActive(true);

            RectTransform r4 = btnAutoLose.GetComponent<RectTransform>();
            if (r4 != null)
            {
                r4.anchoredPosition = new Vector2(0f, -120f);
                r4.sizeDelta = new Vector2(250f, 55f);
            }

            EnsureButtonText(btnAutoLose, "AUTO LOSE");
        }
    }

    private void OnDestroy()
    {
        if (btnPlay) btnPlay.onClick.RemoveAllListeners();
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
        if (btnAutoWin) btnAutoWin.onClick.RemoveAllListeners();
        if (btnAutoLose) btnAutoLose.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickPlay()
    {
        m_mngr.LoadLevelMoves();
    }

    private void OnClickTimeAttack()
    {
        m_mngr.LoadLevelTimeAttack();
    }

    private void OnClickAutoWin()
    {
        m_mngr.LoadLevelAutoWin();
    }

    private void OnClickAutoLose()
    {
        m_mngr.LoadLevelAutoLose();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
