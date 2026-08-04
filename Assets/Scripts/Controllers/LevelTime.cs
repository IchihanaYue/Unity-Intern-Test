using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTime : LevelCondition
{
    private float m_time;

    private GameManager m_mngr;

    public override void Setup(float value, Text txt, GameManager mngr)
    {
        base.Setup(value, txt, mngr);

        m_mngr = mngr;

        m_time = value;

        if (m_txt != null)
        {
            m_txt.gameObject.SetActive(true);
        }

        UpdateText();
    }

    private void Update()
    {
        if (m_conditionCompleted) return;

        if (m_mngr.State != GameManager.eStateGame.GAME_STARTED) return;

        m_time -= Time.deltaTime;

        UpdateText();

        if (m_time <= 0f)
        {
            OnConditionComplete();
        }
    }

    protected override void UpdateText()
    {
        if (m_txt == null) return;
        m_txt.gameObject.SetActive(true);

        if (m_time < 0f)
        {
            m_txt.text = "TIME:\n00s";
            return;
        }

        m_txt.text = string.Format("TIME:\n{0:00}s", Mathf.CeilToInt(m_time));
    }
}
