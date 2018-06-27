/*
	Created by Carl Emil Carlsen.
	Copyright 2016 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

using UnityEngine;
using OscSimpl.Examples;

    public class EEGDataReceiver : MonoBehaviour
    {
        public OscIn oscIn;

        public static float alphaAbsolute;
        public static float betaAbsolute;
        public static float deltaAbsolute;
        public static float thetaAbsolute;
        public static float gammaAbsolute;


        void Start()
        {
            // Ensure that we have a OscIn component.
            if (!oscIn) oscIn = gameObject.AddComponent<OscIn>();

            // Start receiving from unicast and broadcast sources on port 7000.
            oscIn.Open(7000);
        }


        void OnEnable()
        {
            oscIn.Map("/elements/alpha_absolute", OnAlpha);
            oscIn.Map("/elements/beta_absolute", OnBeta);
            oscIn.Map("/elements/delta_absolute", OnDelta);
            oscIn.Map("/elements/theta_absolute", OnTheta);
            oscIn.Map("/elements/gamma_absolute", OnGamma);

        }


        void OnDisable()
        {
            oscIn.Unmap(OnAlpha);
            oscIn.Unmap(OnBeta);
            oscIn.Unmap(OnDelta);
            oscIn.Unmap(OnTheta);
            oscIn.Unmap(OnGamma);
        }


        void OnAlpha(OscMessage message)
        {
          //  Debug.Log(message);
            float i = 0;
            foreach (object a in message.args)
            {

                if (a is double)
                {
                    float b = float.Parse(a.ToString());
                    if (b != 0 && a.ToString() != "-Infinity") i = b + i;
                }


            }
           // i = i / 4;
            alphaAbsolute = i;
            Debug.Log("Alpha: " + i);
        }

        void OnBeta(OscMessage message)
        {
          //  Debug.Log(message);
            float i = 0;
            foreach (object a in message.args)
            {

                if (a is double)
                {
                    float b = float.Parse(a.ToString());
                    if (b != 0 && a.ToString()!= "-Infinity") i = b + i;
                }


            }
          //  i = i / 4;
        betaAbsolute = i;
            Debug.Log("Beta" + i);
        }

        void OnDelta(OscMessage message)
        {
          //  Debug.Log(message);
            float i = 0;
            foreach (object a in message.args)
            {
       
                if (a is double)
                {
                    float b = float.Parse(a.ToString());
                        if (b != 0 && a.ToString() != "-Infinity") i = b + i;
                }
                
                
            }
        //    i = i / 4;
        deltaAbsolute = i;
            Debug.Log("Delta" + i);


        }

        void OnTheta(OscMessage message)
        {
           // Debug.Log(message);
            float i = 0;
            foreach (object a in message.args)
            {

                if (a is double)
                {
                    float b = float.Parse(a.ToString());
                    if (b != 0 && a.ToString() != "-Infinity") i = b + i;
                }


            }
         //   i = i / 4;
        thetaAbsolute = i;
            Debug.Log("Theta: " + i);
        }

        void OnGamma(OscMessage message)
        {
            //Debug.Log(message);
            float i = 0;
            foreach (object a in message.args)
            {

                if (a is double)
                {
                    float b = float.Parse(a.ToString());
                    if (b != 0 && a.ToString() != "-Infinity") i = b + i;
                }


            }
        //    i = i / 4;
        gammaAbsolute = i;
            Debug.Log("Gamma: " +i);


        }
    }
