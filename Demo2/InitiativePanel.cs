using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Linq;
using System.Collections.Generic;

public class InitiativePanel : MonoBehaviour
{

    #region Variables
    // Dependencies
    [SerializeField] private GameObject placeholderPanel; // Panel used as a placeholder during drag-and-drop
    [SerializeField] private Transform canvas; // The canvas where UI elements are drawn
    [SerializeField] private Transform contentPanel; // Panel where initiative display objects are instantiated
    [SerializeField] private TMP_Text roundCounterText; // Text displaying the current round number
    [SerializeField] private GameObject infoPanel; // Panel displaying information
    [SerializeField] private GameObject initDisplayPrefab; // Prefab to instanciate for each client
    [SerializeField] private ScrollRect initiativeScrollRect; // Scroll rect for initiative panel
    [SerializeField] private float holdDeadZone = 100; // Dead zone for determining if a click is a hold
    [SerializeField] private Transform posIndicator; // Indicator for position during drag-and-drop
    [SerializeField] private LayerMask initPanelLayer; // Layer mask for initiative panel

    // State variables
    private bool isDragging = false; // Flag indicating if an object is being dragged
    private bool initiativeIsActive = false; // Flag indicating if initiative is currently active
    private int currentIndex = -1; // Index of the currently hovered over initiative display
    private int currentInitiativeIndex = -1; // Index of the current initiative display in the initiative order
    private int roundCounter = 1;  // Current round number
    private Vector2 startPointPos; // Starting position for drag-and-drop

    private GameObject placeholderPanelInstance; // Instance of the placeholder panel
    private ClientInitiativeDisplayPanel draggedObject; // Reference to the currently dragged initiative display

    // Data collections
    private List<MyDataTypes.ClientRollStatus> clientRollStatuses = new List<MyDataTypes.ClientRollStatus>(); // List of client roll statuses (ulong clientID, string nickname, bool hasRolled, int roll, int mod, int baseMod, int finalRoll)
    private List<ulong> clientIds = new List<ulong>(); // List of client IDs 
    private MyDataTypes.SerializableArrayOfNames clientNames; // Network Friendly Serializable Array of client names 
    private List<ClientInitiativeDisplayPanel> clientInitiativeDisplayPanels = new List<ClientInitiativeDisplayPanel>(); // List of client initiative display panels

    // Input controls
    private MainControls mainControls;  // Main input controls for the panel
    #endregion


    private void Awake()
    {
        // Initialize input controls
        mainControls = new MainControls();
        mainControls.PrimaryMap.Click.performed += ClickPerformed;
        mainControls.PrimaryMap.Click.started += ClickStarted;
        mainControls.PrimaryMap.Click.canceled += ClickCanceled;
        // Subscribe to event for receiving client rolls
        ServerNetworkManager.Singleton.OnClientSentRoll += OnClientSentRoll;
        // Set the layer mask for the initiative panel
        initPanelLayer = LayerMask.NameToLayer("InitPanel");
        ResetIndicesAndCounter();
    }
    // Reload clients in initiative data
    public void ReloadClients() {
        if(clientRollStatuses.Count <= 0) { return; }
        // Extract client IDs and names, IDs from 1000 are reserved for NPCs
        clientIds = clientRollStatuses.Where(s => s.cliendID < 1000).Select(s => s.cliendID).ToList();
        clientNames = new MyDataTypes.SerializableArrayOfNames(clientRollStatuses.Select(s => s.nickname).ToArray());
    }
    // Callback for when a client sends a roll
    public void OnClientSentRoll(MyDataTypes.Roll roll)
    {
        if (roll.checkType != (int)Enums.CheckType.Initiative) return;

        Debug.Log($"Client {roll.clientID} sent back roll");

        // Find the client status and update the roll
        var clientStatus = clientRollStatuses.FirstOrDefault(status => status.cliendID == roll.clientID);
        if (clientStatus != null)
        {
            clientStatus.SetRoll(roll.roll, roll.mod);
        }
        // Sort the roll displays based on initiative order if possible, update roll displays
        SortRollDisplays();
    }

    // Add a roll to the initiative panel
    public void AddRoll(MyDataTypes.ClientRollStatus rollStatus)
    {
        if (infoPanel == null || roundCounterText == null || initDisplayPrefab == null || contentPanel == null)
        {
            Debug.LogError("infoPanel, roundCounterText, initDisplayPrefab or contentPanel is null.");
            return;
        }
        infoPanel.SetActive(false);
        roundCounterText.SetText("W");

        // Instantiate and initialize a new initiative display object
        GameObject initiativeDisplayObject = Instantiate(initDisplayPrefab, contentPanel.transform);
        if (initiativeDisplayObject == null)
        {
            Debug.LogError("Failed to instantiate initDisplayPrefab.");
            return;
        }

        ClientInitiativeDisplayPanel displayPanel = initiativeDisplayObject.GetComponent<ClientInitiativeDisplayPanel>();
        if (displayPanel == null)
        {
            Debug.LogError("ClientInitiativeDisplayPanel component not found.");
            return;
        }

        displayPanel.Init(this);
        displayPanel.SetDisplay(rollStatus);

        // Add roll status and display panel to their respective lists
        clientRollStatuses.Add(rollStatus);
        clientInitiativeDisplayPanels.Add(displayPanel);

        ReloadClients();
    }
    // Remove a roll from the initiative panel
    public void RemoveRoll(MyDataTypes.ClientRollStatus rollStatus) {
        // Inform client to close initiative window
        ClientNetworkManager.Singleton.SetInitiativeDisplayClientRpc(
            null, 0, 0, false, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { rollStatus.clientID } } });

        // Dispose initiative if not enough players left to continue fight
        if (clientRollStatuses.Count <= 1)
        {
            DisposeInitiative();
        }
        else // Remove client roll status and display panel
        { 
            if (currentInitiativeIndex > clientRollStatuses.IndexOf(rollStatus))
                currentInitiativeIndex--;

            clientRollStatuses.Remove(rollStatus);
            Destroy(clientInitiativeDisplayPanels.Last().gameObject);
            clientInitiativeDisplayPanels.Remove(clientInitiativeDisplayPanels.Last());

            UpdateRollDisplays();
            ReloadClients();
        }
    }
    // Disable initiative for clients, destroy placeholder panel, reset indices and round counter, and clear display panels
    public void DisposeInitiative()
    {
        DisableInitiativeForClients();
        DestroyPlaceholderPanel();

        if (draggedObject != null)
        {
            initiativeScrollRect.enabled = true;
        }

        SetPositionIndicatorVisibility(false);
        ResetIndicesAndCounter();
        ClearInitiativeDisplayPanels();
        
        initiativeIsActive = false;

        if (infoPanel == null)
        {
            Debug.LogError("infoPanel is null.");
            return;
        }
        infoPanel.SetActive(true);
    }

    private void DisableInitiativeForClients()
    {
        ClientNetworkManager.Singleton.SetInitiativeDisplayClientRpc(
            clientNames, 0, 0, false, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } });
        clientIds.Clear();
        clientRollStatuses.Clear();
    }

    private void UpdateInitiativeForClients()
    {
        ClientNetworkManager.Singleton.SetInitiativeDisplayClientRpc(
            clientNames, currentInitiativeIndex, roundCounter, true, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } });
    }

    private void DestroyPlaceholderPanel()
    {
        if (placeholderPanelInstance != null)
        {
            Destroy(placeholderPanelInstance);
            placeholderPanelInstance = null;
        }
    }

    private void SetPositionIndicatorVisibility(bool isVisible)
    {
        posIndicator.gameObject.SetActive(isVisible);
        isDragging = isVisible;
    }

    private void ResetIndicesAndCounter()
    {
        currentIndex = -1;
        currentInitiativeIndex = -1;
        roundCounter = 1;
        if (roundCounterText == null)
        {
            Debug.LogError("roundCounterText is null.");
            return;
        }
        roundCounterText.SetText("R");
    }

    private void ClearInitiativeDisplayPanels()
    {
        foreach (var item in clientInitiativeDisplayPanels)
        {
            Destroy(item.gameObject);
        }
        clientInitiativeDisplayPanels.Clear();
    }

    public void UpdateRollDisplays() {

        // Ensure that both lists have the same size
        if (clientInitiativeDisplayPanels.Count != clientRollStatuses.Count)
        {
            Debug.LogError("Display panels and roll statuses have different sizes.");
            return;
        }
        // Update each initiative display panel
        for (int i = 0; i < clientInitiativeDisplayPanels.Count; i++)
        {
            clientInitiativeDisplayPanels[i].SetDisplay(clientRollStatuses[i]);
            if(initiativeIsActive)
                clientInitiativeDisplayPanels[i].SetActive(i == currentInitiativeIndex);
        }
    }
    //Sort and update RollDisplays (Dont sort them if initiative has already started unless forced to, always update displays)
    public void SortRollDisplays(bool forceSort=false)
    {
        if (initiativeIsActive && !forceSort) {
            UpdateRollDisplays();
            return;
        }

        clientRollStatuses = clientRollStatuses.OrderByDescending(s => s.finalRoll).ToList();
        UpdateRollDisplays();
    }
    // Move initiative to the next or previous position, start initiative
    public void MoveInitiative(int moveIndex) {
        // If there are clients and initiative is not active, activate initiative
        if (clientRollStatuses.Count > 0 && !initiativeIsActive)
        {
            initiativeIsActive = true;
            UpdateRoundCounterDisplay();
        }
        else if (clientRollStatuses.Count <= 0) return;

        // Update current initiative index and round counter
        currentInitiativeIndex += moveIndex;
        if (currentInitiativeIndex >= clientInitiativeDisplayPanels.Count) {
            roundCounter++;
            UpdateRoundCounterDisplay();
            currentInitiativeIndex = 0;
        } 
        else if (currentInitiativeIndex < 0) {
            roundCounter--;
            UpdateRoundCounterDisplay();
            currentInitiativeIndex = clientInitiativeDisplayPanels.Count - 1;
        } 

        UpdateInitiativeForClients();

        UpdateRollDisplays();
    }

    private void UpdateRoundCounterDisplay()
    {
        if (roundCounterText == null)
        {
            Debug.LogError("roundCounterText is null.");
            return;
        }
        roundCounterText.SetText(roundCounter.ToString());
    }
    // Handle start of click input action
    private void ClickStarted(InputAction.CallbackContext obj)
    {
        // Store the starting position for drag-and-drop
        startPointPos = mainControls.PrimaryMap.Pos.ReadValue<Vector2>();
    }
    // Handle cancellation of click input action
    private void ClickCanceled(InputAction.CallbackContext obj)
    {
        DestroyPlaceholderPanel();

        // Move the dragged object to the new position, if exists
        if (draggedObject != null) {
            SetNewPosition();
            draggedObject.SetDragged(false);
            draggedObject = null;
            initiativeScrollRect.enabled = true;
        }

        SetPositionIndicatorVisibility(false);
    }
    // Handle click-holding input action
    private void ClickPerformed(InputAction.CallbackContext obj)
    {
        //Get hover over panel

        // Check if the distance between the current position and the start point is within the hold dead zone
        if (Vector2.Distance(mainControls.PrimaryMap.Pos.ReadValue<Vector2>(), startPointPos) <= holdDeadZone) {
            // Perform a raycast to detect if any object is hit
            RaycastHit2D hit = Physics2D.Raycast(mainControls.PrimaryMap.Pos.ReadValue<Vector2>(), Vector2.zero);
            if(hit.collider != null) {
                initiativeScrollRect.enabled=false;

                // Get the ClientInitiativeDisplayPanel component from the hit object and set it as dragged
                draggedObject = hit.transform.GetComponent<ClientInitiativeDisplayPanel>();
                draggedObject.SetDragged(true);

                // Instantiate the placeholder panel at the current position
                placeholderPanelInstance = Instantiate(placeholderPanel, new Vector3(mainControls.PrimaryMap.Pos.ReadValue<Vector2>().x, mainControls.PrimaryMap.Pos.ReadValue<Vector2>().y), Quaternion.identity, canvas);
                // Set the controls for the placeholder panel to follow the input
                placeholderPanelInstance.GetComponent<PlaceholderFollowPosition>().SetControls(mainControls);

                SetPositionIndicatorVisibility(true);
            }
        }
    }
    // Set new position for dragged object
    public void SetNewPosition() {
        int oldIndex = clientInitiativeDisplayPanels.IndexOf(draggedObject);
        Debug.Log($" Moving from {oldIndex} to {currentIndex}");

        // Remove the status from its old position
        MyDataTypes.ClientRollStatus status = clientRollStatuses.ElementAt(oldIndex);
        clientRollStatuses.Remove(status);

        // Determine the target index for the status
        int targetIndex = oldIndex > currentIndex ? currentIndex : currentIndex - 1;

        // Move the status to its new position
        clientRollStatuses.Insert(targetIndex, status);

        // Adjust the currentInitiativeIndex if necessary and then update the roll displays
        if (currentIndex - 1 >= currentInitiativeIndex && oldIndex < currentInitiativeIndex)
            MoveInitiative(-1);
        else if (currentIndex - 1 <= currentInitiativeIndex && oldIndex > currentInitiativeIndex)
            MoveInitiative(1);
        else
            UpdateRollDisplays();
    }
    // Update hover index during dragging
    private void Update()
    {
        if(!isDragging) { return; }

        RaycastHit2D hit = Physics2D.Raycast(mainControls.PrimaryMap.Pos.ReadValue<Vector2>(), Vector2.zero);
        if (hit.collider != null)
        {
            currentIndex = hit.transform.GetSiblingIndex();
            posIndicator.SetSiblingIndex(currentIndex+1);
        }
    }


    // Enable input controls on enable
    private void OnEnable()
    {
        if (mainControls != null)
            mainControls.PrimaryMap.Enable();
    }
    // Disable input controls on disable
    private void OnDisable()
    {
        if (mainControls != null)
            mainControls.PrimaryMap.Disable();
    }
    // Unsubscribe from events on destruction
    private void OnDestroy()
    {
        ServerNetworkManager.Singleton.OnClientSentRoll -= OnClientSentRoll;
    }

}
