using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IQuest,ISaveable
{
    [field: SerializeField] public QuestSO QuestData { get; set; }
    [field: SerializeField] public QuestItemSO QuestItemNeeded { get; set; }
    [field: SerializeField] public UnityEvent QuestActivated { get; set; }
    [field: SerializeField] public UnityEvent QuestCompleted { get; set; }
    [SerializeField] bool _disableOnQuestCompletion;
    List<(bool Enabled,bool GameObjectActive)> _saveActivationValues = new List<(bool, bool)>();

    public Action<QuestSO> TryCompleteQuest { get; set; }

    [SerializeField] QuestItemSO _questItemToPickUp;
    [SerializeField] VoiceLineDataSO _voiceLineToActivate;

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
        }
        if(_voiceLineToActivate != null)
            VoiceLineManager.Instance.PlayVoiceLine(_voiceLineToActivate);
    }

    public void ReloadFromSafe(int saveIndex)
    {
        enabled = _saveActivationValues[saveIndex].Enabled;
        gameObject.SetActive(_saveActivationValues[saveIndex].GameObjectActive);
    }

    public void SaveData()
    {
        _saveActivationValues.Add(new(enabled,gameObject.activeSelf));
    }
}
