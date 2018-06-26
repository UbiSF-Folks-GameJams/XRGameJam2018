using System.Collections;
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
    Delta,
    Gamma,
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

    private static bool mDebugBrainLevels = false;
    private static bool mDebugAttentionLevel = true;
    private int NumberOfWaves = 5;
    private BrainAffected mCurrentTarget;
    private BrainGrabberStates mCurrentState;
    private float stateTimer;
    float[] mBrainWaves;
    float[] mBrainWaveBaseline;
    float mBrainWaveTotalBaseline;
    float mCurrentAttention;
    //To activate, 

    void Awake()
    {
        //trackedObj = GetComponent<SteamVR_TrackedObject>();
        mBrainWaves = new float[NumberOfWaves];
        mBrainWaveBaseline = new float[NumberOfWaves];
        for (int index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaves[index] = 0.0f;
            mBrainWaveBaseline[index] = 0.0f;
        }
        mCurrentTarget = null;
        mCurrentAttention = 0.0f;
    }

    // Use this for initialization
    void Start ()
    {
        EnterState( BrainGrabberStates.Brain_Initializing );
	}
	
	void Update ()
    {
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
        if ( ( mCurrentState == BrainGrabberStates.Brain_Interacting ) && 
            ( mCurrentTarget != null ) && mCurrentTarget.CanBeMoved )
        {
            Vector3 desiredPosition = transform.position + ( mGrabbedObjectDistance * transform.forward );
            Rigidbody rb = GetComponent<Rigidbody>();
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

        //Each frame, gather data (INSERT API HERE)
        for (index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaves[index] = Random.Range(0.0f, 1.0f);
        }
        //Then average that new data with the accumulated data.
        for (index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaveBaseline[index] = ((stateTimer * mBrainWaveBaseline[index]) + (Time.deltaTime * mBrainWaves[index])) / (stateTimer + Time.deltaTime);
        }
        if (stateTimer > mGatheringBaselineTime)
            EnterState(BrainGrabberStates.Brain_NotInteracting);
        Debug.Log("Initializing! " + stateTimer + " out of " + mGatheringBaselineTime );
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
            AccumulateActivation(1.0f, 0.1f, true );
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
        AccumulateActivation(0.0f, 2.0f, true);
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
        //Set the new state
        mCurrentState = newState;
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
        for (int index = 0; index < NumberOfWaves; ++index)
        {
            mBrainWaves[index] = Random.Range(0.0f, 1.0f);
        }
    }

    private void DrawheadLine()
    {
        Debug.DrawLine(mCameraFacing.transform.position, 
            mCameraFacing.transform.position + (mLaserVisibleDistance * mCameraFacing.transform.forward), Color.red);
    }

    private void AccumulateActivation(float positiveMultiplier = 1.0f, float negativeMultiplier = 0.0f, bool zeroOutBelowBaseline = true )
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
    }

    private float AverageArray( float[] theWaves, bool excludeNANsAndINFs = true )
    {
        float total = 0.0f;
        int totalSamples = theWaves.Length;
        for (int index = 0; index < theWaves.Length; ++index)
        {
            if ((theWaves[index] <= -100000.0f) || (theWaves[index] >= 100000.0f))
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
