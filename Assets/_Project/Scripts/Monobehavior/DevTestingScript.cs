using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevTestingScript : MonoBehaviour
{
    [SerializeField] Transform _playerTransform;
    [SerializeField] CharacterController _characterController;
    [SerializeField] List<Vector3> _teleportablePositions;
    void Start()
    {
#if !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }

    public void TeleportTo(int index)
    {
        _characterController.enabled = false;
        _playerTransform.transform.position= _teleportablePositions[index];
        _characterController.enabled = true;
    }
}
