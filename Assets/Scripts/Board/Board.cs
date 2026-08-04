using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    public enum eMatchDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }

    private int boardSizeX;

    private int boardSizeY;

    private int layerCount;

    private int totalTriples;

    private int bottomRowSize;

    private Cell[,,] m_cells;

    private Cell[] m_bottomCells;

    public Cell[] BottomCells => m_bottomCells;

    public int LayerCount => layerCount;

    public int BoardSizeX => boardSizeX;

    public int BoardY => boardSizeY;

    private Transform m_root;

    private int m_matchMin;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        int squareSize = Mathf.Max(gameSettings.BoardSizeX, gameSettings.BoardSizeY);
        this.boardSizeX = squareSize;
        this.boardSizeY = squareSize;
        this.layerCount = gameSettings.LayerCount > 0 ? gameSettings.LayerCount : 3;
        this.totalTriples = gameSettings.TotalTriples > 0 ? gameSettings.TotalTriples : 16;
        this.bottomRowSize = gameSettings.BottomRowSize > 0 ? gameSettings.BottomRowSize : 5;

        m_cells = new Cell[boardSizeX, boardSizeY, layerCount];
        m_bottomCells = new Cell[bottomRowSize];

        CreateBoard();
        CreateBottomRow();
    }

    private void CreateBoard()
    {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int z = 0; z < layerCount; z++)
        {
            int sizeX = boardSizeX - (z * 2);
            int sizeY = boardSizeY - (z * 2);

            if (sizeX <= 0 || sizeY <= 0) break;

            Vector3 layerOrigin = new Vector3(-(sizeX - 1) * 0.5f, -(sizeY - 1) * 0.5f, 0f);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    GameObject go = GameObject.Instantiate(prefabBG);
                    go.transform.position = layerOrigin + new Vector3(x, y, 0f);
                    go.transform.SetParent(m_root);

                    Cell cell = go.GetComponent<Cell>();
                    cell.Setup(x, y, z);
                    cell.SetSortingOrder(z * 10);

                    m_cells[x, y, z] = cell;
                }
            }
        }
    }

    private void CreateBottomRow()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        Vector3 bottomOrigin = new Vector3(-bottomRowSize * 0.5f + 0.5f, origin.y - 1.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int i = 0; i < bottomRowSize; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = bottomOrigin + new Vector3(i, 0f, 0f);
            go.transform.SetParent(m_root);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, -1);

            m_bottomCells[i] = cell;
        }

        for (int i = 0; i < bottomRowSize; i++)
        {
            if (i + 1 < bottomRowSize) m_bottomCells[i].NeighbourRight = m_bottomCells[i + 1];
            if (i > 0) m_bottomCells[i].NeighbourLeft = m_bottomCells[i - 1];
        }
    }

    public Cell GetFirstEmptyHolderCell()
    {
        for (int i = 0; i < bottomRowSize; i++)
        {
            if (m_bottomCells[i].IsEmpty)
            {
                return m_bottomCells[i];
            }
        }
        return null;
    }

    public bool IsHolderFull()
    {
        for (int i = 0; i < bottomRowSize; i++)
        {
            if (m_bottomCells[i].IsEmpty)
            {
                return false;
            }
        }
        return true;
    }

    public int BottomRowSize => bottomRowSize;

    public Cell GetBottomCell(int index)
    {
        if (index >= 0 && index < bottomRowSize) return m_bottomCells[index];
        return null;
    }

    public bool CheckAndExplodeHolderMatches(Action callback)
    {
        Dictionary<NormalItem.eNormalType, List<Cell>> matchesByType = new Dictionary<NormalItem.eNormalType, List<Cell>>();

        for (int i = 0; i < bottomRowSize; i++)
        {
            if (m_bottomCells[i].IsEmpty) continue;

            NormalItem normalItem = m_bottomCells[i].Item as NormalItem;
            if (normalItem != null)
            {
                if (!matchesByType.ContainsKey(normalItem.ItemType))
                {
                    matchesByType[normalItem.ItemType] = new List<Cell>();
                }
                matchesByType[normalItem.ItemType].Add(m_bottomCells[i]);
            }
        }

        List<Cell> matchingCellsToExplode = null;
        foreach (var pair in matchesByType)
        {
            if (pair.Value.Count >= 3)
            {
                matchingCellsToExplode = pair.Value.Take(3).ToList();
                break;
            }
        }

        if (matchingCellsToExplode != null && matchingCellsToExplode.Count >= 3)
        {
            foreach (var cell in matchingCellsToExplode)
            {
                cell.ExplodeItem();
            }

            ShiftHolderItemsLeft();

            if (callback != null) callback();
            return true;
        }

        if (callback != null) callback();
        return false;
    }

    public void ShiftHolderItemsLeft()
    {
        List<Item> remainingItems = new List<Item>();
        for (int i = 0; i < bottomRowSize; i++)
        {
            if (!m_bottomCells[i].IsEmpty)
            {
                remainingItems.Add(m_bottomCells[i].Item);
                m_bottomCells[i].Free();
            }
        }

        for (int i = 0; i < remainingItems.Count; i++)
        {
            m_bottomCells[i].Assign(remainingItems[i]);
            m_bottomCells[i].ApplyItemMoveToPosition();
        }
    }

    internal void Fill()
    {
        List<Cell> allCells = new List<Cell>();
        for (int z = 0; z < layerCount; z++)
        {
            for (int x = 0; x < boardSizeX; x++)
            {
                for (int y = 0; y < boardSizeY; y++)
                {
                    Cell cell = m_cells[x, y, z];
                    if (cell != null)
                    {
                        cell.Clear();
                        allCells.Add(cell);
                    }
                }
            }
        }

        int remainder = allCells.Count % 3;
        if (remainder > 0)
        {
            for (int i = 0; i < remainder; i++)
            {
                int lastIdx = allCells.Count - 1;
                Cell extraCell = allCells[lastIdx];
                allCells.RemoveAt(lastIdx);

                if (extraCell != null)
                {
                    m_cells[extraCell.BoardX, extraCell.BoardY, extraCell.LayerZ] = null;
                    extraCell.Clear();
                    GameObject.Destroy(extraCell.gameObject);
                }
            }
        }

        for (int i = 0; i < allCells.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, allCells.Count);
            Cell temp = allCells[i];
            allCells[i] = allCells[rnd];
            allCells[rnd] = temp;
        }

        int tripleCount = allCells.Count / 3;

        List<NormalItem.eNormalType> itemTypeList = new List<NormalItem.eNormalType>();

        Array allTypes = Enum.GetValues(typeof(NormalItem.eNormalType));
        foreach (NormalItem.eNormalType t in allTypes)
        {
            if (itemTypeList.Count / 3 < tripleCount)
            {
                itemTypeList.Add(t);
                itemTypeList.Add(t);
                itemTypeList.Add(t);
            }
        }

        while (itemTypeList.Count / 3 < tripleCount)
        {
            NormalItem.eNormalType t = Utils.GetRandomNormalType();
            itemTypeList.Add(t);
            itemTypeList.Add(t);
            itemTypeList.Add(t);
        }

        for (int i = 0; i < itemTypeList.Count; i++)
        {
            int rnd = UnityEngine.Random.Range(i, itemTypeList.Count);
            NormalItem.eNormalType temp = itemTypeList[i];
            itemTypeList[i] = itemTypeList[rnd];
            itemTypeList[rnd] = temp;
        }

        for (int i = 0; i < allCells.Count; i++)
        {
            Cell cell = allCells[i];
            NormalItem item = new NormalItem();
            item.SetType(itemTypeList[i]);
            item.SetView();
            item.SetViewRoot(m_root);

            cell.Assign(item);
            cell.ApplyItemPosition(false);
            item.SetSortingOrder(cell.LayerZ * 10 + 1);
        }

        UpdateBlockedVisuals();
    }

    public Cell GetFirstEmptyBoardCell()
    {
        for (int z = 0; z < layerCount; z++)
        {
            int sizeX = boardSizeX - (z * 2);
            int sizeY = boardSizeY - (z * 2);
            if (sizeX <= 0 || sizeY <= 0) continue;

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Cell cell = m_cells[x, y, z];
                    if (cell != null && cell.IsEmpty)
                    {
                        return cell;
                    }
                }
            }
        }
        return null;
    }

    public List<Cell> GetUnblockedCells()
    {
        List<Cell> result = new List<Cell>();
        for (int z = 0; z < layerCount; z++)
        {
            int sizeX = boardSizeX - (z * 2);
            int sizeY = boardSizeY - (z * 2);
            if (sizeX <= 0 || sizeY <= 0) continue;

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Cell cell = m_cells[x, y, z];
                    if (cell != null && !cell.IsEmpty && !IsCellBlocked(cell))
                    {
                        result.Add(cell);
                    }
                }
            }
        }
        return result;
    }

    public bool IsCellBlocked(Cell cell)
    {
        if (cell == null || cell.IsEmpty) return false;
        if (cell.BoardY < 0) return false;

        for (int z = cell.LayerZ + 1; z < layerCount; z++)
        {
            int sizeX = boardSizeX - (z * 2);
            int sizeY = boardSizeY - (z * 2);
            if (sizeX <= 0 || sizeY <= 0) continue;

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Cell upperCell = m_cells[x, y, z];
                    if (upperCell != null && !upperCell.IsEmpty)
                    {
                        float dist = Vector2.Distance(cell.transform.position, upperCell.transform.position);
                        if (dist < 0.95f)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void UpdateBlockedVisuals()
    {
        for (int z = 0; z < layerCount; z++)
        {
            for (int x = 0; x < boardSizeX; x++)
            {
                for (int y = 0; y < boardSizeY; y++)
                {
                    Cell cell = m_cells[x, y, z];
                    if (cell != null && !cell.IsEmpty)
                    {
                        bool blocked = IsCellBlocked(cell);
                        cell.SetBlockedVisual(blocked);
                    }
                }
            }
        }
    }

    public bool IsMainBoardCleared()
    {
        for (int z = 0; z < layerCount; z++)
        {
            for (int x = 0; x < boardSizeX; x++)
            {
                for (int y = 0; y < boardSizeY; y++)
                {
                    if (m_cells[x, y, z] != null && !m_cells[x, y, z].IsEmpty)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public bool IsBoardCleared()
    {
        if (!IsMainBoardCleared()) return false;

        for (int i = 0; i < bottomRowSize; i++)
        {
            if (m_bottomCells[i] != null && !m_bottomCells[i].IsEmpty)
            {
                return false;
            }
        }

        return true;
    }



    public void Clear()
    {
        if (m_cells != null)
        {
            for (int z = 0; z < layerCount; z++)
            {
                for (int x = 0; x < boardSizeX; x++)
                {
                    for (int y = 0; y < boardSizeY; y++)
                    {
                        Cell cell = m_cells[x, y, z];
                        if (cell != null)
                        {
                            cell.Clear();
                            GameObject.Destroy(cell.gameObject);
                            m_cells[x, y, z] = null;
                        }
                    }
                }
            }
        }

        if (m_bottomCells != null)
        {
            for (int i = 0; i < bottomRowSize; i++)
            {
                Cell cell = m_bottomCells[i];
                if (cell != null)
                {
                    cell.Clear();
                    GameObject.Destroy(cell.gameObject);
                    m_bottomCells[i] = null;
                }
            }
        }
    }
}
