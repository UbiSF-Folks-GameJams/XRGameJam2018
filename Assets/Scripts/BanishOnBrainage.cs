using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BanishOnBrainage : MonoBehaviour {

    private BrainAffected mBrainAffected;

    // Use this for initialization
    void Start () {
        mBrainAffected = GetComponent<BrainAffected>();
    }
	
	// Update is called once per frame
	void Update () {
        if (mBrainAffected.IsActivated)
            Destroy(gameObject);
	}
}
