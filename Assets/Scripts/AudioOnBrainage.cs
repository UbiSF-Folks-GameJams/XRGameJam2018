using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOnBrainage : MonoBehaviour
{
    public AudioClip finalSound;
    public float PitchAtStart = 1.0f;
    public float PitchAtEnd = 2.0f;
    public float VolumeAtStart = 0.0f;
    public float VolumeAtEnd = 1.0f;
    public float PowerOfAudioCurve = 2.0f;
    public bool mStopOnMaxReached = true;

    private BrainAffected mBrainAffected;
    private AudioSource mAudioSource;

    private float audioLerp;
    private bool maxReached;

    enum AudioOnBrainageStates
    {
        WaitingAtZero,
        Playing,
        Done,
    }

    AudioOnBrainageStates currentState;

    // Use this for initialization
    void Start ()
    {
        mBrainAffected = GetComponent<BrainAffected>();
        mAudioSource = GetComponent<AudioSource>();
        maxReached = false;
        currentState = AudioOnBrainageStates.WaitingAtZero;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if ((mBrainAffected == null) || (mAudioSource == null))
            return;
		switch (currentState)
        {
            case AudioOnBrainageStates.WaitingAtZero:
                if ( mBrainAffected.ActivationLevel > 0.0f )
                {
                    mAudioSource.Play();
                    currentState = AudioOnBrainageStates.Playing;
                }
                break;
            case AudioOnBrainageStates.Playing:
                audioLerp = Mathf.Pow(Mathf.Clamp01(mBrainAffected.ActivationLevel), PowerOfAudioCurve);
                mAudioSource.volume = Mathf.Lerp(VolumeAtStart, VolumeAtEnd, audioLerp);
                mAudioSource.pitch = Mathf.Lerp(PitchAtStart, PitchAtEnd, audioLerp);
                if ( mStopOnMaxReached && ( mBrainAffected.ActivationLevel >= 1.0f ) )
                {
                    mAudioSource.Stop();
                    mAudioSource.clip = finalSound;
                    mAudioSource.pitch = 1.0f;
                    mAudioSource.PlayOneShot(finalSound);
                    currentState = AudioOnBrainageStates.Done;
                }
                break;
            case AudioOnBrainageStates.Done:
                break;
        }
	}
}
