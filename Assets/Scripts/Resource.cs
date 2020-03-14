using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class Resource : WorldObject
{
    // Public variables
    public float capacity;

    // Subclass-accessible variables
    protected float amtLeft;
    protected ResourceType type;

    /* - Game Engine Methods - */

    protected override void Start()
    {
        base.Start();
        amtLeft = capacity;
        type = ResourceType.Unknown;
    }

    /* - Public methods - */

    public void Remove(float amt)
    {
        amtLeft -= amt;
        if (amtLeft < 0) amtLeft = 0;
    }

    public bool IsEmpty()
    {
        return amtLeft <= 0;
    }

    public ResourceType GetType()
    {
        return type;
    }
}
