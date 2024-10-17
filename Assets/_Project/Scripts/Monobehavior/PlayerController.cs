using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float addedRunSpeed = 1.5f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float gravity = -9.81f;

    

    // Input
    private Vector2 _inputAxis;
    private bool _shiftDown;
    private bool _spacebarDown;

    // State Information
    private bool _isGrounded;
    private Vector3 _velocity;

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
    }

    private void Update()
    {
        // Gather Input
        _inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _shiftDown = Input.GetKey(KeyCode.LeftShift);
        _spacebarDown = Input.GetKey(KeyCode.Space);

        // Check Grounded
        _isGrounded = _controller.isGrounded;

        // Reset y-velocity if grounded
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0f;
        }

        Vector3 move = new Vector3(_inputAxis.x, 0, _inputAxis.y);
        move = _camera.transform.forward * move.z + _camera.transform.right * move.x;
        move.y = 0f;

        // Check for Movement
        if (_inputAxis.magnitude >= 0.1f)
        {
            float move_speed = moveSpeed + (_shiftDown ? addedRunSpeed : 0f);

            _controller.Move(move_speed * Time.deltaTime * move);
        }

        if (_spacebarDown && _isGrounded)
        {
            _velocity.y += Mathf.Sqrt(jumpHeight * -3f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    
}
