using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMoves : LevelCondition
{
    private int m_moves;

    private BoardController m_board;

    public override void Setup(float value, Text txt, BoardController board)
    {
        base.Setup(value, txt);

        m_moves = (int)value;

        m_board = board;

        m_board.OnMoveEvent += OnMove;

        if (m_txt != null)
        {
            m_txt.gameObject.SetActive(false);
        }
    }

    private void OnMove()
    {
        if (m_conditionCompleted) return;

        m_moves--;
    }

    protected override void UpdateText()
    {
        if (m_txt != null)
        {
            m_txt.gameObject.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        if (m_board != null) m_board.OnMoveEvent -= OnMove;

        base.OnDestroy();
    }
}
