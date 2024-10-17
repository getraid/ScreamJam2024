using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable,IQuest
{
    [field:SerializeField] public QuestSO QuestData { get; set; }
    [field: SerializeField] public QuestItemSO QuestItemNeeded { get; set; }
    [field: SerializeField] public UnityEvent QuestActivated { get; set; }
    [field: SerializeField] public UnityEvent QuestCompleted { get; set; }
    public Action<QuestSO> TryCompleteQuest { get; set; }

    [SerializeField] QuestItemSO _questItemToPickUp;
    public void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
        TryCompleteQuest?.Invoke(QuestData);

        if(_questItemToPickUp != null)
            InventoryManager.Instance.AddQuestItemSO(_questItemToPickUp);
    }
}
