using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class JournalManager : MonoBehaviour
{
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public CinemachineBrain MainCamera { get; set; }
    [field: SerializeField] public SkinnedMeshRenderer JournalObj { get; set; }
    [field: SerializeField] public GameObject JournalUI { get; set; }
    [field: SerializeField] public RectTransform PlayerIcon { get; set; }
    [field: SerializeField] public Transform ActualPlayerPos { get; set; }
    
    [field: SerializeField] public PlayerController PlayerController { get; set; }
    
    [field: SerializeField] public Image TabImage { get; set; }
    [field: SerializeField] public List<Image> GoalImages { get; set; } = new List<Image>();
    [field: SerializeField] public Image CrossOverlay { get; set; }
    [field: SerializeField] public List<int> GoalOrderIndex { get; set; } = new List<int>();


    private static readonly int HasOpened = Animator.StringToHash("HasOpened");
    private static readonly int IsReady = Animator.StringToHash("IsReady");

    private bool isClosed = true;
    public bool HasJournalOpened => !isClosed;
    
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
    public bool IsJournalOpenAndAvailable => !isClosed && HasValidCam;
    
    // UI Navigation
    [Serializable]
    public enum NavigationEntry
    {
        Tasks = 0,
        Credits = 1,
        Settings =2,
        Exit =3
    }
    
    // selected setting index
    [field: SerializeField] public NavigationEntry SelectedSetting { get; set; } = NavigationEntry.Tasks;
    [field: SerializeField] public Image SelectedSettingsHighlight { get; set; }
    
        
    // change based on selected setting
    [field: SerializeField] public TMP_Text NavigationHeader { get; set; } 
    
    
    // Tasks Button + UI
    [field: SerializeField] public Image TasksButton { get; set; } 
    [field: SerializeField] public GameObject TasksUI { get; set; } 
    
    
    // Credits Button
    [field: SerializeField] public Image CreditsButton { get; set; } 
    [field: SerializeField] public GameObject CreditsUI { get; set; } 

    
    // Settings Button
    [field: SerializeField] public Image SettingButton { get; set; } 
    [field: SerializeField] public GameObject SettingsUI { get; set; } 
    
     [field: SerializeField] public Slider BrightnessSlider { get; set; } // Reference to the slider
     [field: SerializeField] public Volume PostProcessingVolume  { get; set; }
     
     [field: SerializeField] public Slider VolumeSlider {get;set;} // Reference to the volume slider
     [field: SerializeField] public AudioMixer AudioMixer {get;set;}

    [field: SerializeField] public Slider MouseSensitvitySlider { get; set; } // Reference to the volume slider
    [field: SerializeField] public CinemachineVirtualCamera MainVirtCam { get; set; }

    [SerializeField] Button _loadButton;
     
     private float currentBrightness = 0.5f; // Default brightness value
     private ColorAdjustments colorAdjustments;
    
    // Exit Button
    [field: SerializeField] public Image ExitButton { get; set; } 


    public void ChangeUI(int indx)
    {
        NavigationEntry index = (NavigationEntry)indx;
        
        if (index == NavigationEntry.Exit)
        {
            Application.Quit();
            return;
        }
        
        // Disable all UI first
        SettingsUI.SetActive(false);
        CreditsUI.SetActive(false);
        TasksUI.SetActive(false);

        if (index == NavigationEntry.Tasks)
        {
            // Enable UI
            TasksUI.SetActive(true);
            NavigationHeader.text = "Tasks";
            // Set Highlighter cursor
            SelectedSettingsHighlight.rectTransform.position = TasksButton.rectTransform.position;
        }
        else if (index == NavigationEntry.Credits)
        {
            CreditsUI.SetActive(true);
            NavigationHeader.text = "Credits";
            SelectedSettingsHighlight.rectTransform.position = CreditsButton.rectTransform.position;
        }
        else if (index == NavigationEntry.Settings)
        {
            SettingsUI.SetActive(true);
            NavigationHeader.text = "Settings";
            SelectedSettingsHighlight.rectTransform.position = SettingButton.rectTransform.position;
        }

    }
    
    // disabled icon -> for reload to save to true
    // method for enabling the saving as diskette
    
    
    
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
        
        
        
         // Get the Color Adjustments effect from the Volume
        if (PostProcessingVolume.profile.TryGet(out colorAdjustments))
        {
            this.colorAdjustments = colorAdjustments;
            
            // Set the slider's value to the current post exposure
            BrightnessSlider.value = colorAdjustments.postExposure.value+0.5f;
            
            // Add listener for slider value changes
            BrightnessSlider.onValueChanged.AddListener(SetBrightness);
        }
        
        float currentVolume;
        // AudioMixer.outputAudioMixerGroup.audioMixer.GetFloat("Master",out currentVolume);
        // AudioMixer.GetFloat("Master", out currentVolume);

        // Convert dB to linear scale
        // VolumeSlider.value = Mathf.Pow(10, currentVolume / 20);
        VolumeSlider.value = 1f;
        VolumeSlider.onValueChanged.AddListener(SetVolume);

#if UNITY_EDITOR
        MouseSensitvitySlider.value = 1f;
#elif UNITY_WEBGL
        MouseSensitvitySlider.value = 0.5f;
        ChangeSensitvity(MouseSensitvitySlider.value);
#else
        MouseSensitvitySlider.value = 1f;
#endif

        MouseSensitvitySlider.onValueChanged.AddListener(ChangeSensitvity);



        DontDestroyOnLoad(gameObject);
    }

    private void ChangeSensitvity(float arg0)
    {
        if(arg0 < 0.01f) 
        {
            arg0 = 0.01f;
        }

        SavedMouseSpeed = arg0 * 300f;
    }

    public void SetVolume(float volume)
      {
          float log_value = (volume <= 0f) ? -120f : Mathf.Log10(volume) * 20;

          AudioMixer.SetFloat("MainVolume", log_value);
          
       
      }
      
      public void SetBrightness(float brightness)
        {
            if (colorAdjustments != null)
            {
                var newbrightness = (Mathf.Clamp01(brightness) -0.5f) * 2f;
                colorAdjustments.postExposure.value = newbrightness;
            }
        }
    
    void Start()
    {
        JournalObj.enabled = false; // Make sure the journal is initially hidden
        JournalUI.SetActive(false);
        MainCamera.m_CameraActivatedEvent.AddListener(OnCameraSwitched); // Listen for camera changes
        CrossOverlay?.gameObject.SetActive(false); // Initially hide the cross overlay

        // Hide all goal images initially
        foreach (var goal in GoalImages)
        {
            goal.gameObject.SetActive(false);
        }
    }

    public void EnableCheckpointLoad()
    {
        _loadButton.interactable = true;
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
        ResetNavigation();
        
        // disable mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // PlayerController.CanPlayerMove = false;
        
        isReady = true;
        isClosed = false;
        StartAnimation(isClosed); // Pass false to indicate the journal is opening
        JournalObj.enabled = true; // Enable the mesh when opening
        JournalUI.SetActive(true);
        isClosingAnimationPlaying = false;
    }

    private void CloseJournal()
    {
        // reenabel mosue
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // TODO: Enable current player look dir?
        // PlayerController.CanPlayerMove = true;
        
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

    public float SavedMouseSpeed { get; internal set; } = 300f;

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
        ChangeUI(0);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ToggleJournal();
        }

        // Update the player icon position on the map
        UpdatePlayerIconPosition();

        TabImage.enabled = HasValidCam;

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

                
                //TODO: Maybe the overlay should only be displayed after a certain while, if the player doesn't know what to do
                
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
        if (CrossOverlay == null) return;
        
        // Move the cross overlay to the specified goal's position
        CrossOverlay.rectTransform.position = GoalImages[goalIndex].rectTransform.position;
        CrossOverlay.gameObject.SetActive(true);
    }

    public void ForceCloseJournal()
    {
        CloseJournal();
    }
}