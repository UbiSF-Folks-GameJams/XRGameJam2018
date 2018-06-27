using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeOnBrainage : MonoBehaviour
{
    public float ShakeAmountAtStart = 300.0f;
    public float ShakeAmountAtEnd = 1200.0f;
    public float ShakeChangeFrequencyAtStart = 30.0f;
    public float ShakeChangeFrequencyAtEnd = 1.0f;
    public float PowerOfShakeCurve = 2.0f;

    private BrainAffected mBrainAffected;
    private float shakeLerp;
    private float shakeTimer;
    private Vector3 shakeAxis;

    public void Awake()
    {
        ChangeShakeDirection();
        shakeTimer = 0.0f;
    }

    // Use this for initialization
    void Start ()
    {
        mBrainAffected = GetComponent<BrainAffected>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //If we don't care about brains and their power to mooooove us, bail.
        if (mBrainAffected == null)
            return;
        //Otherwise, determine our progress of intensity.
        shakeLerp = Mathf.Pow(Mathf.Clamp01(mBrainAffected.ActivationLevel), PowerOfShakeCurve);
        //Decrement our timer and change timing/direction if needed.
        shakeTimer = Mathf.Max(0.0f, shakeTimer - Time.deltaTime);
        if (shakeTimer <= 0.0f)
            ResetShake();
        //Finally, rotate like you mean it!
        float shakeAmount = Mathf.Lerp(ShakeAmountAtStart, ShakeAmountAtEnd, shakeLerp);
        transform.Rotate(shakeAxis, Time.deltaTime * shakeAmount);
    }

    private void ChangeShakeDirection()
    {
        shakeAxis = UnityEngine.Random.onUnitSphere;
    }

    private void ResetShake()
    {
        shakeTimer = 1.0f / Mathf.Max( 0.0001f, Mathf.Lerp(ShakeChangeFrequencyAtStart, ShakeChangeFrequencyAtEnd, shakeLerp ) );
        ChangeShakeDirection();
    }
}
