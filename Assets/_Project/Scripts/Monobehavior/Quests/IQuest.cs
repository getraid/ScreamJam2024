using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IQuest
{
    public QuestSO QuestData { get; set; }
    public QuestItemSO QuestItemNeeded { get; set; }
    public QuestItemSO QuestItemToPickupOnCompletion { get; set; }
    public UnityEvent QuestActivated { get; set; }
    public UnityEvent QuestCompleted { get; set; }
    public Action<QuestSO> TryCompleteQuest { get; set; }
}
