using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class GoldMine : Resource
{

    // Private variables
    private int numBlocks;

    /* Game engine methods */

    protected override void Start()
    {
        base.Start();
        numBlocks = GetComponentsInChildren<Gold>().Length;
        type = ResourceType.Money;
    }

    protected override void Update()
    {
        base.Update();

        float percentLeft = (float)amtLeft / (float)capacity;
        if (percentLeft < 0) percentLeft = 0;
        int numBlocksToShow = (int)(percentLeft * numBlocks);
        Gold[] blocks = GetComponentsInChildren<Gold>();
        if (numBlocksToShow >= 0 && numBlocksToShow < blocks.Length)
        {
            Gold[] sortedBlocks = new Gold[blocks.Length];

            foreach (Gold gold in blocks)
            {
                sortedBlocks[blocks.Length - int.Parse(gold.name)] = gold;
            }

            for (int i = 0; i < sortedBlocks.Length; i++)
            {
                sortedBlocks[i].GetComponent<Renderer>().enabled = false;
            }
            CalculateBounds();

        }
    }

}
