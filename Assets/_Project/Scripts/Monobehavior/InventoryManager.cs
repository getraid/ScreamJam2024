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

    List<List<QuestItemSO>> _saveData=new List<List<QuestItemSO>>();
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

    public void ReloadFromSafe(int saveIndex)
    {
        List<QuestItemSO> previousItemsOnSave = _saveData[saveIndex];

        List<QuestItemSO> toDelete = _collectedQuestItems.Keys.Where(x => !previousItemsOnSave.Contains(x)).ToList();

        toDelete.ForEach(x => RemoveQuestItemSO(x));
    }

    public void SaveData()
    {
        _saveData.Add(new List<QuestItemSO>(_collectedQuestItems.Keys ));
    }
}
