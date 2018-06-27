using UnityEngine;
using System.Collections;



public class DemoFruit : MonoBehaviour
{
    #region fields

    enum State
    {
        ShowingFruit,
        TrembleFruit,
        ExplodingFruit
    }
    State state = State.ShowingFruit;

    Vector3 rotationAxis;
    float kRotationSpeed;
    float popLevel;
    public float popRate = 1;
    public float popExpon = 2;
    public float trembleRate = 1;
    public float popLimit = 10;
    ExplodingFruit explodingFruit;


    #endregion

    /*public float getTimeToPop()
    {
        return timeToPop;
    }

    public float getPopTime()
    {
        return popTime;
    }*/

    void Awake()
    {
        explodingFruit = GetComponent<ExplodingFruit>();
        rotationAxis = UnityEngine.Random.onUnitSphere;
        state = State.ShowingFruit;
        //Debug.Log("Awake!");
    }

    public void Tremble()
    {
        if (state == State.TrembleFruit)
        {
            return;
        }
        state = State.TrembleFruit;
        Debug.Log("Tremble State");
    }

    public void Deactivate()
    {
        //transform.Rotate(rotationAxis, Time.deltaTime * kRotationSpeed);
        //popLevel = 0;
        //kRotationSpeed = 0;
        state = State.ShowingFruit;
        //Debug.Log("Deactivated; Showing Fruit");
    }

    public void Explode()
    {
        if (state != State.TrembleFruit)
        {
            return;
        }

        explodingFruit.Explode();
        state = State.ExplodingFruit;
        Debug.Log("Explode!");
        //for demo purposes, it reappears after 4 seconds
        Invoke("Reset", 4);
    }

    void Reset()
    {
        explodingFruit.Reset();
        kRotationSpeed = 0;
        state = State.ShowingFruit;
    }


    void Update()
    {
        if (state == State.TrembleFruit)
        {
            if ((popLimit - popLevel) == 0)
            {
                explodingFruit.Explode();
                Debug.Log("POP");
            }
            else
            {
                popLevel = (popLevel + popRate);
                kRotationSpeed = ((Mathf.Pow((popLevel / popLimit), popExpon) * trembleRate));
                transform.Rotate(rotationAxis, Time.deltaTime * kRotationSpeed);
                //Debug.Log("Tremble Update");
            }

        }
        else if (state == State.ShowingFruit)
        {
            if (kRotationSpeed > 0)
            {
                kRotationSpeed = ((Mathf.Pow((popLimit / popLevel), popExpon)));
                transform.Rotate(rotationAxis, Time.deltaTime * kRotationSpeed);
                Debug.Log("Showing Update");
            }
            else
            {
                kRotationSpeed = 0;
            }
        }
    }
}
