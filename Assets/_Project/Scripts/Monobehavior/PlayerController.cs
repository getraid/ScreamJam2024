using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Volume volume;
    [SerializeField] InhalerUI inhalerUI;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float addedRunSpeed = 1.5f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private float stamina = 10f;
    [SerializeField] private float jumpStaminaCost = 1f;
    [SerializeField] private float staminaStopTimeRequired = 1f;
    [SerializeField] private float staminaRecoveryTime = 1f;

    [SerializeField] private float fatigueTime = 3f;
    [SerializeField] private float fatigueSpeed = 0.3f;

    [SerializeField] private bool hasInhaler = true;
    [SerializeField] private float inhalerStaminaRecoveryTime = 1f;
    [SerializeField] private float inhalerSpeed = 0.6f;
    [SerializeField] private float inhalerCooldown = 30f;

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

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _camera = Camera.main;

        _currentStamina = stamina;
    }

    private void Update()
    {
        // Gather Input
        _inputAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _shiftDown = Input.GetKey(KeyCode.LeftShift);
        _spacebarDown = Input.GetKey(KeyCode.Space);
        _qPressed = Input.GetKeyDown(KeyCode.Q);

        // Check Grounded
        _isGrounded = _controller.isGrounded;

        // Reset y-velocity if grounded
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0f;
        }

        // Basic Move Direction - Camera Relative
        Vector3 move = new Vector3(_inputAxis.x, 0, _inputAxis.y);
        move = _camera.transform.forward * move.z + _camera.transform.right * move.x;
        move.Normalize();
        move.y = 0f;

        // Check for Movement
        float move_speed = (_inputAxis.magnitude >= 0.1f) ? moveSpeed : 0f;

        // Check for Inhaler
        if (_qPressed && hasInhaler && _currentInhalerCooldown <= 0f && _currentStamina < stamina)
        {
            _isInhaling = true;
            _currentInhalerCooldown = inhalerCooldown;
        }

        // Fatigue / Speed
        if (_isFatigued)
        {
            _currentStamina += Time.deltaTime * stamina / fatigueTime;
            move_speed *= fatigueSpeed;
            _currentStopTime = 0f;
        }
        else if (_isInhaling)
        {
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
            _currentStamina -= Time.deltaTime;
            move_speed += addedRunSpeed;
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

        _controller.Move(move_speed * Time.deltaTime * move);

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
            _currentStamina -= jumpStaminaCost;
            _velocity.y += Mathf.Sqrt(jumpHeight * -3f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);

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

    
}
