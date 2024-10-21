using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class JournalManager : MonoBehaviour
{
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public CinemachineBrain MainCamera { get; set; }
    [field: SerializeField] public SkinnedMeshRenderer JournalObj { get; set; }
    [field: SerializeField] public GameObject JournalUI { get; set; }
    [field: SerializeField] public RectTransform PlayerIcon { get; set; }
    [field: SerializeField] public Transform ActualPlayerPos { get; set; }

    [field: SerializeField] public List<Image> GoalImages { get; set; } = new List<Image>();
    [field: SerializeField] public Image CrossOverlay { get; set; }
    [field: SerializeField] public List<int> GoalOrderIndex { get; set; } = new List<int>();

    private static readonly int HasOpened = Animator.StringToHash("HasOpened");
    private static readonly int IsReady = Animator.StringToHash("IsReady");

    private bool isClosed = true;
    private bool isReady = false;
    private bool isClosingAnimationPlaying = false;

    // World map boundaries
    private Vector3 worldMin = new Vector3(0, 0, 0); // Bottom-left corner in world space
    private Vector3 worldMax = new Vector3(504, 0, 498); // Top-right corner in world space

    // Map UI boundaries (relative to anchors)
    private Vector2 uiMin = new Vector2(-0.334f, 0.227f); // Corresponding to worldMin
    private Vector2 uiMax = new Vector2(0.259f, -0.285f); // Corresponding to worldMax

    private int currentGoalIndex = -1;
    
    public static JournalManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        JournalObj.enabled = false; // Make sure the journal is initially hidden
        JournalUI.SetActive(false);
        MainCamera.m_CameraActivatedEvent.AddListener(OnCameraSwitched); // Listen for camera changes
        CrossOverlay.gameObject.SetActive(false); // Initially hide the cross overlay

        // Hide all goal images initially
        foreach (var goal in GoalImages)
        {
            goal.gameObject.SetActive(false);
        }
    }

    public void ToggleJournal()
    {
        // Check if the camera is the valid one before toggling
        if (!HasValidCam)
        {
            Debug.Log("Invalid camera, cannot open journal.");
            return;
        }

        if (isClosed)
        {
            OpenJournal();
        }
        else
        {
            CloseJournal();
        }
    }

    private void OpenJournal()
    {
        isReady = true;
        isClosed = false;
        StartAnimation(isClosed); // Pass false to indicate the journal is opening
        JournalObj.enabled = true; // Enable the mesh when opening
        JournalUI.SetActive(true);
        isClosingAnimationPlaying = false;
    }

    private void CloseJournal()
    {
        isReady = true;
        isClosed = true;
        StartAnimation(isClosed); // Pass true to indicate the journal is closing
        isClosingAnimationPlaying = true; // Mark that the closing animation is in progress
    }

    private void StartAnimation(bool isClosing)
    {
        Animator.SetBool(HasOpened, !isClosing);
        Animator.SetBool(IsReady, true);
    }

    private bool HasValidCam
    {
        get
        {
            // Check if the active virtual camera is the one tagged as "MainVirtCam"
            return MainCamera.ActiveVirtualCamera != null &&
                   MainCamera.ActiveVirtualCamera.VirtualCameraGameObject.CompareTag("MainVirtCam");
        }
    }

    private void OnCameraSwitched(ICinemachineCamera fromCam, ICinemachineCamera toCam)
    {
        // Close the journal if the new active camera is not valid
        if (!HasValidCam && !isClosed)
        {
            CloseJournal();
            Debug.Log("Journal closed due to camera switch.");
        }
    }

    private void ResetNavigation()
    {
        // Implement your reset navigation logic here
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ToggleJournal();
        }

        // Update the player icon position on the map
        UpdatePlayerIconPosition();

        // Check if the closing animation is playing and if it has finished
        if (isClosingAnimationPlaying && isClosed)
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1f && stateInfo.IsName("CloseAnimation"))
            {
                // The closing animation has finished, so disable the journal objects
                JournalObj.enabled = false;
                JournalUI.SetActive(false);
                isClosingAnimationPlaying = false; // Reset the flag
            }
        }
    }

    private void UpdatePlayerIconPosition()
    {
        // Get the player's world position
        Vector3 playerWorldPos = GetPlayerWorldPosition();

        // Normalize the player's position within the world boundaries
        float normalizedX = Mathf.InverseLerp(worldMin.x, worldMax.x, playerWorldPos.x);
        float normalizedY = Mathf.InverseLerp(worldMin.z, worldMax.z, playerWorldPos.z);

        // Interpolate to find the position within the UI boundaries
        float uiPosX = Mathf.Lerp(uiMin.x, uiMax.x, normalizedX);
        float uiPosY = Mathf.Lerp(uiMin.y, uiMax.y, normalizedY);

        // Update the player icon's position on the map
        PlayerIcon.anchoredPosition = new Vector2(uiPosX, -uiPosY);
    }

    private Vector3 GetPlayerWorldPosition()
    {
        return ActualPlayerPos.position; 
    }

    // Function to unlock the next goal based on the GoalOrderIndex list
    public void NextGoal()
    {
        // Increment the current goal index
        currentGoalIndex++;

        // Make sure the current index is within the GoalOrderIndex range
        if (currentGoalIndex >= 0 && currentGoalIndex < GoalOrderIndex.Count)
        {
            int goalIndex = GoalOrderIndex[currentGoalIndex];

            // Ensure the goal index is within the bounds of the GoalImages list
            if (goalIndex >= 0 && goalIndex < GoalImages.Count)
            {
                // Enable the goal image
                GoalImages[goalIndex].gameObject.SetActive(true);

                // Move the cross overlay to this goal
                MoveCrossOverlayToGoal(goalIndex);
            }
            else
            {
                Debug.LogWarning("The specified goal index is out of range.");
            }
        }
        else
        {
            Debug.LogWarning("No more goals in the GoalOrderIndex list.");
        }
    }

    // Function to move the cross overlay to a specified goal
    private void MoveCrossOverlayToGoal(int goalIndex)
    {
        // Move the cross overlay to the specified goal's position
        CrossOverlay.rectTransform.position = GoalImages[goalIndex].rectTransform.position;
        CrossOverlay.gameObject.SetActive(true);
    }
}