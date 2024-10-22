using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkieTalkiActivation : MonoBehaviour,ISaveable
{
    [SerializeField] Transform _walkieTalkieMain;
    [SerializeField] List<MeshRenderer> _activeLeds;
    [SerializeField] List<MeshRenderer> _inactiveLeds;

    [SerializeField] Transform _walkieTalkieRestingTransform;
    [SerializeField] Transform _walkieTalkieActiveTransform;

    float _timeOfLerp = 1f;
    float _activeLerpTime = 0;
    bool _doesUseAnimation = false;
    bool _isActive = false;

    void Start()
    {
        for(int i=0;i< _activeLeds.Count;i++)
        {
            _activeLeds[i].material.SetFloat("_TimeOffset", i);
        }

    }

    public void Activate(bool forceActivation=false)
    {
        if (_isActive && !forceActivation)
            return;

        _activeLeds.ForEach(x=>x.gameObject.SetActive(true));
        _inactiveLeds.ForEach(x => x.gameObject.SetActive(false));


        if (_doesUseAnimation)
        {
            _walkieTalkieMain.parent = Camera.main.transform;
            StartCoroutine(LerpIntoActivePosition(_walkieTalkieRestingTransform, _walkieTalkieActiveTransform));
        }
        _isActive = true;
    }

    public void StartUsingWalkieTalkieAnimation() => _doesUseAnimation = true;
    public void StopUsingWalkieTalkieAnimation() =>_doesUseAnimation = false;
    public void StrapWalkieTalkieToBelt()
    {
        _walkieTalkieMain.parent=_walkieTalkieRestingTransform.parent;
        _walkieTalkieMain.position=_walkieTalkieRestingTransform.position;
        _walkieTalkieMain.rotation = _walkieTalkieRestingTransform.rotation;

        StartUsingWalkieTalkieAnimation();
    }


    IEnumerator LerpIntoActivePosition(Transform startTransform, Transform endTransform)
    {
        _activeLerpTime = 0;
        while (true)
        {
            _activeLerpTime += Time.deltaTime;

            float t = Mathf.Min(_activeLerpTime / _timeOfLerp, 1);
            _walkieTalkieMain.position = Vector3.Lerp(startTransform.position, endTransform.position, t);
            _walkieTalkieMain.rotation = Quaternion.Lerp(startTransform.rotation, endTransform.rotation, t);

            if (_activeLerpTime >= _timeOfLerp)
            {
                _activeLerpTime = 0;
                break;
            }

            yield return null;
        }
    }
    public void Deactivate(bool forceDeactivation=false)
    {
        if (!_isActive && !forceDeactivation)
            return;

        _activeLeds.ForEach(x => x.gameObject.SetActive(false));
        _inactiveLeds.ForEach(x => x.gameObject.SetActive(true));

        if (_doesUseAnimation)
        {
            _walkieTalkieMain.parent = _walkieTalkieRestingTransform.parent;
            StartCoroutine(LerpIntoActivePosition(_walkieTalkieActiveTransform, _walkieTalkieRestingTransform));
        }
        _isActive = false;

    }
    public struct WalkieTalkieData
    {
        public Transform Parent;
        public Transform WalkieTalkiePosition;
        public Quaternion WalkieTalkieRotation;
    }
    Dictionary<DateTime, bool> _saveData = new Dictionary<DateTime, bool>();
    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        bool wasActive = _saveData[saveDateStamp];

        if (!_isActive && wasActive)
            Activate(true);
        else if(_isActive&&!wasActive)
            Deactivate(true);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        _saveData.Add(saveDateStamp, _isActive);
    }
}
