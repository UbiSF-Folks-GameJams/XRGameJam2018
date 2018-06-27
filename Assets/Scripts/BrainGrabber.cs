﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The brain grabber does some amount of smoothing on the brain input before telling the target object it's being messed with.
//So all the affected object gets is a "is this thing on" and a total amount of activation (which we can think of is a 0-1 where 1 is 
//where the actual effect happens.

public enum BrainGrabberStates
{
    Brain_Initializing, //The state in which the grabber is getting the the baseline brain wave values.
    Brain_NotInteracting, //
    Brain_Interacting,
}

public enum BrainWaveNames
{
    Alpha = 0,
    Beta,
    Gamma,
    Delta,
    Theta,
}

public class BrainGrabber : MonoBehaviour
{
    //The VR head rig itself.
    public GameObject mCameraFacing;
    //The layer of objects we'll allow the brain can manipulate.
    public LayerMask brainAffectedLayer;
    //How far the head laser extends.
    public float mLaserVisibleDistance = 100.0f;
    //The amount of time within which we're just grabbing a resting state to compare against.
    public float mGatheringBaselineTime = 1.0f;
    //The amount of deviation from the baseline we require before we start filling the activation "bucket".
    public float mRequiredDeviation = 0.2f;
    //The total deviation-over-time bucket that you have to fill to activate the object.
    public float mActivationBucketSize = 2.0f;
    //The total deviation-over-time bucket that you have to fill to un-activate the object.
    public float mDeactivationBucketSize = 1.0f;
    //The amount per second that the activation bucket falls if you switch objects.
    public float mActivationLossOnOtherObjectPerSecond = 2.0f;
    //How close to your eyeline you want to drag movable objects.
    public float mGrabbedObjectDistance = 1.5f;
    //
    public float mAmountToMoveEachFrame = 0.05f;

    //
    public float mAccumulatePositive = 1.0f;

    private static bool mDebugBrainLevels = false;
    private static bool mDebugAttentionLevel = true;
    private static bool mDebugFixedUpdate = false;
    private static bool mDebugActualBrains = true;
    private int NumberOfWaves = 5;
    private BrainAffected mCurrentTarget;
    private BrainGrabberStates mCurrentState;
    private float stateTimer;
    float[] mBrainWaves;
    float[] mBrainWaveAccumulator;
    float[] mBrainWaveBaseline;
    float mBrainWaveTotalBaseline;
    float mCurrentAttention;
    int numSamples;

    //To activate, 

    public static readonly float[] ElenaAtRestValues = { -1.71f, -2.61f, -1.93f, -3.51f, -3.04f };
    public static readonly float[] ElenaFocusValues = { -2.61f, -1.8f, -2.39f, -1.71f, -3.17f };
    //-3.36,-2.97,-2.71,-3.38,-3.99]
    //-2.61,-1.8, -2.39,-1.71,-3.17
    public static readonly float[] ElenaConsistencyValues = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
    //public static readonly float[] ElenaConsistencyValues = { 1.0f, 0.0f, 0.0f, 0.0f, 0.0f };
    void Awake()
    {
        //trackedObj = GetComponent<SteamVR_TrackedObject>();
        mBrainWaves = new float[NumberOfWaves];
        mBrainWaveBaseline = new float[NumberOfWaves];
        mBrainWaveAccumulator = new float[NumberOfWaves];
        for (int index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaves[index] = 0.0f;
            mBrainWaveBaseline[index] = 0.0f;
            mBrainWaveAccumulator[index] = 0.0f;
        }
        mCurrentTarget = null;
        mCurrentAttention = 0.0f;
        numSamples = 0;
    }

    // Use this for initialization
    void Start ()
    {
        EnterState( BrainGrabberStates.Brain_Initializing );
	}
	
	void Update ()
    {
        mBrainWaves[(int)BrainWaveNames.Alpha] = EEGDataReceiver.alphaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Beta] = EEGDataReceiver.betaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Gamma] = EEGDataReceiver.gammaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Delta] = EEGDataReceiver.deltaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Theta] = EEGDataReceiver.thetaAbsolute;

        switch ( mCurrentState )
        {
            case BrainGrabberStates.Brain_Initializing:
                InitializingState();
                break;
            case BrainGrabberStates.Brain_NotInteracting:
                
                NotInteractingState();
                break;
            case BrainGrabberStates.Brain_Interacting:
                InteractingState();
                break;
        }
        stateTimer += Time.deltaTime;
    }

    public void FixedUpdate()
    {
        if (mDebugFixedUpdate)
            Debug.Log("Enter fixed update!" + mCurrentState + mCurrentTarget );
        if ( ( mCurrentState == BrainGrabberStates.Brain_Interacting ) && 
            ( mCurrentTarget != null ) && mCurrentTarget.CanBeMoved )
        {
            Vector3 desiredPosition = transform.position + ( mGrabbedObjectDistance * transform.forward );
            Rigidbody rb = GetComponent<Rigidbody>();
            if (mDebugFixedUpdate)
                Debug.Log("Attempted to move object!");
            if (rb.isKinematic)
                rb.MovePosition(Vector3.Lerp(mCurrentTarget.transform.position, desiredPosition, mAmountToMoveEachFrame));
            else
                mCurrentTarget.transform.position = Vector3.Lerp(mCurrentTarget.transform.position, desiredPosition, mAmountToMoveEachFrame);
        }
    }

    /****STATE LOGIC****/

    private void InitializingState()
    {
        int index;

        /*for (index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaves[index] = Random.Range(0.0f, 1.0f);
        }*/
        /*mBrainWaves[(int)BrainWaveNames.Alpha] = EEGDataReceiver.alphaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Beta] = EEGDataReceiver.betaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Gamma] = EEGDataReceiver.gammaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Delta] = EEGDataReceiver.deltaAbsolute;
        mBrainWaves[(int)BrainWaveNames.Theta] = EEGDataReceiver.thetaAbsolute;
        //Then average that new data with the accumulated data.
        for (index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaveBaseline[index] = ((stateTimer * mBrainWaveBaseline[index]) + (Time.deltaTime * mBrainWaves[index])) / (stateTimer + Time.deltaTime);
        }*/
        Debug.Log("Initializing! At time " + stateTimer + " samples are " +
            EEGDataReceiver.alphaAbsolute + ", " +
            EEGDataReceiver.betaAbsolute + ", " +
            EEGDataReceiver.gammaAbsolute + ", " +
            EEGDataReceiver.deltaAbsolute + ", " +
            EEGDataReceiver.thetaAbsolute + ".");

        mBrainWaveAccumulator[(int)BrainWaveNames.Alpha] += EEGDataReceiver.alphaAbsolute;
        mBrainWaveAccumulator[(int)BrainWaveNames.Beta] += EEGDataReceiver.betaAbsolute;
        mBrainWaveAccumulator[(int)BrainWaveNames.Gamma] += EEGDataReceiver.gammaAbsolute;
        mBrainWaveAccumulator[(int)BrainWaveNames.Delta] += EEGDataReceiver.deltaAbsolute;
        mBrainWaveAccumulator[(int)BrainWaveNames.Theta] += EEGDataReceiver.thetaAbsolute;
        ++numSamples;
        mBrainWaveBaseline[(int)BrainWaveNames.Alpha] = mBrainWaveAccumulator[(int)BrainWaveNames.Alpha] / (float)numSamples;
        mBrainWaveBaseline[(int)BrainWaveNames.Beta] = mBrainWaveAccumulator[(int)BrainWaveNames.Beta] / (float)numSamples;
        mBrainWaveBaseline[(int)BrainWaveNames.Gamma] = mBrainWaveAccumulator[(int)BrainWaveNames.Gamma] / (float)numSamples;
        mBrainWaveBaseline[(int)BrainWaveNames.Delta] = mBrainWaveAccumulator[(int)BrainWaveNames.Delta] / (float)numSamples;
        mBrainWaveBaseline[(int)BrainWaveNames.Theta] = mBrainWaveAccumulator[(int)BrainWaveNames.Theta] / (float)numSamples;
        if (stateTimer > mGatheringBaselineTime)
            EnterState(BrainGrabberStates.Brain_NotInteracting);
        Debug.Log("Initializing! At time " + stateTimer + " baseline is " +
            mBrainWaveBaseline[(int)BrainWaveNames.Alpha] + ", " +
            mBrainWaveBaseline[(int)BrainWaveNames.Beta] + ", " +
            mBrainWaveBaseline[(int)BrainWaveNames.Delta] + ", " +
            mBrainWaveBaseline[(int)BrainWaveNames.Gamma] + ", " +
            mBrainWaveBaseline[(int)BrainWaveNames.Theta] + "." );
        Debug.Log("alpha accumulator = " + mBrainWaveAccumulator[(int)BrainWaveNames.Alpha]);
    }

    private void NotInteractingState()
    {
        //Draw a line down the facing line so folks can see it. 
        DrawheadLine();
        //Read the current frame of brain data
        ReadBrainlevel();
        //Try to get an object.
        BrainAffected thisFrameLookTarget = CheckGrabObject();
        //If this frame's look target is the same as the saved one, accumulate attention!
        if (thisFrameLookTarget == mCurrentTarget)
        {
            AccumulateActivation(1.0f, 1.0f, true );
            if (mCurrentAttention > mActivationBucketSize)
                EnterState(BrainGrabberStates.Brain_Interacting);
        }
        //If this frame's look target is different, reduce attention on the current, and switch if the threshold is low enough.
        else
        {
            mCurrentAttention = Mathf.Clamp(mCurrentAttention - (Time.deltaTime * mActivationLossOnOtherObjectPerSecond),
                0.0f, Mathf.Infinity);
            if (mCurrentAttention <= 0.0f)
            {
                mCurrentTarget = thisFrameLookTarget;
                //if ( mCurrentTarget != null )
                //    SetColorOfObject(mCurrentTarget.gameObject);
            }
        }
        //If we have a valid target, pass the current activation levels along to it. Rescale to 0-1 
        if (mCurrentTarget != null)
            mCurrentTarget.ActivationLevel = Mathf.Clamp01( mCurrentAttention / mActivationBucketSize );
    }

    private void InteractingState()
    {
        //If for some reason we've lost our target, drop out of this state immediately.
        if ( mCurrentTarget == null )
        {
            EnterState(BrainGrabberStates.Brain_NotInteracting);
            return;
        }
        //Outline the affected object.
        //We don't allow object switching until you've "let go" of the current object. So no need to check current head-aim.
        //Read brain levels
        ReadBrainlevel();
        //
        AccumulateActivation(1.0f, 1.0f, true);
        if (mCurrentAttention <= 0.0f)
            EnterState(BrainGrabberStates.Brain_NotInteracting);
        //On the affected object, consider the attention level at 1.0 until told otherwise.
        mCurrentTarget.ActivationLevel = 1.0f;
    }

    private void EnterState(BrainGrabberStates newState)
    {
        if (newState == mCurrentState)
            return;
        //Do any state exit logic
        if (mCurrentState == BrainGrabberStates.Brain_Initializing)
            mBrainWaveTotalBaseline = AverageArray( mBrainWaveBaseline );
        if (mCurrentState == BrainGrabberStates.Brain_Interacting)
        {
            //Always throw movable objects when we're done with them
            if ( (mCurrentTarget != null) && ( mCurrentTarget.CanBeMoved ) )
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if ( rb != null )
                    rb.AddForce(transform.forward, ForceMode.Acceleration);
            }
        }
        //Set the new state
        mCurrentState = newState;
        Debug.Log("Entered new state! " + newState);
        stateTimer = 0.0f;
        //Do any state entry logic, if needed
    }

    /****UTILITY METHODS****/

    //If the headset is currently pointing at an object that has a brain-affected script, return it.
    private BrainAffected CheckGrabObject()
    {
        //Then figure out what object is at the other end of that raytrace.
        RaycastHit aRaycastReturn;
        bool hitAThing = Physics.Raycast(mCameraFacing.transform.position, mCameraFacing.transform.forward, out aRaycastReturn, mLaserVisibleDistance, brainAffectedLayer.value);
        if (!hitAThing)
            return null;
        Debug.Log("Raycast hit!");
        //If the brain (or controller for now) is engaged, start affecting/moving the targeted object.
        if (aRaycastReturn.transform.gameObject != null)
        {
            Debug.Log("Raycast hit " + aRaycastReturn.transform.gameObject.name);
            Debug.DrawLine( mCameraFacing.transform.position, aRaycastReturn.point, Color.green );
        }
        BrainAffected theBrainPart = aRaycastReturn.transform.GetComponent<BrainAffected>();
        
        return theBrainPart;
        
    }

    private void ReadBrainlevel( )
    {
        //Each frame, gather data (INSERT API HERE)
        /*for (int index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaves[index] = Random.Range(0.0f, 1.0f);
        }*/
    }

    private void DrawheadLine()
    {
        Debug.DrawLine(mCameraFacing.transform.position, 
            mCameraFacing.transform.position + (mLaserVisibleDistance * mCameraFacing.transform.forward), Color.red);
    }

    private void AccumulateActivation(float positiveMultiplier = 1.0f, float negativeMultiplier = 0.0f, bool zeroOutBelowBaseline = true)
    {
        /*public static readonly float[] ElenaAtRestValues = { 0.75f, 0.33f, 0.5f, 0.675f, 0.325f };
        public static readonly float[] ElenaFocusValues = { 0.31f, 0.475f, 0.725f, 0.5f, 0.1f };
        public static readonly float[] ElenaConsistencyValues = { 1.0f, 0.5f, 0.5f, 0.0f, 0.8f };**/

        
        float frameScore = 0.0f;
        float consistencySum = 0.0f;
        for (int index = 0; index < NumberOfWaves; ++index)
        {
            consistencySum += ElenaConsistencyValues[index];
        }
        for ( int index = 0; index < NumberOfWaves; ++index )
        {
            float theUnlerp = Mathf.InverseLerp(ElenaAtRestValues[index], ElenaFocusValues[index], mBrainWaves[index]);
            theUnlerp = Mathf.Lerp(-1.0f, 1.0f, theUnlerp);
            frameScore += ( ElenaConsistencyValues[index] * theUnlerp ) / consistencySum;
        }
        Debug.Log("Added " + frameScore + " on frame " + Time.time + ".");
        mCurrentAttention += Time.deltaTime * frameScore * ( ( frameScore > 0.0f ) ? positiveMultiplier : negativeMultiplier  );

        mCurrentAttention = Mathf.Clamp(mCurrentAttention, 0.0f, Mathf.Infinity);

        if (mDebugActualBrains)
        {
            Debug.Log("Alpha: Resting - " + ElenaAtRestValues[(int)BrainWaveNames.Alpha] + ", Current - " +
                mBrainWaves[(int)BrainWaveNames.Alpha] + ", Focused - " + ElenaFocusValues[(int)BrainWaveNames.Alpha] + ", ilerp - " +
                Mathf.InverseLerp(ElenaAtRestValues[(int)BrainWaveNames.Alpha], ElenaFocusValues[(int)BrainWaveNames.Alpha], mBrainWaves[(int)BrainWaveNames.Alpha]) );
            Debug.Log("Beta: Resting - " + ElenaAtRestValues[(int)BrainWaveNames.Beta] + ", Current - " +
                mBrainWaves[(int)BrainWaveNames.Beta] + ", Focused - " + ElenaFocusValues[(int)BrainWaveNames.Beta]);
            Debug.Log("Gamma: Resting - " + ElenaAtRestValues[(int)BrainWaveNames.Gamma] + ", Current - " +
                            mBrainWaves[(int)BrainWaveNames.Gamma] + ", Focused - " + ElenaFocusValues[(int)BrainWaveNames.Gamma]);
            Debug.Log("Delta: Resting - " + ElenaAtRestValues[(int)BrainWaveNames.Delta] + ", Current - " +
                            mBrainWaves[(int)BrainWaveNames.Delta] + ", Focused - " + ElenaFocusValues[(int)BrainWaveNames.Delta]);
            Debug.Log("Theta: Resting - " + ElenaAtRestValues[(int)BrainWaveNames.Theta] + ", Current - " +
                            mBrainWaves[(int)BrainWaveNames.Theta] + ", Focused - " + ElenaFocusValues[(int)BrainWaveNames.Theta]);
            Debug.Log("Consistency sum = " + consistencySum + ", score for frame = " + frameScore);
        }
        if (mDebugAttentionLevel)
            Debug.Log("Attention level = " + mCurrentAttention + " for object " + ((mCurrentTarget == null) ? "null" : mCurrentTarget.name));
    }

       /*private void AccumulateActivation(float positiveMultiplier = 1.0f, float negativeMultiplier = 0.0f, bool zeroOutBelowBaseline = true )
    {
        //Average existing brainwaves.
        float thisFramesWaves = AverageArray( mBrainWaves );
        if (mDebugBrainLevels)
            Debug.Log("Currenet brain level = " + thisFramesWaves + " against baseline " + mBrainWaveTotalBaseline);
        //Get the delta between this frame's brainwaves and the baseline.
        float thisFrameDelta = thisFramesWaves - mBrainWaveTotalBaseline;
        //if we zero out on negative, do this check here.
        if (zeroOutBelowBaseline)
            thisFrameDelta = Mathf.Clamp(thisFrameDelta, 0.0f, Mathf.Infinity);
        else
            thisFrameDelta = Mathf.Abs(thisFrameDelta);
        //How far above our threshold is this frame?
        float thisFrameDeviation = thisFrameDelta - (mRequiredDeviation * mBrainWaveTotalBaseline);
        //If the deviation is negative (aka its within the baseline) use our negative multiplier.
        mCurrentAttention += ( (thisFrameDeviation < 0.0f ) ? negativeMultiplier : positiveMultiplier ) * Time.deltaTime * thisFrameDeviation;
        mCurrentAttention = Mathf.Clamp(mCurrentAttention, 0.0f, Mathf.Infinity);
        if (mDebugAttentionLevel)
            Debug.Log("Attention level = " + mCurrentAttention + " for object " + (( mCurrentTarget == null) ? "null" : mCurrentTarget.name));
    }*/

    private float AverageArray( float[] theWaves, bool excludeNANsAndINFs = true )
    {
        float total = 0.0f;
        int totalSamples = theWaves.Length;
        for (int index = 0; index < theWaves.Length; ++index)
        {
            if ((theWaves[index] <= -100000.0f) || (theWaves[index] >= 100000.0f) || ( theWaves[index] == float.NaN ))
                --totalSamples;
            else
                total += theWaves[index];
        }
        return total / totalSamples;
    }

    private void SetColorOfObject( GameObject aTarget )
    {
        Renderer rend = aTarget.GetComponent<Renderer>();

        //Set the main Color of the Material to green
        rend.material.shader = Shader.Find("_Color");
        rend.material.SetColor("_Color", Color.green);
    }
}
