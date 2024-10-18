using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IQuest
{
    [field:SerializeField] public QuestSO QuestData { get; set; }
    [field: SerializeField] public QuestItemSO QuestItemNeeded { get; set; }
    [field: SerializeField] public UnityEvent QuestActivated { get; set; }
    [field: SerializeField] public UnityEvent QuestCompleted { get; set; }
    [SerializeField] bool _disableOnQuestCompletion;
    public Action<QuestSO> TryCompleteQuest { get; set; }

    [SerializeField] QuestItemSO _questItemToPickUp;

    private void Start()
    {
        if (_disableOnQuestCompletion)
            QuestCompleted.AddListener(() => { enabled = false; });
    }
    public void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
        TryCompleteQuest?.Invoke(QuestData);

        if (_questItemToPickUp != null)
        {
            InventoryManager.Instance.AddQuestItemSO(_questItemToPickUp);
            Destroy(gameObject);
        }
    }
}
