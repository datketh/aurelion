using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS;

public class PlayerUI : MonoBehaviour
{
    private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
    private const int SELECTION_NAME_HEIGHT = 15;
    private const int ICON_WIDTH = 32, ICON_HEIGHT = 32;
    private const int TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
    private const int BUILD_IMG_WIDTH = 64, BUILD_IMG_HEIGHT = 64, BUILD_IMG_PADDING = 8;
    private const int SCROLL_BAR_WIDTH = 22;
    private const int BUTTON_SPACING = 7;

    private Player player;

    public GUISkin resourceSkin, ordersSkin;
    public GUISkin selectBoxSkin;
    public GUISkin mouseCursorSkin;

    public Texture2D healthy, damaged, critical;

    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor, rallyPointCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursors;

    public Texture2D buttonClick, buttonHover;
    public Texture2D smallButtonHover, smallButtonClick;

    public Texture2D buildFrame, buildMask;

    public Texture2D[] resources;
    private Dictionary<ResourceType, Texture2D> resourceImages;

    private CursorState activeCursorState;
    private CursorState previousCursorState;
    private int currentFrame;

    private Dictionary<ResourceType, int> resourceValues, resourceLimits;

    private WorldObject lastSelection;
    private float sliderValue;
    private int buildAreaHeight;


    // Start is called before the first frame update
    void Start()
    {
        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
        SetCursorState(CursorState.Select);
        resourceValues = new Dictionary<ResourceType, int>();
        resourceLimits = new Dictionary<ResourceType, int>();
        InitializeResourceImages();

        buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (player && player.human)
        {
            DrawOrdersBar();
            DrawResourceBar();
            DrawMouseCursor();
        }
    }

    private void DrawOrdersBar ()
    {
        GUI.skin = ordersSkin;
        GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMG_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMG_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
        GUI.Box(new Rect(BUILD_IMG_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");

        string selectionName = "";
        if (player.SelectedObject)
        {
            selectionName = player.SelectedObject.objectName;
            if (player.SelectedObject.IsOwnedBy(player) && !player.SelectedObject.UnderConstruction())
            {
                if (lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
                DrawActions(player.SelectedObject.GetActions());
                // Store current selection
                lastSelection = player.SelectedObject;

                Building selectedBuilding = lastSelection.GetComponent<Building>();
                if (selectedBuilding)
                {
                    DrawBuildQueue(selectedBuilding.GetBuildQueueValues(), selectedBuilding.GetBuildPercentage());
                    DrawStandardBuildingOptions(selectedBuilding);
                }
            }
        }
        if (!selectionName.Equals(""))
        {
            int leftPos = BUILD_IMG_WIDTH + SCROLL_BAR_WIDTH / 2;
            int topPos = buildAreaHeight + BUTTON_SPACING;
            GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
        }

        GUI.EndGroup();
    }

    private void DrawStandardBuildingOptions(Building building)
    {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = smallButtonHover;
        buttons.active.background = smallButtonClick;
        GUI.skin.button = buttons;

        int left = BUILD_IMG_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
        int top = buildAreaHeight - BUILD_IMG_HEIGHT / 2;
        int width = BUILD_IMG_WIDTH / 2;
        int height = BUILD_IMG_HEIGHT / 2;

        if (GUI.Button(new Rect(left, top, width, height), building.sellImage))
        {
            building.Sell();
        }

        left += width + BUTTON_SPACING;

        if (building.HasSpawnPoint())
        {
            if (GUI.Button(new Rect(left, top, width, height), building.rallyPointImage))
            {
                if (activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint) SetCursorState(CursorState.RallyPoint);
                else
                {
                    SetCursorState(CursorState.PanRight);
                    SetCursorState(CursorState.Select);
                }
                
            }
        }
    }

    private void DrawActions(string[] actions)
    {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        GUI.skin.button = buttons;

        int numActions = actions.Length;

        GUI.BeginGroup(new Rect(BUILD_IMG_WIDTH, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
        // 
        if (numActions >= MaxNumRows(buildAreaHeight)) DrawSlider(buildAreaHeight, numActions / 2.0f);

        for (int i = 0; i < numActions; i++)
        {
            int column = i % 2;
            int row = i / 2;
            Rect pos = GetButtonPos(row, column);
            Texture2D action = ResourceManager.GetBuildImage(actions[i]);
            if (action)
            {
                if (GUI.Button(pos, action))
                {

                    if (player.SelectedObject)
                    {
                        player.SelectedObject.PerformAction(actions[i]);
                    }
                }
            }
        }

        GUI.EndGroup();
    }

    private int MaxNumRows(int areaHeight)
    {
        return areaHeight / BUILD_IMG_HEIGHT;
    }

    private Rect GetButtonPos(int row, int column)
    {
        int left = SCROLL_BAR_WIDTH + column * BUILD_IMG_WIDTH;
        float top = row * BUILD_IMG_HEIGHT - sliderValue * BUILD_IMG_HEIGHT;
        return new Rect(left, top, BUILD_IMG_WIDTH, BUILD_IMG_HEIGHT);
    }

    private void DrawSlider(int grpHeight, float numRows)
    {
        sliderValue = GUI.VerticalSlider(GetScrollPos(grpHeight), sliderValue, 0.0f, numRows - MaxNumRows(grpHeight));
    }

    private Rect GetScrollPos(int grpHeight)
    {
        return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, grpHeight - 2 * BUTTON_SPACING);
    }

    private void DrawBuildQueue(string[] queue, float percent)
    {
        for (int i = 0; i < queue.Length; i++)
        {
            float top = i * BUILD_IMG_HEIGHT - (i + 1) * BUILD_IMG_PADDING;
            Rect buildPos = new Rect(BUILD_IMG_PADDING, top, BUILD_IMG_WIDTH, BUILD_IMG_HEIGHT);
            GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(queue[i]));
            GUI.DrawTexture(buildPos, buildFrame);
            top += BUILD_IMG_PADDING;
            float w = BUILD_IMG_WIDTH - 2 * BUILD_IMG_PADDING;
            float h = BUILD_IMG_HEIGHT - 2 * BUILD_IMG_PADDING;
            if (i == 0)
            {
                top += h * percent;
                h *= (1 - percent);
            }
            GUI.DrawTexture(new Rect(2 * BUILD_IMG_PADDING, top, w, h), buildMask);
        }
    }

    private void DrawResourceBar ()
    {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
        GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");

        int topPos = 4, iconLeft = 4, textLeft = 20;
        DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
        iconLeft += TEXT_WIDTH;
        textLeft += TEXT_WIDTH;
        DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);

        GUI.EndGroup();
    }

    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos)
    {
        Texture2D icon = resourceImages[type];
        string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
        GUI.Label(new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
    }

    public void SetResourceValues(Dictionary<ResourceType, int> resValues, Dictionary<ResourceType, int> resLimits)
    {
        resourceValues = resValues;
        resourceLimits = resLimits;
    }

    private void InitializeResourceImages()
    {
        resourceImages = new Dictionary<ResourceType, Texture2D>();
        for (int i = 0; i < resources.Length; i++)
        {
            switch(resources[i].name)
            {
                case "Money":
                    resourceImages.Add(ResourceType.Money, resources[i]);
                    resourceValues.Add(ResourceType.Money, 0);
                    resourceLimits.Add(ResourceType.Money, 0);
                    break;
                case "Power":
                    resourceImages.Add(ResourceType.Power, resources[i]);
                    resourceValues.Add(ResourceType.Power, 0);
                    resourceLimits.Add(ResourceType.Power, 0);
                    break;
                default:
                    break;
            }
        }
    }

    public bool MouseInBounds()
    {
        //Screen coordinates start in the lower-left corner of the screen
        //not the top-left of the screen like the drawing coordinates do
        Vector3 mousePos = Input.mousePosition;
        bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
        bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
        return insideWidth && insideHeight;
    }

    public Rect GetPlayingArea()
    {
        return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
    }

    public void SetCursorState(CursorState newState)
    {
        if (activeCursorState != newState) previousCursorState = activeCursorState;
        activeCursorState = newState;
        switch (newState)
        {
            case CursorState.Select:
                activeCursor = selectCursor;
                break;
            case CursorState.Attack:
                activeCursor = attackCursors[currentFrame];
                break;
            case CursorState.Harvest:
                activeCursor = harvestCursors[currentFrame];
                break;
            case CursorState.Move:
                activeCursor = moveCursors[currentFrame];
                break;
            case CursorState.PanLeft:
                activeCursor = leftCursor;
                break;
            case CursorState.PanRight:
                activeCursor = rightCursor;
                break;
            case CursorState.PanUp:
                activeCursor = upCursor;
                break;
            case CursorState.PanDown:
                activeCursor = downCursor;
                break;

            case CursorState.RallyPoint:
                activeCursor = rallyPointCursor;
                break;
            default:
                break;
        }
    }

    private void DrawMouseCursor()
    {
        bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
        if(mouseOverHud)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
            if (!player.IsFindingBuildingLocation())
            {
                GUI.skin = mouseCursorSkin;
                GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
                UpdateCursorAnimation();
                Rect cursorPosition = GetCursorDrawPosition();
                GUI.Label(cursorPosition, activeCursor);
                GUI.EndGroup();
            }
            
        }
    }

    private void UpdateCursorAnimation()
    {
        if (activeCursorState == CursorState.Move)
        {
            currentFrame = (int)Time.time % moveCursors.Length;
            activeCursor = moveCursors[currentFrame];
        }
        else if (activeCursorState == CursorState.Attack)
        {
            currentFrame = (int)Time.time % attackCursors.Length;
            activeCursor = attackCursors[currentFrame];
        }
        else if (activeCursorState == CursorState.Harvest)
        {
            currentFrame = (int)Time.time % harvestCursors.Length;
            activeCursor = harvestCursors[currentFrame];
        }
    }

    private Rect GetCursorDrawPosition()
    {
        float leftPos = Input.mousePosition.x;
        float topPos = Screen.height - Input.mousePosition.y;

        if (activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
        else if (activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest)
        {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }
        else if (activeCursorState == CursorState.RallyPoint)
        {
            topPos -= activeCursor.height;
        }
        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }

    public CursorState GetCursorState()
    {
        return activeCursorState;
    }

    public CursorState GetPreviousCursorState()
    {
        return previousCursorState;
    }
}
