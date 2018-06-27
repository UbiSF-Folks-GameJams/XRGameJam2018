using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFOVOnBrainage : MonoBehaviour
{
    public float mCameraFOVOnMaxBrainage = 120.0f;
    public float mCameraReturnToNormalSpeed = 300.0f;
    public float PowerOfFOVCurve = 3.0f;
    public bool mStopOnMaxReached = true;

    private static bool mDebugCurrentFOV = false;
    private BrainAffected mBrainAffected;
    private float fovLerp;
    private float initialFoV;
    private bool maxReached;

    // Use this for initialization
    void Start () {
        mBrainAffected = GetComponent<BrainAffected>();
        initialFoV = Camera.main.fieldOfView;
        maxReached = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if ( mDebugCurrentFOV )
            Debug.Log("Camera FOV is now = " + Camera.main.fieldOfView);
        //If we don't care about brains and their power to mooooove us, bail.
        if (mBrainAffected == null)
            return;
        //Otherwise, determine our progress of intensity.
        fovLerp = Mathf.Pow(Mathf.Clamp01(mBrainAffected.ActivationLevel), PowerOfFOVCurve);
        if (maxReached && mStopOnMaxReached)
        {
            if (Camera.main.fieldOfView > initialFoV )
                Camera.main.fieldOfView = Mathf.Max( initialFoV, Camera.main.fieldOfView - ( Time.deltaTime * mCameraReturnToNormalSpeed ) );
            else
                Camera.main.fieldOfView = Mathf.Min(initialFoV, Camera.main.fieldOfView + (Time.deltaTime * mCameraReturnToNormalSpeed));
        }
        else
            Camera.main.fieldOfView = Mathf.Lerp(initialFoV, mCameraFOVOnMaxBrainage, fovLerp);
        if (fovLerp >= 1.0f)
            maxReached = true;
    }
}
