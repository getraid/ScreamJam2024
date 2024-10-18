using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform interactorSource;
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private Image interactIcon;
    [SerializeField] private TextMeshProUGUI interactText;

    private bool _isInteract;

    private Interactable _targetInteractable;
    private Transform _targetParent;

    private void Update()
    {
        // Reset
        _targetInteractable = null;

        // Get Input
        _isInteract = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0);

        // Raycast
        Ray r = new Ray(interactorSource.position, interactorSource.forward);
        if (Physics.Raycast(r, out RaycastHit hitInfo, interactDistance))
        {
            bool is_hit = hitInfo.collider.TryGetComponent(out _targetInteractable);

            // Check if Script is active
            if (_targetInteractable != null && !_targetInteractable.enabled)
                _targetInteractable = null;

            _targetParent = is_hit ? hitInfo.collider.transform.parent : null;
        }

        SetIconActive(_targetInteractable != null);
        SetText(_targetInteractable != null ? _targetParent.name : "");

        if (_isInteract)
        {
            _targetInteractable?.Interact();
        }
    }

    private void SetIconActive(bool isActive)
    {
        if (interactIcon == null) return;

        interactIcon.enabled = isActive;
    }

    private void SetText(string text)
    {
        if (interactText == null) return;

        interactText.text = text;
    }
}
