using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/QuestItemSO", order = 1)]

public class QuestItemSO : ScriptableObject
{
    [field:SerializeField] public Sprite UISprite { get; set; } 
}
