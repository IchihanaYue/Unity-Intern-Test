using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                StopHints();
                break;
        }
    }


    private bool m_isTimeAttack;

    public void SetTimeAttackMode(bool isTimeAttack)
    {
        m_isTimeAttack = isTimeAttack;
    }

    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hits = Physics2D.RaycastAll(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            Cell selectedBoardCell = null;
            Cell selectedHolderCell = null;
            int maxZ = -1;

            foreach (var hit in hits)
            {
                if (hit.collider != null)
                {
                    Cell cell = hit.collider.GetComponent<Cell>();
                    if (cell != null)
                    {
                        if (cell.BoardY >= 0 && !cell.IsEmpty)
                        {
                            if (!m_board.IsCellBlocked(cell) && cell.LayerZ > maxZ)
                            {
                                selectedBoardCell = cell;
                                maxZ = cell.LayerZ;
                            }
                        }
                        else if (cell.BoardY < 0 && !cell.IsEmpty && m_isTimeAttack)
                        {
                            selectedHolderCell = cell;
                        }
                    }
                }
            }

            if (selectedHolderCell != null && selectedHolderCell.Item != null)
            {
                Item item = selectedHolderCell.Item;
                if (item.OriginalCell != null)
                {
                    StartCoroutine(ReturnItemToBoardCoroutine(selectedHolderCell, item.OriginalCell));
                }
            }
            else if (selectedBoardCell != null)
            {
                Cell emptyHolderCell = m_board.GetFirstEmptyHolderCell();
                if (emptyHolderCell != null)
                {
                    StartCoroutine(OnItemClickedCoroutine(selectedBoardCell, emptyHolderCell));
                }
            }
        }
    }

    private IEnumerator ReturnItemToBoardCoroutine(Cell holderCell, Cell targetBoardCell)
    {
        IsBusy = true;
        StopHints();

        Item item = holderCell.Item;
        holderCell.Free();
        targetBoardCell.Assign(item);

        item.SetSortingOrder(targetBoardCell.LayerZ * 10 + 1);
        item.View.DOMove(targetBoardCell.transform.position, 0.25f);
        yield return new WaitForSeconds(0.25f);

        m_board.ShiftHolderItemsLeft();
        m_board.UpdateBlockedVisuals();

        IsBusy = false;
        m_timeAfterFill = 0f;
    }

    private IEnumerator OnItemClickedCoroutine(Cell mainCell, Cell targetHolderCell)
    {
        IsBusy = true;
        StopHints();

        Item item = mainCell.Item;
        if (item.OriginalCell == null)
        {
            item.OriginalCell = mainCell;
        }
        mainCell.Free();
        targetHolderCell.Assign(item);

        OnMoveEvent();

        item.View.DOMove(targetHolderCell.transform.position, 0.25f);
        yield return new WaitForSeconds(0.25f);

        m_board.UpdateBlockedVisuals();

        bool hadHolderMatch = m_board.CheckAndExplodeHolderMatches(null);
        while (hadHolderMatch)
        {
            yield return new WaitForSeconds(0.3f);
            hadHolderMatch = m_board.CheckAndExplodeHolderMatches(null);
        }

        if (m_board.IsMainBoardCleared())
        {
            if (m_gameManager != null)
            {
                m_gameManager.GameWin();
            }
        }
        else if (m_board.IsHolderFull() && !m_isTimeAttack)
        {
            if (m_gameManager != null)
            {
                m_gameManager.GameOver();
            }
        }

        IsBusy = false;
        m_timeAfterFill = 0f;
    }

    private void ResetRayCast()
    {
        m_isDragging = false;
        m_hitCollider = null;
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    private void ShowHint()
    {
        if (m_potentialMatch == null) return;
        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            if (cell != null) cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        m_hintIsShown = false;
        if (m_potentialMatch != null)
        {
            foreach (var cell in m_potentialMatch)
            {
                if (cell != null) cell.StopHintAnimation();
            }
            m_potentialMatch.Clear();
        }
    }

    public enum eAutoplayMode
    {
        NONE,
        AUTO_WIN,
        AUTO_LOSE
    }

    private eAutoplayMode m_autoplayMode = eAutoplayMode.NONE;
    private Coroutine m_autoplayCoroutine;

    public void SetAutoplayMode(eAutoplayMode mode)
    {
        m_autoplayMode = mode;
        if (m_autoplayCoroutine != null)
        {
            StopCoroutine(m_autoplayCoroutine);
        }
        if (m_autoplayMode != eAutoplayMode.NONE)
        {
            m_autoplayCoroutine = StartCoroutine(AutoplayCoroutine());
        }
    }

    private IEnumerator AutoplayCoroutine()
    {
        while (!m_gameOver && m_board != null && !m_board.IsMainBoardCleared())
        {
            yield return new WaitForSeconds(0.5f);

            while (IsBusy)
            {
                yield return null;
            }

            if (m_gameOver || m_board == null || m_board.IsMainBoardCleared() || m_board.IsHolderFull())
            {
                break;
            }

            Cell targetHolderCell = m_board.GetFirstEmptyHolderCell();
            if (targetHolderCell == null) break;

            List<Cell> unblockedCells = m_board.GetUnblockedCells();
            if (unblockedCells == null || unblockedCells.Count == 0) break;

            Cell chosenCell = SelectAutoplayMove(unblockedCells);
            if (chosenCell != null)
            {
                yield return StartCoroutine(OnItemClickedCoroutine(chosenCell, targetHolderCell));
            }
        }
    }

    private Cell SelectAutoplayMove(List<Cell> unblockedCells)
    {
        List<Item> holderItems = new List<Item>();
        for (int i = 0; i < m_board.BottomRowSize; i++)
        {
            Cell c = m_board.GetBottomCell(i);
            if (c != null && !c.IsEmpty && c.Item != null)
            {
                holderItems.Add(c.Item);
            }
        }

        Dictionary<NormalItem.eNormalType, int> holderCounts = new Dictionary<NormalItem.eNormalType, int>();
        foreach (var item in holderItems)
        {
            NormalItem nItem = item as NormalItem;
            if (nItem != null)
            {
                if (!holderCounts.ContainsKey(nItem.ItemType)) holderCounts[nItem.ItemType] = 0;
                holderCounts[nItem.ItemType]++;
            }
        }

        int currentHolderCount = holderItems.Count;

        if (m_autoplayMode == eAutoplayMode.AUTO_WIN)
        {
            Cell bestCell = null;
            int maxScore = int.MinValue;

            Dictionary<NormalItem.eNormalType, int> unblockedFreq = new Dictionary<NormalItem.eNormalType, int>();
            foreach (var cell in unblockedCells)
            {
                NormalItem nItem = cell.Item as NormalItem;
                if (nItem != null)
                {
                    if (!unblockedFreq.ContainsKey(nItem.ItemType)) unblockedFreq[nItem.ItemType] = 0;
                    unblockedFreq[nItem.ItemType]++;
                }
            }

            foreach (var cell in unblockedCells)
            {
                NormalItem nItem = cell.Item as NormalItem;
                if (nItem == null) continue;

                int score = 0;
                int countInHolder = holderCounts.ContainsKey(nItem.ItemType) ? holderCounts[nItem.ItemType] : 0;

                if (countInHolder == 2)
                {
                    score += 10000;
                }
                else if (countInHolder == 1)
                {
                    score += 1000;
                }
                else if (currentHolderCount >= 3)
                {
                    score -= 5000;
                }

                score += cell.LayerZ * 20;

                if (unblockedFreq.ContainsKey(nItem.ItemType))
                {
                    score += unblockedFreq[nItem.ItemType] * 5;
                }

                if (score > maxScore)
                {
                    maxScore = score;
                    bestCell = cell;
                }
            }

            return bestCell != null ? bestCell : unblockedCells[0];
        }
        else if (m_autoplayMode == eAutoplayMode.AUTO_LOSE)
        {
            foreach (var cell in unblockedCells)
            {
                NormalItem nItem = cell.Item as NormalItem;
                if (nItem != null && !holderCounts.ContainsKey(nItem.ItemType))
                {
                    return cell;
                }
            }

            foreach (var cell in unblockedCells)
            {
                NormalItem nItem = cell.Item as NormalItem;
                if (nItem != null && holderCounts.ContainsKey(nItem.ItemType) && holderCounts[nItem.ItemType] < 2)
                {
                    return cell;
                }
            }

            return unblockedCells[0];
        }

        return unblockedCells[0];
    }
}
