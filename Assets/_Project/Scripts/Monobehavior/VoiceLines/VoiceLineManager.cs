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

    public VoiceLineDataSO CurrentVoiceLinePlaying { get; set; }
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

    public void PlayVoiceLine(VoiceLineDataSO voiceLineData,UnityEvent callBackOnVoiceLineCompleted)
    {
        if (_runningCoroutine != null)
            StopCoroutine(_runningCoroutine);

        CurrentVoiceLinePlaying = voiceLineData;
        _runningCoroutine =StartCoroutine(ShowVoiceLineDatas());

        IEnumerator ShowVoiceLineDatas()
        {
            List<VoiceLineData> dataToPlay;

            if (voiceLineData.PlayOneRandom)
                dataToPlay = new List<VoiceLineData>() { voiceLineData.VoiceLines[UnityEngine.Random.Range(0, voiceLineData.VoiceLines.Count - 1)] };
            else
                dataToPlay = voiceLineData.VoiceLines;


            for (int i = 0; i < dataToPlay.Count; i++)
            {
                yield return new WaitForSeconds(dataToPlay[i].TimeDelay);

                WalkieTalkieActivation(dataToPlay[i].TalkToWalkieTalkie);

                _voiceLineAnimator.SetBool("SubtitleShown", true);
                _voiceLineText.text = dataToPlay[i].Text;
                _voiceLineAudio.clip = dataToPlay[i].Audio;
                _voiceLineAudio.Play();

                yield return new WaitForSeconds(dataToPlay[i].Audio.length);
                _voiceLineAnimator.SetBool("SubtitleShown", false);

                yield return new WaitForSeconds(_lengthOfVoiceLineDataAnimation);

                WalkieTalkieActivation(dataToPlay[i].TalkToWalkieTalkie);
            }
            
            _runningCoroutine = null;


            callBackOnVoiceLineCompleted?.Invoke();

            _walkieTalkieActivation.Deactivate();
            CurrentVoiceLinePlaying = null;
        }

        void WalkieTalkieActivation(bool val)
        {
            if (val)
                _walkieTalkieActivation.Activate();
            else
                _walkieTalkieActivation.Deactivate();
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
