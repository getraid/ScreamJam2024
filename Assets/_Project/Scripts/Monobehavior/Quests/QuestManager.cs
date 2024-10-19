using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour,ISaveable
{
    [SerializeField] TMP_Text _uiQuestPrefab;
    [SerializeField] Transform _questUIParent;
    [SerializeField] List<ChronologicalQuests> _chronologicalQuests;

    [Serializable]
    public struct ChronologicalQuests
    {
        public QuestSO Quest;
        public float DelayBeforeShownInQuickTasks;
    }

    List<(IQuest SceneQuest, ChronologicalQuests QuestData,TMP_Text QuestUIText)> _allChronologicalQuests = new List<(IQuest, ChronologicalQuests, TMP_Text)>();
    int _lastCompletedQuest = 0;
    
    Dictionary<DateTime,int> _questSaveData= new Dictionary<DateTime, int>();
    void Start()
    {
        List<IQuest> allGameQuests = FindObjectsOfType<MonoBehaviour>(true).OfType<IQuest>().ToList();

        for(int i=0;i< _chronologicalQuests.Count;i++)
        {
            List<IQuest> sceneQuests= allGameQuests.FindAll(x => x.QuestData == _chronologicalQuests[i].Quest);

            if (sceneQuests.Count > 1)
                Debug.LogError($"Same quest is used on multiple places in game, it must be used only on one place [{_chronologicalQuests[i].Quest.name} ({String.Join(",",sceneQuests)})]!!!");
            else if (sceneQuests.Count == 0)
                Debug.LogError($"The quest is not inside the scene, but is in QuestManager [{_chronologicalQuests[i].Quest.name} ]!!!");
            else
            {
                TMP_Text tmpText = Instantiate(_uiQuestPrefab, _questUIParent);
                tmpText.text = _chronologicalQuests[i].Quest.QuestText;
                _allChronologicalQuests.Add(new(sceneQuests.First(), _chronologicalQuests[i], tmpText));
            }
        }
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest += OnQuestCompletion;
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestActivated?.Invoke();

        StartCoroutine(EnableWithDelay(_allChronologicalQuests[_lastCompletedQuest].QuestUIText, _allChronologicalQuests[_lastCompletedQuest].QuestData.DelayBeforeShownInQuickTasks));
    }

    IEnumerator EnableWithDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);
        text.enabled = true;
    }

    private void OnQuestCompletion(QuestSO sO)
    {
        QuestItemSO questItemNeeded = _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestItemNeeded;
        if (questItemNeeded != null && (!InventoryManager.Instance.RemoveQuestItemSO(questItemNeeded)))             //If player doesnt have right quest item for quest, do not complete the quest
            return;

        _allChronologicalQuests[_lastCompletedQuest].QuestUIText.color = Color.green;
        _allChronologicalQuests[_lastCompletedQuest].QuestUIText.text= _allChronologicalQuests[_lastCompletedQuest].QuestUIText.text.Insert(0,"<s>");   //Temporary strikethrough
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestCompleted?.Invoke();

        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest -= OnQuestCompletion;

        _lastCompletedQuest = Math.Min(_lastCompletedQuest + 1, _chronologicalQuests.Count - 1);         //Clamp it so it doesnt go over the last quest

        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest += OnQuestCompletion;
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestActivated?.Invoke();

        StartCoroutine(EnableWithDelay(_allChronologicalQuests[_lastCompletedQuest].QuestUIText, _allChronologicalQuests[_lastCompletedQuest].QuestData.DelayBeforeShownInQuickTasks));

        PlayQuestCompleteSound();
    }

    void PlayQuestCompleteSound()
    {
        //TODO
    }

    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest -= OnQuestCompletion;

        _lastCompletedQuest = _questSaveData[saveDateStamp];

        for(int i=_lastCompletedQuest+1;i<_chronologicalQuests.Count;i++)                 //On the quest list returning to the previous task
            _allChronologicalQuests[i].QuestUIText.enabled = false;
        for (int i = _lastCompletedQuest; i < _chronologicalQuests.Count; i++)
        { 
            _allChronologicalQuests[i].QuestUIText.text = _allChronologicalQuests[i].QuestUIText.text.Replace("<s>", "");
            _allChronologicalQuests[i].QuestUIText.color = Color.white;
        }

        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest += OnQuestCompletion;

    }

    public void SaveData(DateTime saveDateStamp)
    {
        _questSaveData.Add(saveDateStamp, _lastCompletedQuest);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.F3))
        {
            OnQuestCompletion(_allChronologicalQuests[_lastCompletedQuest].QuestData.Quest);
        }
#endif
    }
}
