using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum LeafState { Fresh, Eaten, Wilted }

[Serializable]
public class LeafData
{
    public string id;
    public LeafState state;
    public RoutePoint point;
    public LeafVisualController visual;
}

public class BranchStatus : MonoBehaviour
{
    public List<LeafData> leaves;
    public RoutePoint[] sleepPoints = new RoutePoint[2];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAllFresh()
    {
        foreach (var l in leaves)
        {
            l.state = LeafState.Fresh;
            if (l.visual != null) l.visual.ShowFresh();
        }
    }

    public void SetEaten(string id)
    {
        var leaf = leaves.Find(l => l.id == id);
        if (leaf == null) return;
        leaf.state = LeafState.Eaten;
        //TODO:ÉAÉjÉÅÇ…íuä∑
        if (leaf.visual != null) leaf.visual.PlayEatenTransition();
    }

    public void SetRemainingWilted()
    {
        foreach (var l in leaves)
        {
            if (l.state == LeafState.Fresh)
            {
                l.state = LeafState.Wilted;
                if (l.visual != null) l.visual.ShowWilted();
            }
        }
    }

    public RoutePoint GetRoutePointById(string id)
    {
        if (leaves == null || string.IsNullOrEmpty(id))
            return null;

        for (int i = 0; i < leaves.Count; i++)
        {
            var l = leaves[i];
            if (l != null && l.id == id)
                return l.point;
        }
        return null;
    }

    [ContextMenu("DEBUG/SetAllFresh")]
    void DBG_SetAllFresh() => SetAllFresh();

    [ContextMenu("DEBUG/SetEaten_A")]
    void DBG_SetEatenA() => SetEaten("A");

    [ContextMenu("DEBUG/SetRemainingWilted")]
    void DBG_SetRemainingWilted() => SetRemainingWilted();
}
