using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestColliderTrigger :MonoBehaviour, IQuest
{
    [field:SerializeField] public QuestSO QuestData { get; set; }
    [field: SerializeField] public UnityEvent QuestActivated { get; set; }
    [field: SerializeField] public UnityEvent QuestCompleted { get; set; }
    public Action<QuestSO> TryCompleteQuest { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        TryCompleteQuest?.Invoke(QuestData);
    }
}
