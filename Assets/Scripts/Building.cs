using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Building : WorldObject
{

    public Texture2D rallyPointImage;
    public Texture2D sellImage;
    public float maxBuildProgress;
    protected Queue<string> buildQueue;
    private float currentBuildProgress = 0.0f;
    private Vector3 spawnPoint;
    protected Vector3 rallyPoint;
    private bool needsBuilding = false;

    protected override void Awake()
    {
        base.Awake();

        buildQueue = new Queue<string>();
        float spawnX = selectionBounds.center.x + transform.forward.x * (selectionBounds.extents.x - 1);
        float spawnZ = selectionBounds.center.z + transform.forward.z * (selectionBounds.extents.z - 1);
        spawnPoint = new Vector3(spawnX, transform.position.y, spawnZ);

        rallyPoint = spawnPoint;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        ProcessBuildQueue();
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }

    private void DrawBuildProgress()
    {
        GUI.skin = ResourceManager.SelectBoxSkin;
        Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);

        GUI.BeginGroup(playingArea);
        CalculateCurrentHealth(0.5f, 0.99f);
        DrawHealthBar(selectBox, "Building...");
        GUI.EndGroup();
    }

    public void Sell()
    {
        if (player) player.AddResource(ResourceType.Money, sellValue);
        if (currentlySelected) SetSelection(false, playingArea);
        Destroy(this.gameObject);
    }

    public override void SetHoverState(GameObject hoverObject)
    {
        base.SetHoverState(hoverObject);

        if (IsSelected())
        {
            if (hoverObject.name == "Ground")
            {
                if (player.hud.GetPreviousCursorState() == CursorState.RallyPoint) player.hud.SetCursorState(CursorState.RallyPoint);
            }
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
    {
        base.MouseClick(hitObject, hitPoint, controller);

        if (IsSelected())
        {
            if (hitObject.name == "Ground")
            {
                if ((player.hud.GetCursorState() == CursorState.RallyPoint || player.hud.GetPreviousCursorState() == CursorState.RallyPoint) &&
                        hitPoint != ResourceManager.InvalidPosition)
                {
                    SetRallyPoint(hitPoint);
                    player.hud.SetCursorState(CursorState.PanRight);
                    player.hud.SetCursorState(CursorState.Select);
                }
            }
        }
    }

    public override void SetSelection(bool selected, Rect playArea)
    {
        base.SetSelection(selected, playArea);

        if (player)
        {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
            if (selected)
            {
                if (flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition)
                {
                    flag.transform.localPosition = rallyPoint;
                    flag.transform.forward = transform.forward;
                    flag.Enable();
                } else
                {
                    if (flag && player.human) flag.Disable();
                }
            }
        }
    }

    public void StartConstruction()
    {
        CalculateBounds();
        needsBuilding = true;
        hitPoints = 0;
    }

    public void SetRallyPoint(Vector3 pos)
    {
        rallyPoint = pos;
        if (IsSelected())
        {
            RallyPoint flag = player.GetComponentInChildren<RallyPoint>();
            if (flag) flag.transform.localPosition = rallyPoint;
        }
    }

    protected void CreateUnit(string unitName)
    {
        buildQueue.Enqueue(unitName);
    }

    protected void ProcessBuildQueue()
    {
        if (buildQueue.Count > 0)
        {
            currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
            if (currentBuildProgress > maxBuildProgress)
            {
                if (player) player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);
                currentBuildProgress = 0.0f;
            }
        }
    }

    public string[] GetBuildQueueValues()
    {
        string[] values = new string[buildQueue.Count];
        int pos = 0;
        foreach(string unit in buildQueue)
        {
            values[pos++] = unit;
        }
        return values;
    }

    public float GetBuildPercentage()
    {
        return currentBuildProgress / maxBuildProgress;
    }

    public bool HasSpawnPoint()
    {
        return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
    }
}
