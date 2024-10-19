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

    Dictionary<DateTime, bool> _saveActivationValues = new Dictionary<DateTime, bool>();

    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_saveActivationValues.ContainsKey(saveDateStamp))
            enabled = _saveActivationValues[saveDateStamp];
        else
            Destroy(gameObject);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _saveActivationValues.Add(saveDateStamp, enabled);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCompleteQuest?.Invoke(QuestData);
    }
}
