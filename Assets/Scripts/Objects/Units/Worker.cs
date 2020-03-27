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

        if (!moving && !rotating)
        {
            if (building && currentProject && currentProject.UnderConstruction())
            {
                amtBuilt += buildSpeed * Time.deltaTime;
                int amt = Mathf.FloorToInt(amtBuilt);
                if (amt > 0)
                {
                    amtBuilt -= amt;
                    currentProject.Construct(amt);
                    if (!currentProject.UnderConstruction()) building = false;
                }
            }
        }
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
        building = false;
    }

    private void CreateBuilding(string buildingName)
    {
        Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
        if (player) player.CreateBuilding(buildingName, buildPoint, this, playingArea);
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
    {
        bool doBase = true;
        //only handle input if owned by a human player and currently selected
        if (player && player.human && currentlySelected && hitObject && hitObject.name != "Ground")
        {
            Building building = hitObject.transform.parent.GetComponent<Building>();
            if (building)
            {
                if (building.UnderConstruction())
                {
                    SetBuilding(building);
                    doBase = false;
                }
            }
        }
        if (doBase) base.MouseClick(hitObject, hitPoint, controller);
    }
}
