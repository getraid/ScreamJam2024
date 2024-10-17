using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/QuestScriptableObject", order = 1)]
public class QuestSO : ScriptableObject
{
    [field:SerializeField] public string QuestText { get; set; }
}
