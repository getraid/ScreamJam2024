using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum MonsterState
{
    Idle,
    SinkVanish,
    Lurk,

    Hide,
    Roam,
    Chase,
}

public class MonsterAI : MonoBehaviour
{
    [SerializeField] private bool destroyOnStateChange = false;

    [SerializeField] Renderer monsterRenderer;
    [SerializeField] private LayerMask monsterMask;
    [SerializeField] private LayerMask terrainMask;

    [SerializeField] private MonsterState currentState;


    [SerializeField] bool canApproachCampfire = false;

    [SerializeField] private float monsterHeight = 2f;

    [SerializeField] private float roamSpeed = 3f;
    [SerializeField] private float roamRadius = 15f;
    [SerializeField] private float roamRadiusMax = 25f;
    [SerializeField] private float teleportThreshold = 30f;
    [SerializeField] private float roamIdleDistance = 30f;

    [SerializeField] private float lurkDistance = 25f;

    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float killDistance = 3f;
    [SerializeField] private Vector2 hideTimeRange = new Vector2(5f, 10f);

    [SerializeField] private float raycastDistance = 50f;

    private Transform _player;
    private Transform _campfire;
    private Camera _camera;

    private float _roamFlipTimer = 0f;
    private bool _roamClockwise;
    private float _hideTimer;
    private bool _lockedState;
    private float _lurkAngle;

    private void OnEnable()
    {
        _player = GameObject.FindWithTag("Player").transform;
        _campfire = GameObject.FindWithTag("Campfire").transform;
        _camera = Camera.main;
    }

    public bool isVisible;

    private void Update()
    {
        // Turn towards the player
        transform.forward = (_player.position - transform.position).normalized;

        bool is_visible = IsObjectBeingRendered(monsterRenderer) && IsMonsterInCameraView();
        isVisible = is_visible;

        if (_lockedState)
            return;

        float player_distance_from_campfire = Vector3.Distance(_player.position, _campfire.position);
        if (player_distance_from_campfire <= 50 && !canApproachCampfire)
        {
            SetState(MonsterState.Hide);
            _hideTimer = Random.Range(hideTimeRange.x, hideTimeRange.y);
            return;
        }

        switch (currentState)
        {
            case MonsterState.Idle:
                Idle();
                break;
            case MonsterState.SinkVanish:
                StartCoroutine(SinkVanish());
                break;
            case MonsterState.Lurk:
                Lurk();
                break;
            case MonsterState.Hide:
                Hide();
                break;
            case MonsterState.Roam:
                Roam();
                break;
            case MonsterState.Chase:
                Chase();
                break;
        }
    }

    private void Idle()
    {
        transform.position = Vector3.zero;
    }

    private IEnumerator SinkVanish()
    {
        _lockedState = true;
        
        float delay_timer = 5f;
        while (delay_timer > 0f)
        {
            delay_timer -= Time.deltaTime;

            // If the monster is visible, immediately break this while loop
            if (isVisible)
            {
                delay_timer = 0f;
            }

            yield return null;
        }

        // Move the model away from the player
        float disappear_timer = 2f;
        float disappear_speed = 30f;

        while (disappear_timer > 0f)
        {
            disappear_timer -= Time.deltaTime;
            Vector3 move_direction = (transform.position - _player.position).normalized;
            Vector3 next_position = transform.position + move_direction * disappear_speed * Time.deltaTime;
            //next_position.y = GetRaycastYPosition(next_position);

            transform.position = next_position;
            yield return null;
        }

        _lockedState = false;
        SetState(MonsterState.Idle);
    }

    private void Lurk()
    {
        // Random Low Chance to Change Target Angle
        if (Random.value < 0.01f)
        {
            _lurkAngle = Random.Range(0f, 360f);
        }

        float lurk_distance = lurkDistance;
        if (isVisible)
        {
            lurk_distance *= 2f;
        }

        // Get the angle between the monster and the player.
        float current_angle = Vector3.SignedAngle(Vector3.forward, transform.position - _player.position, Vector3.up);
        float next_angle = Mathf.MoveTowardsAngle(current_angle, _lurkAngle, 1f);

        // Use the next angle to get the next position.
        Vector3 next_position = _player.position + new Vector3(Mathf.Sin(next_angle * Mathf.Deg2Rad) * lurk_distance, 0, Mathf.Cos(next_angle * Mathf.Deg2Rad) * lurk_distance);

        // Increase the speed when the player is close to the monster, proportional to the distance.
        float distance_to_desired_position = Vector3.Distance(transform.position, next_position);
        float move_speed = Mathf.Lerp(roamSpeed, roamSpeed * 5f, Mathf.Clamp01(distance_to_desired_position / 10f));

        next_position = Vector3.MoveTowards(transform.position, next_position, move_speed * Time.deltaTime);
        next_position.y = GetRaycastYPosition(next_position);

        // Move the monster towards the desired position
        transform.position = next_position;
    }

    private void Hide()
    {
        _hideTimer -= Time.deltaTime;

        if (_hideTimer <= 0f && !isVisible)
        {
            SetState(MonsterState.Roam);
        }

        float distance = Vector3.Distance(transform.position, _player.position);

        if (distance > teleportThreshold)
            return;

        // Move away from player
        Vector3 move_direction = (transform.position - _player.position).normalized;
        Vector3 next_position = transform.position + move_direction * roamSpeed * 2f * Time.deltaTime;
        next_position.y = GetRaycastYPosition(next_position);

        transform.position = next_position;
    }

    private void Roam()
    {
        if (_player == null)
        {
            SetState(MonsterState.Idle);
            return;
        }
        
        // Get Player Position
        Vector3 player_position = _player.position;

        // If the distance between the player and the monster is greater than the twice the roam radius
        // then teleport the monster.

        // Rotate the _desiredAngle behind the player.
        float desired_angle = CalculateAngleBehindPlayer(_player.forward);
        float desired_distance = CalculatedDesiredDistanceFromPlayer(player_position, _player.forward, transform.position);

        float distance = Vector3.Distance(transform.position, player_position);

        // Teleport behind the player.
        if (distance > teleportThreshold)
        {
            Teleport(player_position, desired_angle);
            return;
        }

        // If visible, then the monster is spotted by the player.
        if (isVisible)
        {
            Spotted(player_position);
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

    private void Chase()
    {
        // Run towards the player.
        Vector3 move_direction = (_player.position - transform.position).normalized;
        Vector3 next_position = transform.position + move_direction * chaseSpeed * Time.deltaTime;
        next_position.y = GetRaycastYPosition(next_position);
        transform.position = next_position;

        float distance = Vector3.Distance(transform.position, _player.position);
        if (distance < killDistance)
        {
            SetState(MonsterState.Idle);
            _player.SendMessage("KillPlayer", SendMessageOptions.DontRequireReceiver);
        }
    }

    private float _spottedTime = 0f;

    // When spotted by the player, the monster will stand still.
    // If the player approached, it will move away quickly.
    private void Spotted(Vector3 playerPosition)
    {
        _spottedTime += Time.deltaTime;

        if (_spottedTime > 3f)
        {
            SetState(MonsterState.Hide);
            _hideTimer = Random.Range(hideTimeRange.x, hideTimeRange.y);
            _spottedTime = 0f;
        }

        float distance = Vector3.Distance(transform.position, playerPosition);
        if (distance > roamIdleDistance)
        {
            return;
        }
        
        // Move away from the player.
        Vector3 move_direction = (transform.position - playerPosition).normalized;
        Vector3 next_position = transform.position + move_direction * roamSpeed * 2f * Time.deltaTime;
        next_position.y = GetRaycastYPosition(next_position);

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

        float dotRes = Vector3.Dot(_camera.transform.forward, (transform.position - _player.transform.position).normalized);
        return dotRes > 0.6f;
            
       // Vector3 screen_position = _camera.WorldToScreenPoint(transform.position);
       // return screen_position.x > 0 && screen_position.x < Screen.width && screen_position.y > 0 && screen_position.y < Screen.height;
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

    public void SetVanish()
    {
        currentState = MonsterState.SinkVanish;
    }

    public void SetState(MonsterState state)
    {
        if (destroyOnStateChange)
        {
            Destroy(gameObject);
            return;
        }

        currentState = state;
    }
}
