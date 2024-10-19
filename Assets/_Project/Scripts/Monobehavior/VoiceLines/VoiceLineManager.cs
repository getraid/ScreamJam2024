using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class VoiceLineManager : MonoBehaviour,ISaveable
{
    [SerializeField] AudioSource _voiceLineAudio;
    [SerializeField] TMP_Text _voiceLineText;
    [SerializeField] Animator _voiceLineAnimator;
    [SerializeField] AnimationClip _subtitleAnimationClip;
    [SerializeField] WalkieTalkiActivation _walkieTalkieActivation;

    float _lengthOfVoiceLineDataAnimation;
    Coroutine _runningCoroutine;
    public static VoiceLineManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            _lengthOfVoiceLineDataAnimation = _subtitleAnimationClip.length;
        }
    }

    public void PlayVoiceLine(VoiceLineDataSO voiceLineData,bool activateWalkieTalkie,UnityEvent callBackOnVoiceLineCompleted)
    {
        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);

        _runningCoroutine=StartCoroutine(ShowVoiceLineDatas());

        IEnumerator ShowVoiceLineDatas()
        {
            if(activateWalkieTalkie)
                _walkieTalkieActivation.Activate();

            for (int i = 0; i < voiceLineData.VoiceLines.Count; i++)
            {
                yield return new WaitForSeconds(voiceLineData.VoiceLines[i].TimeDelay);

                _voiceLineAnimator.SetBool("SubtitleShown", true);
                _voiceLineText.text = voiceLineData.VoiceLines[i].Text;
                _voiceLineAudio.clip = voiceLineData.VoiceLines[i].Audio;
                _voiceLineAudio.Play();

                yield return new WaitForSeconds(voiceLineData.VoiceLines[i].Audio.length);
                _voiceLineAnimator.SetBool("SubtitleShown", false);

                yield return new WaitForSeconds(_lengthOfVoiceLineDataAnimation);
            }
            _runningCoroutine = null;
            if (activateWalkieTalkie)
                _walkieTalkieActivation.Deactivate();

            callBackOnVoiceLineCompleted?.Invoke();
        }
    }

    public void ReloadFromSafe(DateTime saveDateStamp)
    {
        if(_runningCoroutine!=null)
            StopCoroutine(_runningCoroutine);

        _voiceLineAudio.Stop();
        _voiceLineAnimator.SetBool("SubtitleShown", false);
    }

    public void SaveData(DateTime saveDateStamp)
    {
        //
    }
}
