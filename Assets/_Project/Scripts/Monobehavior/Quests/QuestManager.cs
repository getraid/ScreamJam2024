using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class QuestManager : MonoBehaviour,ISaveable
{
    [SerializeField] Transform _journalQuestParent;
    [SerializeField] TMP_Text _uiQuestPrefab;
    [SerializeField] TMP_Text _uiQuestJournalPrefab;
    [SerializeField] Transform _questUIParent;
    [SerializeField] List<ChronologicalQuests> _chronologicalQuests;

    [Tooltip("Only works in editor and not build. This initially completes the set amount of quests from the start for dev purposes")]

    [Serializable]
    public struct ChronologicalQuests
    {
        public QuestSO Quest;
        public float DelayBeforeShownInQuickTasks;
    }

    List<(IQuest SceneQuest, ChronologicalQuests QuestData,TMP_Text QuestUIText,TMP_Text JournalUIText)> _allChronologicalQuests = new List<(IQuest, ChronologicalQuests, TMP_Text, TMP_Text)>();
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
                TMP_Text questSceeen = Instantiate(_uiQuestPrefab, _questUIParent);
                questSceeen.text = _chronologicalQuests[i].Quest.QuestText;

                TMP_Text questJournal = Instantiate(_uiQuestJournalPrefab, _journalQuestParent);
                questJournal.fontSize = 0.03f;
                questJournal.text = _chronologicalQuests[i].Quest.QuestText;

                _allChronologicalQuests.Add(new(sceneQuests.First(), _chronologicalQuests[i], questSceeen, questJournal));
            }
        }
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest += OnQuestCompletion;
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestActivated?.Invoke();

        StartCoroutine(EnableWithDelay(_allChronologicalQuests[_lastCompletedQuest].QuestUIText, _allChronologicalQuests[_lastCompletedQuest].QuestData.DelayBeforeShownInQuickTasks));
        StartCoroutine(EnableWithDelay(_allChronologicalQuests[_lastCompletedQuest].JournalUIText, _allChronologicalQuests[_lastCompletedQuest].QuestData.DelayBeforeShownInQuickTasks));
    }

    IEnumerator EnableWithDelay(TMP_Text text, float delay)
    {
        yield return new WaitForSeconds(delay);
        text.enabled = true;
    }
    private void OnQuestCompletion(QuestSO sO) => OnQuestCompletion(sO, false);
    private void OnQuestCompletion(QuestSO sO,bool forceCompletionWithoutQuestItem)
    {
        QuestItemSO questItemNeeded = _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestItemNeeded;
        JournalManager.Instance.NextGoal();
        
        if (!forceCompletionWithoutQuestItem && questItemNeeded != null && (!InventoryManager.Instance.RemoveQuestItemSO(questItemNeeded)))             //If player doesnt have right quest item for quest, do not complete the quest
            return;

        _allChronologicalQuests[_lastCompletedQuest].QuestUIText.color = Color.green;
        _allChronologicalQuests[_lastCompletedQuest].QuestUIText.text= _allChronologicalQuests[_lastCompletedQuest].QuestUIText.text.Insert(0,"<s>");   //Temporary strikethrough

        _allChronologicalQuests[_lastCompletedQuest].JournalUIText.color = Color.green;
        _allChronologicalQuests[_lastCompletedQuest].JournalUIText.text = _allChronologicalQuests[_lastCompletedQuest].JournalUIText.text.Insert(0, "<s>");   //Temporary strikethrough

        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestCompleted?.Invoke();

        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest -= OnQuestCompletion;

        if(_lastCompletedQuest + 1>= _chronologicalQuests.Count)
        {
            StartCoroutine(LoadEndWithDelay());
            IEnumerator LoadEndWithDelay()
            {
                yield return new WaitForSeconds(5);
                SceneManager.LoadScene(1);

            }
        }

        _lastCompletedQuest = Math.Min(_lastCompletedQuest + 1, _chronologicalQuests.Count - 1);         //Clamp it so it doesnt go over the last quest

        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest += OnQuestCompletion;
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestActivated?.Invoke();

        StartCoroutine(EnableWithDelay(_allChronologicalQuests[_lastCompletedQuest].QuestUIText, _allChronologicalQuests[_lastCompletedQuest].QuestData.DelayBeforeShownInQuickTasks));
        StartCoroutine(EnableWithDelay(_allChronologicalQuests[_lastCompletedQuest].JournalUIText, _allChronologicalQuests[_lastCompletedQuest].QuestData.DelayBeforeShownInQuickTasks));
        
        
    }


    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        _allChronologicalQuests[_lastCompletedQuest].SceneQuest.TryCompleteQuest -= OnQuestCompletion;

        _lastCompletedQuest = _questSaveData[saveDateStamp];

        for (int i = _lastCompletedQuest + 1; i < _chronologicalQuests.Count; i++)                 //On the quest list returning to the previous task
        {
            _allChronologicalQuests[i].QuestUIText.enabled = false;
            _allChronologicalQuests[i].JournalUIText.enabled = false;

        }
        for (int i = _lastCompletedQuest; i < _chronologicalQuests.Count; i++)
        { 
            _allChronologicalQuests[i].QuestUIText.text = _allChronologicalQuests[i].QuestUIText.text.Replace("<s>", "");
            _allChronologicalQuests[i].QuestUIText.color = Color.white;

            _allChronologicalQuests[i].JournalUIText.text = _allChronologicalQuests[i].JournalUIText.text.Replace("<s>", "");
            _allChronologicalQuests[i].JournalUIText.color = Color.white;
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
            QuestItemSO questItemAdded = _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestItemToPickupOnCompletion;
            QuestItemSO questItemNeeded = _allChronologicalQuests[_lastCompletedQuest].SceneQuest.QuestItemNeeded;

            if (questItemAdded != null)
                InventoryManager.Instance.AddQuestItemSO(questItemAdded);
            if (questItemNeeded != null)
                InventoryManager.Instance.RemoveQuestItemSO(questItemNeeded);

            OnQuestCompletion(_allChronologicalQuests[_lastCompletedQuest].QuestData.Quest,true);
        }
#endif
    }
}
