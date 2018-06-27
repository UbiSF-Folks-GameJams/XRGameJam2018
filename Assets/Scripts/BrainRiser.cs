using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A simple script to raise brain activation over time.

public class BrainRiser : MonoBehaviour {
    public float DelayBeforeBraining = 2.0f;
    public float BrainRiseTime = 10.0f;
    public bool RepeatOnFinish = false;

    enum BrainRiserStates
    {
        Delaying,
        Rising,
        Nothingness,
    }

    private BrainRiserStates mState;
    private float stateTimer;
    private BrainAffected mBrainAffected;

    // Use this for initialization
    void Start () {
        mState = BrainRiserStates.Delaying;
        stateTimer = DelayBeforeBraining;
        mBrainAffected = GetComponent<BrainAffected>();
    }
	
	// Update is called once per frame
	void Update () {
		switch ( mState )
        {
            case BrainRiserStates.Delaying:
                stateTimer = Mathf.Max(0.0f, stateTimer - Time.deltaTime);
                mBrainAffected.ActivationLevel = 0.0f;
                if (stateTimer <= 0.0f)
                {
                    mState = BrainRiserStates.Rising;
                    stateTimer = BrainRiseTime;
                }
                break;
            case BrainRiserStates.Rising:
                stateTimer = Mathf.Max(0.0f, stateTimer - Time.deltaTime);
                mBrainAffected.ActivationLevel = 1.0f - (stateTimer / BrainRiseTime);
                if (stateTimer <= 0.0f)
                {
                    mState = RepeatOnFinish ? BrainRiserStates.Delaying : BrainRiserStates.Nothingness;
                    stateTimer = DelayBeforeBraining;
                }
                break;
            case BrainRiserStates.Nothingness:
                //Embrace it.
                mBrainAffected.ActivationLevel = 0.0f;
                break;
        }
	}
}
