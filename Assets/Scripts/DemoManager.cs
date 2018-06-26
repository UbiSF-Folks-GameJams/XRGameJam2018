using UnityEngine;
using System.Collections.Generic;



public class DemoManager : MonoBehaviour
{
    #region fields
    List<DemoFruit> demoFruits = new List<DemoFruit>();
    float popTime = 5;
    #endregion


    DemoFruit CheckForFruitUnderCursor()
	{
		RaycastHit hit;
		if ( Physics.Raycast( Camera.main.ScreenPointToRay( Input.mousePosition ), out hit, 1 << 9 ) )
			return hit.collider.gameObject.GetComponent<DemoFruit>();
		else
			return null;
	}
	
	
	void Start ()
	{


        foreach ( ExplodingFruit explodingFruit in FindObjectsOfType( typeof(ExplodingFruit)))
		{
			DemoFruit demoFruit = explodingFruit.gameObject.AddComponent<DemoFruit>();
			demoFruits.Add ( demoFruit );
			demoFruit.gameObject.layer = 9;
		}
	}

    

    void Update()
	{
        if ( Input.GetMouseButton( 0 ) )
		{
			DemoFruit fruitUnderCursor = CheckForFruitUnderCursor();

            if (fruitUnderCursor != null)
            {
                fruitUnderCursor.Tremble();

                if (fruitUnderCursor.getTimeToPop() >= fruitUnderCursor.getPopTime())
                {
                    fruitUnderCursor.Explode();
                    Debug.Log("POP");
                }
            }
		}
		else if ( Input.GetMouseButtonUp( 0 ))
		{
			DemoFruit fruitUnderCursor = CheckForFruitUnderCursor();
            if (fruitUnderCursor != null)
            {
                fruitUnderCursor.Deactivate();
            }
			
		}
	}
	
	
}
