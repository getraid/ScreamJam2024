using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour,ISaveable
{
    [Header("References")]
    [SerializeField] Volume volume;
    [SerializeField] InhalerUI inhalerUI;
    [SerializeField] private Transform fatigueTransform;

    [SerializeField] CinemachineVirtualCamera _standingVM;
    [SerializeField] CinemachineVirtualCamera _crouchVM;
    [SerializeField] CinemachineVirtualCamera _fatigueVM;
    [SerializeField] CinemachineVirtualCamera _deadVM;

    [SerializeField] private Image blackMask;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float addedRunSpeed = 1.5f;
    [SerializeField] private Vector2 sprintAmplitudeRange;
    [SerializeField] private Vector2 sprintFrequencyRange;
    [SerializeField] private Vector2 sprintFOVRange;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float maxVerticalDelta = 1f; // Prevents skipping when moving down slopes

    [SerializeField] private float stamina = 10f;
    [SerializeField] private float jumpStaminaCost = 1f;
    [SerializeField] private float staminaStopTimeRequired = 1f;
    [SerializeField] private float staminaRecoveryTime = 1f;
    [SerializeField] private float maxFallDistance = 10f;

    [SerializeField] private float fatigueTime = 3f;
    [SerializeField] private float fatigueSpeed = 0.3f;

    [SerializeField] private bool hasAsthma = true;
    [SerializeField] private bool hasInhaler = false;
    [SerializeField] private float inhalerActivationThreshold = 0.3f;
    [SerializeField] private float inhalerStaminaRecoveryTime = 1f;
    [SerializeField] private float inhalerSpeed = 0.6f;
    [SerializeField] private float inhalerCooldown = 30f;
    
    [Tooltip("Disables the initial stuck on first quest. So you can run around and test stuff")]
    [SerializeField] bool _devMode;
    [SerializeField] AudioSource _steps;
    [SerializeField] int _pitchStepDivide = 5;

    public bool CanPlayerMove { get; set; }

    // Input
    private Vector2 _inputAxis;
    private bool _shiftDown;
    private bool _spacebarDown;
    private bool _qPressed;

    // State Information
    private bool _isGrounded;
    private Vector3 _velocity;
    private float _currentStamina;
    private float _currentStopTime;
    private bool _isFatigued;
    private bool _isInhaling;
    private float _currentInhalerCooldown;

    private CharacterController _controller;
    private Camera _camera;
    private CinemachineBasicMultiChannelPerlin _cameraNoise;
   

    Dictionary<DateTime, PlayerSaveData> _saveData = new Dictionary<DateTime, PlayerSaveData>();
    public struct PlayerSaveData
    {
        public bool HasInhaler;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool CanPlayerMove;
        public float Stamina;
    }
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        
        if(_devMode)
            StandUp();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _camera = Camera.main;

        _currentStamina = stamina;

        _cameraNoise = _standingVM.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        CameraPrioritiesOnGameLoad();
        StartCoroutine(AliveTransition(false));
    }

    public void Crouch()
    {
        CanPlayerMove = false;
        _standingVM.Priority = 0;
        _crouchVM.Priority = 10;
    }
    public void StandUp()
    {
        CanPlayerMove = true;
        _standingVM.Priority = 10;
        _crouchVM.Priority = 0;
    }
    public void PickupInhaler()
    {
        hasInhaler = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (!CanPlayerMove)
            return;

        // Gather Input
        _inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _shiftDown = Input.GetKey(KeyCode.LeftShift);
        _spacebarDown = Input.GetKey(KeyCode.Space);

        // Check Grounded
        _isGrounded = _controller.isGrounded;

        // Reset y-velocity if grounded
        float previous_y_velocity = _velocity.y;
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0f;
        }

        if (previous_y_velocity != _velocity.y && previous_y_velocity <= -maxFallDistance)
        {
            KillPlayer();
        }

        // Basic Move Direction - Camera Relative
        Vector3 move = new Vector3(_inputAxis.x, 0, _inputAxis.y);
        move = _camera.transform.forward * move.z + _camera.transform.right * move.x;
        move.Normalize();
        move.y = 0f;

        // Set Rotation to Camera Forward while staying upright
        if (CanPlayerMove)
        {
            transform.forward = new Vector3(_camera.transform.forward.x, 0f, _camera.transform.forward.z);
        }

        // Check for Movement
        float move_speed = (_inputAxis.magnitude >= 0.1f) ? moveSpeed : 0f;

        

        // Fatigue Camera Priority
        _fatigueVM.Priority = (_isFatigued) ? 100 : 0;

        // Fatigue / Speed
        if (_isFatigued)
        {
            SFXManager.Instance.PlaySFX(SFXManager.SFXType.HeavyBreathing_1,1);
            
            _currentStamina += Time.deltaTime * stamina / fatigueTime;
            move_speed *= fatigueSpeed;
            _currentStopTime = 0f;

            // Check for Inhaler
            if (hasInhaler && _currentInhalerCooldown <= inhalerActivationThreshold && _currentStamina < stamina)
            {
                _isInhaling = true;
                _currentInhalerCooldown = inhalerCooldown;
            }
        }
        else if (_isInhaling)
        {
            SFXManager.Instance.PlaySFX(SFXManager.SFXType.Inhaler, 1,true);

            _currentStamina += Time.deltaTime * stamina / inhalerStaminaRecoveryTime;
            move_speed *= inhalerSpeed;
            _currentStopTime = 0f;

            if (_currentStamina >= stamina)
            {
                _isInhaling = false;
            }
        }
        else if (_shiftDown)
        {
            // Toggle Off Journal
            JournalManager.Instance.ForceCloseJournal();

            if (hasAsthma)
            {
                _currentStamina -= Time.deltaTime;
            }

            if (move_speed > 0)
            {
                move_speed += addedRunSpeed;
            }
            
            _currentStopTime = 0f;
        }
        else
        {
            _currentStopTime += Time.deltaTime;

            if (_currentStopTime >= staminaStopTimeRequired)
            {
                _currentStamina += Time.deltaTime * stamina / staminaRecoveryTime;
            }
        }

        // Modify the Noise based on the Move Speed
        AdjustNoiseValues(move_speed);

        Vector3 move_delta = move_speed * Time.deltaTime * move;

        // Fix Skipping when moving down slopes
        if (_isGrounded && move_delta.magnitude > 0f)
        {
            // Raycast forward and down to check for slopes
            float ray_length = maxVerticalDelta + _controller.height * 0.5f;
            Vector3 move_direction = new Vector3(move_delta.x, 0f, move_delta.z);

            if (Physics.Raycast(transform.position + move_direction, Vector3.down, ray_length))
            {
                // Add the y difference to the move delta based on the hit point
                move_delta.y = -ray_length + _controller.height * 0.5f;
            }
        }

        _controller.Move(move_delta);

        // Check for Fatigue
        if (_currentStamina <= 0f)
        {
            _isFatigued = true;
        }

        // Check for Recovery
        if (_isFatigued && _currentStamina >= stamina)
        {
            _isFatigued = false;
        }

        // Volume - Post Processing
        // Adjust the vignette intensity based on the current stamina
        if(volume != null && volume.profile.TryGet(out Vignette vignette))
        {
            vignette.intensity.value = 1 - (_currentStamina / stamina);
        }

        // Jump/Gravity
        if (!_isFatigued && _spacebarDown && _isGrounded)
        {
            if (hasAsthma)
            {
                _currentStamina -= jumpStaminaCost;
            }

            _velocity.y += Mathf.Sqrt(jumpHeight * -3f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);

        _steps.pitch = move_speed/ _pitchStepDivide;

        // Clamp Stamina
        _currentStamina = Mathf.Clamp(_currentStamina, 0f, stamina);

        // Inhaler Cooldown
        if (_currentInhalerCooldown > 0f)
        {
            _currentInhalerCooldown -= Time.deltaTime;
        }

        // Update Inhaler UI
        if (inhalerUI != null)
        {
            inhalerUI.SetFillPercentage(_currentInhalerCooldown / inhalerCooldown);
        }
    }

    public float passed_noise_move_speed = 0f;

    private void AdjustNoiseValues(float move_speed)
    {
        passed_noise_move_speed = move_speed;

        float max_speed = moveSpeed + addedRunSpeed;
        float speed_percentage = move_speed / max_speed;

        float current_amplitude = _cameraNoise.m_AmplitudeGain;
        float current_frequency = _cameraNoise.m_FrequencyGain;
        
        float target_amplitude = Mathf.Lerp(sprintAmplitudeRange.x, sprintAmplitudeRange.y, speed_percentage);
        float target_frequency = Mathf.Lerp(sprintFrequencyRange.x, sprintFrequencyRange.y, speed_percentage);
        
        _cameraNoise.m_AmplitudeGain = Mathf.Lerp(current_amplitude, target_amplitude, Time.deltaTime);
        _cameraNoise.m_FrequencyGain = Mathf.Lerp(current_frequency, target_frequency, Time.deltaTime);

        // Modify the Standing VM POV
        // Only increase FOV when running
        if (move_speed <= moveSpeed)
            speed_percentage = 0f;
        else
            speed_percentage = (move_speed - moveSpeed) / (max_speed - moveSpeed);

        speed_percentage = Mathf.Clamp01(speed_percentage);
        float current_fov = _standingVM.m_Lens.FieldOfView;
        float target_fov = Mathf.Lerp(sprintFOVRange.x, sprintFOVRange.y, speed_percentage);
        _standingVM.m_Lens.FieldOfView = Mathf.Lerp(current_fov, target_fov, Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SafeZone"))
        {
            MusicManager.Instance.IsInSafeZone = true;
            MusicManager.Instance.StopMusic();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SafeZone"))
        {
            MusicManager.Instance.IsInSafeZone = false;
            MusicManager.Instance.StartMusic();
            
        }
    }

    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        PlayerSaveData data = _saveData[saveDateStamp];

        _controller.enabled = false;
        _currentStamina = data.Stamina;
        transform.position = data.Position;
        transform.rotation = data.Rotation;
        CanPlayerMove = data.CanPlayerMove;
        hasInhaler = data.HasInhaler;
        _controller.enabled = true;
    }

    public void SaveData(DateTime saveDateStamp)
    {
        PlayerSaveData data;
        data.Stamina = _currentStamina;
        data.Position = transform.position;
        data.Rotation=transform.rotation;
        data.CanPlayerMove = CanPlayerMove;
        data.HasInhaler = hasInhaler;

        _saveData.Add(saveDateStamp, data);
    }

    public void KillPlayer()
    {


        CanPlayerMove = false;
        _deadVM.Priority = 100;

        // Start Coroutine to Reload Scene
        StartCoroutine(DeathTransition());
    }

    IEnumerator DeathTransition()
    {
        CinemachineBasicMultiChannelPerlin camera_noise = _deadVM.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        float amplitude = 10f;
        float frequency = 10f;

        camera_noise.m_AmplitudeGain = amplitude;
        camera_noise.m_FrequencyGain = frequency;

        Color mask_color = Color.black;

        float death_time = 3f;
        float death_time_remaining = death_time;
        while (death_time_remaining > 0f)
        {
            death_time_remaining -= Time.deltaTime;
            float ratio = 1 - (death_time_remaining / death_time);

            camera_noise.m_AmplitudeGain = Mathf.Lerp(amplitude, 0f, ratio);
            camera_noise.m_FrequencyGain = Mathf.Lerp(frequency, 0f, ratio);

            if (volume != null && volume.profile.TryGet(out Vignette vignette))
            {
                vignette.intensity.value = ratio;
            }

            mask_color.a = ratio;
            blackMask.color = mask_color;

            yield return null;
        }

        SaveSystemManager.Instance.ReloadLastSave();
        ResetCameraPriorities();

        yield return new WaitForSeconds(1f);

        StartCoroutine(AliveTransition(true));
    }

    IEnumerator AliveTransition(bool allowMove)
    {
        float alive_time = 3f;
        float alive_time_remaining = alive_time;
        Color mask_color = Color.black;

        while (alive_time_remaining > 0f)
        {
            alive_time_remaining -= Time.deltaTime;
            float ratio = alive_time_remaining / alive_time;

            if (volume != null && volume.profile.TryGet(out Vignette vignette))
            {
                vignette.intensity.value = ratio;
            }

            mask_color.a = ratio;
            blackMask.color = mask_color;

            yield return null;
        }

        CanPlayerMove = allowMove;
    }

    public void SetAsthma()
    {
        hasAsthma = true;
    }

    private void CameraPrioritiesOnGameLoad()
    {
        _standingVM.Priority = 0;
        _crouchVM.Priority = 10;
        _fatigueVM.Priority = 0;
        _deadVM.Priority = 0;
    }
    private void ResetCameraPriorities()
    {
        _standingVM.Priority = 10;
        _crouchVM.Priority = 0;
        _fatigueVM.Priority = 0;
        _deadVM.Priority = 0;
    }
}
