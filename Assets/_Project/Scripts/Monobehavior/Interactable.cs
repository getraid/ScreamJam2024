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
    Dictionary<DateTime,(bool Enabled,bool GameObjectActive)> _saveActivationValues = new Dictionary<DateTime, (bool Enabled, bool GameObjectActive)>();

    public Action<QuestSO> TryCompleteQuest { get; set; }

    [SerializeField] QuestItemSO _questItemToPickUp;
    [SerializeField] VoiceLineDataSO _voiceLineToActivate;
    [SerializeField] bool _activateWalkieTalkieDuringVoiceLine = false;
    [SerializeField] UnityEvent _onVoiceLineCompleted;

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
            VoiceLineManager.Instance.PlayVoiceLine(_voiceLineToActivate, _activateWalkieTalkieDuringVoiceLine, _onVoiceLineCompleted);
    }

    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_saveActivationValues.ContainsKey(saveDateStamp))
        {
            enabled = _saveActivationValues[saveDateStamp].Enabled;
            gameObject.SetActive(_saveActivationValues[saveDateStamp].GameObjectActive);
        }
        else
            Destroy(gameObject);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _saveActivationValues.Add(saveDateStamp, new(enabled,gameObject.activeSelf));
    }
}
