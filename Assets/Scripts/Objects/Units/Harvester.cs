using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Harvester : Unit
{
    // Public variables
    public float capacity;
    public Building depot;
    public float collAmt, depAmt;

    // Private variables
    private bool harvesting = false, emptying = false;
    private float currLoad = 0.0f, currDeposit = 0.0f;
    private Resource resDeposit;
    private ResourceType harvestType;

    /* - Game Engine Methods - */

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        
        if (!rotating && !moving)
        {
            if (harvesting || emptying)
            {
                Arms[] arms = GetComponentsInChildren<Arms>();
                foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = true;
                if (harvesting)
                {
                    Collect();
                    if (currLoad >= capacity || resDeposit.IsEmpty())
                    {
                        currLoad = Mathf.Floor(currLoad);
                        harvesting = false;
                        emptying = true;
                        foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = false;
                        StartMove(depot.transform.position, depot.gameObject);
                    }
                } 
                else
                {
                    Deposit();
                    if (currLoad <= 0)
                    {
                        emptying = false;
                        foreach (Arms arm in arms) arm.GetComponent<Renderer>().enabled = false;
                        if (!resDeposit.IsEmpty())
                        {
                            harvesting = true;
                            StartMove(resDeposit.transform.position, resDeposit.gameObject);
                        }
                    }
                }
            }
        }
        
    }

    /* - Public methods - */
    
    public override void SetHoverState(GameObject hoverObject)
    {
        base.SetHoverState(hoverObject);
        if (IsSelected())
        {
            if (hoverObject.name != "Ground")
            {
                Resource resource = hoverObject.transform.parent.GetComponent<Resource>();
                if (resource && !resource.IsEmpty()) player.hud.SetCursorState(CursorState.Harvest);
            }
        }
    }

    public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
    {
        base.MouseClick(hitObject, hitPoint, controller);
        if (player && player.human)
        {
            if (hitObject.name != "Ground")
            {
                Resource resource = hitObject.transform.parent.GetComponent<Resource>();
                if (resource && !resource.IsEmpty())
                {
                    if (player.SelectedObject) player.SelectedObject.SetSelection(false, playingArea);
                    SetSelection(true, playingArea);
                    player.SelectedObject = this;
                    StartHarvest(resource);
                }
            }
            else StopHarvest();
        }
    }

    public void StartMove(Vector3 destination, GameObject target)
    {
        StartMove(destination);
        destinationTarget = target;
    }
    
    /* - Private methods - */
    
    private void StartHarvest(Resource resource)
    {
        resDeposit = resource;
        StartMove(resource.transform.position, resource.gameObject);
        if (harvestType == ResourceType.Unknown || harvestType != resource.GetResType())
        {
            harvestType = resource.GetResType();
            currLoad = 0.0f;
        }
        harvesting = true;
        emptying = false;
    }

    private void StopHarvest()
    {

    }

    private void Collect()
    {
        float collect = collAmt * Time.deltaTime;
        if (currLoad + collect > capacity) collect = capacity - currLoad;
        resDeposit.Remove(collect);
        currLoad += collect;
    }

    private void Deposit()
    {
        currDeposit += depAmt * Time.deltaTime;
        int deposit = Mathf.FloorToInt(currDeposit);
        if (deposit >= 1)
        {
            if (deposit > currLoad) deposit = Mathf.FloorToInt(currLoad);
            currDeposit -= deposit;
            currLoad -= deposit;
            ResourceType depositType = harvestType;
            if (harvestType == ResourceType.Gold) depositType = ResourceType.Money;
            player.AddResource(depositType, deposit);
        }
    }
    
}
