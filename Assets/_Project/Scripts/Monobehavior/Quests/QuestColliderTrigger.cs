using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestColliderTrigger :MonoBehaviour, IQuest,ISaveable
{
    [field:SerializeField] public QuestSO QuestData { get; set; }
    [field: SerializeField] public QuestItemSO QuestItemNeeded { get; set; }
    [field: SerializeField] public UnityEvent QuestActivated { get; set; }
    [field: SerializeField] public UnityEvent QuestCompleted { get; set; }
    public Action<QuestSO> TryCompleteQuest { get; set; }

    List<bool> _saveActivationValues = new List<bool>();

    public void ReloadFromSafe(int saveIndex)
    {
        enabled = _saveActivationValues[saveIndex];
    }

    public void SaveData()
    {
        _saveActivationValues.Add(enabled);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCompleteQuest?.Invoke(QuestData);
    }
}
