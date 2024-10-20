using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/VoiceLineData", order = 1)]
public class VoiceLineDataSO : ScriptableObject
{
    [field:SerializeField] public List<VoiceLineData> VoiceLines { get; set; }

}

[Serializable]
public class VoiceLineData
{
    [TextArea(2, 5)] 
    public string Text;
    public AudioClip Audio;
    public float TimeDelay;
    public bool TalkToWalkieTalkie;
}


