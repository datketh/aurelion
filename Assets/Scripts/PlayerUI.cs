﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private const int ORDERS_BAR_WIDTH = 100, RESOURCE_BAR_HEIGHT = 40;
    private Player player;

    public GUISkin resourceSkin, ordersSkin;
    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.GetComponent<Player>();
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (player && player.human)
        {
            DrawOrdersBar();
            DrawResourceBar();
        }
    }

    private void DrawOrdersBar ()
    {

    }

    private void DrawResourceBar ()
    {

    }
}
