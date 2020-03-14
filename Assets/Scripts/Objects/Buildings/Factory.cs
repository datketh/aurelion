using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Building
{

    protected override void Start()
    {
        base.Start();

        actions = new string[] { "Tank", "Tank", "Harvester" };
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateUnit(actionToPerform);
        Debug.Log("performing action");
    }
}
