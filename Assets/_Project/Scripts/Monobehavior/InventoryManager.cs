using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour,ISaveable
{
    [SerializeField] Image _uiQuestItemPrefab;
    [SerializeField] Transform _uiQuestParent;
    Dictionary<QuestItemSO,Image> _collectedQuestItems=new Dictionary<QuestItemSO, Image> ();

    public static InventoryManager Instance { get; private set; }

    Dictionary<DateTime,List<QuestItemSO>> _saveData=new Dictionary<DateTime, List<QuestItemSO>>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    public bool AddQuestItemSO(QuestItemSO item)
    {
        if (!_collectedQuestItems.ContainsKey(item))
        {
            Image img = Instantiate(_uiQuestItemPrefab, _uiQuestParent);
            img.sprite = item.UISprite;
            _collectedQuestItems.Add(item, img);
            return true;
        }
        return false;
    }
    public bool RemoveQuestItemSO(QuestItemSO item)
    {
        if(_collectedQuestItems.ContainsKey(item))
        {
            Destroy(_collectedQuestItems[item].gameObject);
            _collectedQuestItems.Remove(item);
            return true;
        }
        return false;
    }

    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        List<QuestItemSO> previousItemsOnSave = _saveData[saveDateStamp];

        List<QuestItemSO> toDelete = _collectedQuestItems.Keys.Where(x => !previousItemsOnSave.Contains(x)).ToList();

        toDelete.ForEach(x => RemoveQuestItemSO(x));
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _saveData.Add(saveDateStamp, new List<QuestItemSO>(_collectedQuestItems.Keys ));
    }
}
