using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum MonsterState
{
    Idle,
    Roam,
}

public class MonsterAI : MonoBehaviour
{
    [SerializeField] Renderer monsterRenderer;
    [SerializeField] private LayerMask monsterMask;
    [SerializeField] private LayerMask terrainMask;

    [SerializeField] private MonsterState currentState;

    [SerializeField] private float monsterHeight = 2f;

    [SerializeField] private float roamSpeed = 3f;
    [SerializeField] private float roamRadius = 15f;
    [SerializeField] private float roamRadiusMax = 25f;
    [SerializeField] private float teleportThreshold = 30f;
    [SerializeField] private float roamIdleDistance = 30f;

    [SerializeField] private float raycastDistance = 50f;

    private Transform _player;
    private Camera _camera;

    private float _roamFlipTimer = 0f;
    private bool _roamClockwise;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player").transform;
        _camera = Camera.main;
    }

    public bool isVisible;

    private void Update()
    {
        bool is_visible = IsObjectBeingRendered(monsterRenderer) && IsMonsterInCameraView() && IsRaycastSuccess();
        isVisible = is_visible;

        switch (currentState)
        {
            case MonsterState.Idle:
                Idle();
                break;
            case MonsterState.Roam:
                Roam();
                break;
        }
    }

    private void Idle()
    {
        transform.position = Vector3.zero;
    }

    public float desiredAngle = 0f;
    public float desiredDistance = 0f;

    private void Roam()
    {
        if (_player == null)
        {
            currentState = MonsterState.Idle;
            return;
        }
        
        // Get Player Position
        Vector3 player_position = _player.position;

        // If the distance between the player and the monster is greater than the twice the roam radius
        // then teleport the monster.

        // Rotate the _desiredAngle behind the player.
        float desired_angle = CalculateAngleBehindPlayer(_player.forward);
        float desired_distance = CalculatedDesiredDistanceFromPlayer(player_position, _player.forward, transform.position);

        desiredAngle = desired_angle;
        desiredDistance = desired_distance;

        float distance = Vector3.Distance(transform.position, player_position);

        // Teleport behind the player.
        if (distance > teleportThreshold)
        {
            Teleport(player_position, desired_angle);
            return;
        }

        // Check the Roam Idle Distance
        if (distance > roamIdleDistance)
        {
            return;
        }

        // Check Roam Flip Timer
        _roamFlipTimer -= Time.deltaTime;
        if (_roamFlipTimer <= 0f)
        {
            _roamFlipTimer = Random.Range(1f, 7f);
            _roamClockwise = !_roamClockwise;
        }

        // Get the angle between the monster and the player.
        float current_angle = Vector3.SignedAngle(Vector3.forward, transform.position - player_position, Vector3.up);

        // Set roam direction
        float roamDirection = _roamClockwise ? 1f : -1f;
        float next_angle = Mathf.MoveTowardsAngle(current_angle, desired_angle, roamDirection);

        // Use the next angle to get the next position.
        Vector3 next_position = player_position + new Vector3(Mathf.Sin(next_angle * Mathf.Deg2Rad) * desired_distance, 0, Mathf.Cos(next_angle * Mathf.Deg2Rad) * desired_distance);
        
        // Increase the speed when the player is close to the monster, proportional to the distance.
        float roam_speed = Mathf.Lerp(roamSpeed, roamSpeed * 2f, Mathf.Clamp01(distance / 10f));

        next_position = Vector3.MoveTowards(transform.position, next_position, roam_speed * Time.deltaTime);
        next_position.y = GetRaycastYPosition(next_position);

        // Move the monster towards the desired position
        transform.position = next_position;
    }

    private void Teleport(Vector3 playerPosition, float desiredAngle)
    {
        float spawn_distance = roamRadiusMax;
        // Spawn the monster behind the player.
        Vector3 spawn_position = playerPosition + new Vector3(Mathf.Sin(desiredAngle * Mathf.Deg2Rad) * spawn_distance, 0, Mathf.Cos(desiredAngle * Mathf.Deg2Rad) * spawn_distance);
        spawn_position.y = GetRaycastYPosition(spawn_position);
        transform.position = spawn_position;
    }

    public float GetRaycastYPosition(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * raycastDistance / 2f, Vector3.down, out RaycastHit hit, raycastDistance, terrainMask))
        {
            return hit.point.y;
        }

        return 0f;
    }

    public float CalculateAngleBehindPlayer(Vector3 playerForward)
    {
        Vector3 behind = -playerForward;
        float angle = Vector3.SignedAngle(Vector3.forward, behind, Vector3.up);
        return (angle + 360f) % 360f;
    }

    // We are getting the desired distance from the player based on where the player is looking.
    public float CalculatedDesiredDistanceFromPlayer(Vector3 player_position, Vector3 player_forward, Vector3 monster_position)
    {
        // As the player looks towards the monster, the monster should be further away.
        // As the player looks away from the monster, the monster should be closer.
        
        Vector3 to_monster = monster_position - player_position;
        Vector3 forward_normalized = player_forward.normalized;
        float dot_product = Vector3.Dot(forward_normalized, to_monster.normalized);
        float t = Mathf.Clamp01((dot_product + 1) / 2f);

        return Mathf.Lerp(roamRadius, roamRadiusMax, t);
    }

    public bool IsMonsterInCameraView()
    {
        Vector3 screen_position = _camera.WorldToScreenPoint(transform.position);
        return screen_position.x > 0 && screen_position.x < Screen.width && screen_position.y > 0 && screen_position.y < Screen.height;
    }

    public bool IsObjectBeingRendered(Renderer obj)
    {
        return obj.isVisible;
    }

    private bool IsRaycastSuccess()
    {
        Ray ray = _camera.ScreenPointToRay(_camera.WorldToScreenPoint(transform.position + Vector3.up * monsterHeight / 2f));
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            // Check if the hit object belongs to the monster layer.
            if (((1 << hit.collider.gameObject.layer) & monsterMask) != 0)
                return true;
        }

        return false;
    }
}
