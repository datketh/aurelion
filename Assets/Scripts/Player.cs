using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Player : MonoBehaviour
{
    public string username;
    public bool human;
    public PlayerUI hud;
    public WorldObject SelectedObject { get; set; }

    public int startMoney, startMoneyLimit, startPower, startPowerLimit;

    private Dictionary<ResourceType, int> resources, resourceLimits;

    private void Awake()
    {
        resources = InitResourceList();
        resourceLimits = InitResourceList();
    }

    // Start is called before the first frame update
    void Start()
    {
        hud = GetComponentInChildren<PlayerUI>();
        AddStartResourceLimits();
        AddStartResources();
    }

    // Update is called once per frame
    void Update()
    {
        if (human)
        {
            hud.SetResourceValues(resources, resourceLimits);
        }
    }

    public void AddResource(ResourceType type, int amt)
    {
        resources[type] += amt;
    }

    public void IncrementResourceLimit(ResourceType type, int amt)
    {
        resourceLimits[type] += amt;
    }

    private Dictionary<ResourceType, int> InitResourceList()
    {
        Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
        list.Add(ResourceType.Money, 0);
        list.Add(ResourceType.Power, 0);
        return list;
    }

    private void AddStartResourceLimits()
    {
        IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
        IncrementResourceLimit(ResourceType.Power, startPowerLimit);
    }

    private void AddStartResources()
    {
        AddResource(ResourceType.Money, startMoney);
        AddResource(ResourceType.Power, startPower);
    }

    public void AddUnit(string name, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation)
    {
        Units units = GetComponentInChildren<Units>();
        GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(name), spawnPoint, rotation);
        newUnit.transform.parent = units.transform;
        Unit unitObject = newUnit.GetComponent<Unit>();
        if (unitObject && spawnPoint != rallyPoint) unitObject.StartMove(rallyPoint);
    }
}
