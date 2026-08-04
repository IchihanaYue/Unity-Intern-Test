using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int BoardX { get; private set; }

    public int BoardY { get; private set; }

    public int LayerZ { get; private set; }

    public Item Item { get; private set; }

    public Cell OriginalCell { get; set; }

    public Cell NeighbourUp { get; set; }

    public Cell NeighbourRight { get; set; }

    public Cell NeighbourBottom { get; set; }

    public Cell NeighbourLeft { get; set; }


    public bool IsEmpty => Item == null;

    public bool IsBlocked { get; private set; }

    public void Setup(int cellX, int cellY, int cellZ = 0)
    {
        this.BoardX = cellX;
        this.BoardY = cellY;
        this.LayerZ = cellZ;
    }

    public void SetSortingOrder(int order)
    {
        SpriteRenderer sp = GetComponent<SpriteRenderer>();
        if (sp)
        {
            sp.sortingOrder = order;
        }

        if (Item != null)
        {
            Item.SetSortingOrder(order + 1);
        }
    }

    public void SetBlockedVisual(bool isBlocked)
    {
        IsBlocked = isBlocked;
        Color col = isBlocked ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;

        SpriteRenderer sp = GetComponent<SpriteRenderer>();
        if (sp)
        {
            sp.color = col;
        }

        if (Item != null)
        {
            Item.SetColor(col);
        }
    }

    public bool IsNeighbour(Cell other)
    {
        return BoardX == other.BoardX && Mathf.Abs(BoardY - other.BoardY) == 1 ||
            BoardY == other.BoardY && Mathf.Abs(BoardX - other.BoardX) == 1;
    }


    public void Free()
    {
        Item = null;
    }

    public void Assign(Item item)
    {
        Item = item;
        Item.SetCell(this);
    }

    public void ApplyItemPosition(bool withAppearAnimation)
    {
        Item.SetViewPosition(this.transform.position);

        if (withAppearAnimation)
        {
            Item.ShowAppearAnimation();
        }
    }

    internal void Clear()
    {
        if (Item != null)
        {
            Item.Clear();
            Item = null;
        }
    }

    internal bool IsSameType(Cell other)
    {
        return Item != null && other.Item != null && Item.IsSameType(other.Item);
    }

    internal void ExplodeItem()
    {
        if (Item == null) return;

        Item.ExplodeView();
        Item = null;
    }

    internal void AnimateItemForHint()
    {
        Item.AnimateForHint();
    }

    internal void StopHintAnimation()
    {
        Item.StopAnimateForHint();
    }

    internal void ApplyItemMoveToPosition()
    {
        Item.AnimationMoveToPosition();
    }
}
