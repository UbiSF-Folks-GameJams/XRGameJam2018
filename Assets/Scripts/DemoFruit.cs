using UnityEngine;
using System.Collections;



public class DemoFruit : MonoBehaviour
{
	#region fields
	
		enum State {
			ShowingFruit,
            TrembleFruit,
			ExplodingFruit
		}
		State state = State.ShowingFruit;
	
		Vector3 rotationAxis;
		float kRotationSpeed;
        float timeToPop;
        float popTime = 12000;
        ExplodingFruit explodingFruit;


    #endregion

    public float getTimeToPop()
    {
        return timeToPop;
    }

    public float getPopTime()
    {
        return popTime;
    }

    void Awake()
	{
		explodingFruit = GetComponent<ExplodingFruit>();
        rotationAxis = UnityEngine.Random.onUnitSphere;
        state = State.ShowingFruit;
    }

    public void Tremble()
    {
        if (state != State.ShowingFruit) { 
            return;
        }
        state = State.TrembleFruit;
    }

    public void Deactivate()
    {
        timeToPop = 0;
        kRotationSpeed = 0;
        state = State.ShowingFruit;
    }

    public void Explode()
	{
		if ( state != State.TrembleFruit )
			return;

		explodingFruit.Explode();
		state = State.ExplodingFruit;
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
            Debug.Log("Tremble Update");
            timeToPop += 60;
            kRotationSpeed = 1 * timeToPop;
            transform.Rotate(rotationAxis, Time.deltaTime * kRotationSpeed);
        }
	}

}
