﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Building : WorldObject
{

    public float maxBuildProgress;
    protected Queue<string> buildQueue;
    private float currentBuildProgress = 0.0f;
    private Vector3 spawnPoint;
    protected Vector3 rallyPoint;

    protected override void Awake()
    {
        base.Awake();

        buildQueue = new Queue<string>();
        float spawnX = selectionBounds.center.x + transform.forward.x * (selectionBounds.extents.x + 2);
        float spawnZ = selectionBounds.center.z + transform.forward.z * (selectionBounds.extents.z + 2);
        spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);

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
                if (player) player.AddUnit(buildQueue.Dequeue(), spawnPoint, transform.rotation);
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
}
