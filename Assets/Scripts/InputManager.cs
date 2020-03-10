﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class InputManager : MonoBehaviour {

    private Player player;

    // Start is called before the first frame update
    void Start() {
        player = transform.root.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update() {
        if (player && player.human)
        {
            MoveCamera();
            RotateCamera();
            MouseActivity();
        }
        
    }

    void MoveCamera()
    {
        // Get mouse position and initialize movement vector
        float xPos = Input.mousePosition.x;
        float yPos = Input.mousePosition.y;
        Vector3 movement = new Vector3(0, 0, 0);

        Debug.Log(xPos);
        Debug.Log(yPos);

        // Get X scrolling movement
        if (xPos >= 0 && xPos < ResourceManager.ScrollWidth)
        {
            movement.x -= ResourceManager.ScrollSpeed;
        } else if (xPos <= Screen.width && xPos > Screen.width - ResourceManager.ScrollWidth)
        {
            movement.x += ResourceManager.ScrollSpeed;
        }

        // Get Y scrolling movement
        if (yPos >= 0 && yPos < ResourceManager.ScrollWidth)
        {
            movement.z -= ResourceManager.ScrollSpeed;
        }
        else if (yPos <= Screen.height && yPos > Screen.height - ResourceManager.ScrollWidth)
        {
            movement.z += ResourceManager.ScrollSpeed;
        }

        // Keep movement in the direction the camera is pointed
        movement = Camera.main.transform.TransformDirection(movement);
        movement.y = 0;

        // Get camera zoom
        movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");

        // Calculate new camera position
        Vector3 origin = Camera.main.transform.position;
        Vector3 destination = origin;
        destination.x += movement.x;
        destination.y += movement.y;
        destination.z += movement.z;

        // Restrict camera height
        if (destination.y > ResourceManager.MaxCameraHeight)
        {
            destination.y = ResourceManager.MaxCameraHeight;
        } else if (destination.y < ResourceManager.MinCameraHeight)
        {
            destination.y = ResourceManager.MinCameraHeight;
        }

        // If position is changed, move to new position
        if (destination != origin)
        {
            Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
            Debug.Log("Should be moving camera");
        }
    }

    void RotateCamera()
    {   
        // Initialize origin and destination points
        Vector3 origin = Camera.main.transform.eulerAngles;
        Vector3 destination = origin;

        // Get user input
        if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButton(1))
        {
            destination.x -= Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
            destination.y += Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
        }

        // If is angle is changed, update transform
        if (destination != origin)
        {
            Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
        }
    }

    private void MouseActivity()
    {
        if (Input.GetMouseButtonDown(0)) LeftMouseClick();
        else if (Input.GetMouseButtonDown(1)) RightMouseClick();
    }

    private void LeftMouseClick()
    {
        if (player.hud.MouseInBounds())
        {
            GameObject hitObject = FindHitObject();
            Vector3 hitPoint = FindHitPoint();
            if (hitObject && hitPoint != ResourceManager.InvalidPosition)
            {
                if (player.SelectedObject) player.SelectedObject.MouseClick(hitObject, hitPoint, player);
                else if (hitObject.name != "Ground")
                {
                    WorldObject worldObject = hitObject.transform.root.GetComponent<WorldObject>();
                    if (worldObject)
                    {
                        //we already know the player has no selected object
                        player.SelectedObject = worldObject;
                        worldObject.SetSelection(true);
                    }
                }
            }
        }
    }

    private void RightMouseClick()
    {
        if (player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObject)
        {
            player.SelectedObject.SetSelection(false);
            player.SelectedObject = null;
        }
    }

    private GameObject FindHitObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.collider.gameObject;
        return null;
    }

    private Vector3 FindHitPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) return hit.point;
        return ResourceManager.InvalidPosition;
    }
}
