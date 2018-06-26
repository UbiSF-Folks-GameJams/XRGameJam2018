using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayResult : MonoBehaviour {

    public Text Alpha;
    public Text Beta;
    public Text Delta;
    public Text Theta;
    public Text Gamma;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Alpha.text = "Alpha: " + EEGDataReceiver.alphaAbsolute;
        Beta.text = "Beta: " + EEGDataReceiver.betaAbsolute;
        Delta.text = "Delta: " + EEGDataReceiver.deltaAbsolute;
        Theta.text = "Theta: " + EEGDataReceiver.thetaAbsolute;
        Gamma.text = "Gamma: " + EEGDataReceiver.gammaAbsolute;

	}
}
