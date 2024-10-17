using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRandomly : MonoBehaviour
{
    [SerializeField] private float range = 1f;
    [SerializeField] private float speed = 1f;
    
    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    void Start()
    {
        _startPosition = transform.localPosition;
    }

    void Update()
    {
        if (Vector3.Distance(transform.localPosition, _targetPosition) < 0.1f)
        {
            SetNewTargetPosition();
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPosition, speed * Time.deltaTime);
    }

    private void SetNewTargetPosition()
    {
        _targetPosition = _startPosition + Random.insideUnitSphere * range;
    }
}
