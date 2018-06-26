using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainAffected : MonoBehaviour
{
    public bool AllowBrainMovement = true;
    private float mActivationAmount;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool CanBeMoved
    { get { return AllowBrainMovement; } }
    
    public bool IsActivated
    { get { return mActivationAmount >= 1.0f; } }

    public float ActivationLevel
    {
        get { return mActivationAmount; }
        set
        {
            mActivationAmount = value;
        }
    }
}
