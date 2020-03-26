using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Worker : Unit
{

    public int buildSpeed;

    private Building currentProject;
    private bool building;
    private float amtBuilt = 0.0f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        actions = new string[] { "Factory" };
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    // Public methods

    public override void SetBuilding(Building project)
    {
        base.SetBuilding(project);
        currentProject = project;
        StartMove(currentProject.transform.position); //currentProject.gameObject);
        building = true;
    }

    public override void PerformAction(string actionToPerform)
    {
        base.PerformAction(actionToPerform);
        CreateBuilding(actionToPerform);
    }

    public override void StartMove(Vector3 destination)
    {
        base.StartMove(destination);
        amtBuilt = 0.0f;
    }

    private void CreateBuilding(string buildingName)
    {

    }
}
