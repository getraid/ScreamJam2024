using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
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
        RaycastHit[] hits = Physics.RaycastAll(r, interactDistance);
        if(hits.Length > 0 )
        {
            for(int i=0;i<hits.Length; i++)
            {
                Interactable[] interactables = hits[i].collider.gameObject.GetComponents<Interactable>();

                Interactable found = interactables.FirstOrDefault(x => x.enabled);

                if(found!=null)
                {
                    _targetInteractable = found;

                    _targetParent = found.transform.parent;


                    if (_isInteract)
                    {
                        _targetInteractable?.Interact();
                    }
                    break;
                }
                
            }
        }
        SetIconActive(_targetInteractable != null);
        SetText(_targetInteractable != null ? _targetParent.name : "");

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
