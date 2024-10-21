using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class JournalManager : MonoBehaviour
{
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public CinemachineBrain MainCamera { get; set; }
    [field: SerializeField] public SkinnedMeshRenderer JournalObj { get; set; }
    [field: SerializeField] public GameObject JournalUI { get; set; }

    
    
    private static readonly int HasOpened = Animator.StringToHash("HasOpened");
    private static readonly int IsReady = Animator.StringToHash("IsReady");
    
    

    private bool isClosed = true;
    private bool isReady = false;
    private bool isClosingAnimationPlaying = false;

    void Start()
    {
        JournalObj.enabled = false; // Make sure the journal is initially hidden
        JournalUI.SetActive(false);
        MainCamera.m_CameraActivatedEvent.AddListener(OnCameraSwitched); // Listen for camera changes
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
}