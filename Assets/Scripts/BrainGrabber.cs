using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The brain grabber does some amount of smoothing on the brain input before telling the target object it's being messed with.
//So all the affected object gets is a "is this thing on" and a total amount of activation (which we can think of is a 0-1 where 1 is 
//where the actual effect happens.

public enum BrainGrabberStates
{
    Brain_NotInteracting,
    Brain_Interacting,
}

public class BrainGrabber : MonoBehaviour
{
    //The VR rig itself
    public GameObject mCameraFacing;
    //The layer of things the brain can manipulate
    LayerMask brainAffectedLayer;
    //How far your head laser extends.
    public float mLaserVisibleDistance = 100.0f;

    //A reference to the tracked controller.
    //private SteamVR_TrackedObject trackedObj;
    private BrainAffected currentTarget;
    private BrainGrabberStates currentState;
    
    void Awake()
    {
        //trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    // Use this for initialization
    void Start ()
    {
        currentState = BrainGrabberStates.Brain_NotInteracting;
	}
	
	void Update ()
    {
        switch ( currentState )
        {
            case BrainGrabberStates.Brain_NotInteracting:
                //Draw a line down the facing line so folks can see it. 
                Debug.DrawLine(mCameraFacing.transform.position, mCameraFacing.transform.position + (mLaserVisibleDistance * mCameraFacing.transform.forward));
                if (CheckGrabObject())
                    currentState = BrainGrabberStates.Brain_Interacting;
                break;
            case BrainGrabberStates.Brain_Interacting:
                //Read brain levels
                float amountOfBrainage = ReadBrainlevel();
                if (currentTarget != null)
                    currentTarget.ActivationLevel = amountOfBrainage;
                //If there's no attention, revert to not interacting.
                if ( (currentTarget == null) || (amountOfBrainage <= 0.0f))
                    currentState = BrainGrabberStates.Brain_NotInteracting;
                break;
        }
        
    }

    private bool CheckGrabObject()
    {
        //Then figure out what object is at the other end of that raytrace.
        RaycastHit aRaycastReturn;
        bool hitAThing = Physics.Raycast(mCameraFacing.transform.position, mCameraFacing.transform.forward, out aRaycastReturn, mLaserVisibleDistance, 0);
        if (!hitAThing)
            return false;
        //If the brain (or controller for now) is engaged, start affecting/moving the targeted object.
        BrainAffected theBrainPart = aRaycastReturn.transform.GetComponent<BrainAffected>();
        if (theBrainPart == null)
            return false;
        currentTarget = theBrainPart;
        return true;
    }

    private float ReadBrainlevel()
    {
        //EIGENVALUE MADNESS GOES HERE
        return 1.0f;
    }

    /*private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }*/
}
