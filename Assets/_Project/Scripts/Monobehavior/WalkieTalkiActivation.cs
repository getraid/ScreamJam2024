using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkieTalkiActivation : MonoBehaviour
{
    [SerializeField] List<MeshRenderer> _activeLeds;
    [SerializeField] List<MeshRenderer> _inactiveLeds;
    void Start()
    {
        for(int i=0;i< _activeLeds.Count;i++)
        {
            _activeLeds[i].material.SetFloat("_TimeOffset", i);
        }

    }

    public void Activate()
    {
        _activeLeds.ForEach(x=>x.gameObject.SetActive(true));
        _inactiveLeds.ForEach(x => x.gameObject.SetActive(false));
    }
    public void Deactivate()
    {
        _activeLeds.ForEach(x => x.gameObject.SetActive(false));
        _inactiveLeds.ForEach(x => x.gameObject.SetActive(true));
    }
}
