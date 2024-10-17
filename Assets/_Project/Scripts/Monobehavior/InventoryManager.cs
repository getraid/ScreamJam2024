using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] Image _uiQuestItemPrefab;
    [SerializeField] Transform _uiQuestParent;
    Dictionary<QuestItemSO,Image> _collectedQuestItems=new Dictionary<QuestItemSO, Image> ();

    public static InventoryManager Instance { get; private set; }

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
}
