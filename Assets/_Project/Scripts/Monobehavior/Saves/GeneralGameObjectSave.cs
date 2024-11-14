using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneralGameObjectSave : MonoBehaviour, ISaveable
{
    

    struct GeneralGameObjectData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool ActiveItself;
        public List<bool> ComponentActivity;
        public List<int> AnimatorHashNames;
    }
    [SerializeField] List<Component> _componentActivityToTrack;
    [SerializeField] List<Animator> _animatorsToTrack;

    Dictionary<DateTime,GeneralGameObjectData> _savedData= new Dictionary<DateTime, GeneralGameObjectData>();
    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_savedData.ContainsKey(saveDateStamp))
        {
            GeneralGameObjectData dataToLoad = _savedData[saveDateStamp];

            gameObject.transform.position = dataToLoad.Position;
            gameObject.transform.rotation = dataToLoad.Rotation;
            gameObject.SetActive(dataToLoad.ActiveItself);

            for (int i = 0; i < _componentActivityToTrack.Count; i++)
            {
                if (_componentActivityToTrack[i] is Collider collider)
                    collider.enabled = dataToLoad.ComponentActivity[i];
                else if(_componentActivityToTrack[i] is MonoBehaviour monbehav)
                    monbehav.enabled = dataToLoad.ComponentActivity[i];
            }
            for (int i = 0; i < _animatorsToTrack.Count; i++)
            {
                _animatorsToTrack[i].Play(dataToLoad.AnimatorHashNames[i]);
            }
        }
        else
            Destroy(gameObject);
        
    }

    public void SaveData(DateTime saveDateStamp)
    {
        GeneralGameObjectData saveData;
        saveData.Position=gameObject.transform.position;
        saveData.Rotation = gameObject.transform.rotation;
        saveData.ActiveItself = gameObject.activeSelf;

        saveData.ComponentActivity = _componentActivityToTrack.Select(x=> 
        {
            if (x is Collider col)
                return col.enabled;
            else
                return ((MonoBehaviour)x).enabled;
        }).ToList();

        List<int> animatorStateNames = new List<int>();

        for(int i=0;i<_animatorsToTrack.Count;i++)
        {
            animatorStateNames.Add(_animatorsToTrack[i].GetCurrentAnimatorStateInfo(0).shortNameHash);
        }
        saveData.AnimatorHashNames = animatorStateNames;

        _savedData.Add(saveDateStamp, saveData);
    }
}
