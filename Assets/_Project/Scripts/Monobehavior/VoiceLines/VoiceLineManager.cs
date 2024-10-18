using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoiceLineManager : MonoBehaviour
{
    [SerializeField] AudioSource _voiceLineAudio;
    [SerializeField] TMP_Text _VoiceLineText;
    [SerializeField] Animator _VoiceLineAnimator;

    [SerializeField] float _lengthOfVoiceLineDataAnimation = 1/3f;
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
        }
    }

    public void PlayVoiceLine(VoiceLineDataSO voiceLineData)
    {
        StartCoroutine(ShowVoiceLineDatas());

        IEnumerator ShowVoiceLineDatas()
        {
            for (int i = 0; i < voiceLineData.VoiceLineDataVoiceLines.Count; i++)
            {
                yield return new WaitForSeconds(voiceLineData.VoiceLineDataVoiceLines[i].TimeDelay);

                _VoiceLineAnimator.SetBool("SubtitleShown", true);
                _VoiceLineText.text = voiceLineData.VoiceLineDataVoiceLines[i].Text;
                _voiceLineAudio.clip = voiceLineData.VoiceLineDataVoiceLines[i].Audio;
                _voiceLineAudio.Play();

                yield return new WaitForSeconds(voiceLineData.VoiceLineDataVoiceLines[i].Audio.length);
                _VoiceLineAnimator.SetBool("SubtitleShown", false);

                yield return new WaitForSeconds(_lengthOfVoiceLineDataAnimation);
            } 
        }
    }

}
