/* 	Breakable Object
	(C) Unluck Software
	http://www.chemicalbliss.com
*/
#pragma warning disable 0618
using UnityEngine;
using System.Collections;


public class BreakableObject:MonoBehaviour{
	public Transform fragments; 					//Place the fractured object
	public float waitForRemoveCollider = 1.0f; 		//Delay before removing collider (negative/zero = never)
	public float waitForRemoveRigid = 10.0f; 		//Delay before removing rigidbody (negative/zero = never)
	public float waitForDestroy = 2.0f; 			//Delay before removing objects (negative/zero = never)
	public float explosiveForce = 350.0f; 			//How much random force applied to each fragment
	public float durability = 5.0f; 				//How much physical force the object can handle before it breaks
	public ParticleSystem breakParticles;			//Assign particle system to apear when object breaks
	public bool mouseClickDestroy;					//Mouse Click breaks the object
	Transform fragmentd;							//Stores the fragmented object after break
	bool broken; 									//Determines if the object has been broken or not 
	
	public void OnCollisionEnter(Collision collision) {
	    if (collision.relativeVelocity.magnitude > durability) {
	        triggerBreak();
	    }
	}
	
	public void OnMouseDown() {
		if(mouseClickDestroy){
			triggerBreak();
		}
	}
	
	public void triggerBreak() {
	    Destroy(transform.FindChild("object").gameObject);
	    Destroy(transform.GetComponent<Collider>());
	    Destroy(transform.GetComponent<Rigidbody>());
	    StartCoroutine(breakObject());
	}
	
	public IEnumerator breakObject() {// breaks object
		
	    if (!broken) {
	    
	    	if(this.GetComponent<AudioSource>() != null){
	    		GetComponent<AudioSource>().Play();
	    	}
	    	
	    	broken = true;
	    	if(breakParticles!=null){
	    		ParticleSystem ps = (ParticleSystem)Instantiate(breakParticles,transform.position, transform.rotation); // adds particle system to stage
	    		Destroy(ps.gameObject, ps.duration); // destroys particle system after duration of particle system
	    	}
	        fragmentd = (Transform)Instantiate(fragments, transform.position, transform.rotation); // adds fragments to stage (!memo:consider adding as disabled on start for improved performance > mem)
	        fragmentd.localScale = transform.localScale; // set size of fragments
	        Transform frags = fragmentd.FindChild("fragments");
	        foreach(Transform child in frags) {
				child.GetComponent<Rigidbody>().AddForce(Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce));
	            child.GetComponent<Rigidbody>().AddTorque(Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce), Random.Range(-explosiveForce, explosiveForce));
	        }
			//transform.position.y -=1000;	// Positions the object out of view to avoid further interaction
	        if (transform.FindChild("particles") != null) transform.FindChild("particles").GetComponent<ParticleEmitter>().emit = false;
	        StartCoroutine(removeColliders());
	        StartCoroutine(removeRigids());
	        if (waitForDestroy > 0) { // destroys fragments after "waitForDestroy" delay
	            foreach(Transform child in transform) {
	   					child.gameObject.SetActive(false);
				}				
	            yield return new WaitForSeconds(waitForDestroy);
	            GameObject.Destroy(fragmentd.gameObject); 
	            GameObject.Destroy(transform.gameObject);
	        }else if (waitForDestroy <=0){ // destroys gameobject
	        	foreach(Transform child in transform) {
	   					child.gameObject.SetActive(false);
				}
	        	yield return new WaitForSeconds(1.0f);
	            GameObject.Destroy(transform.gameObject);
	        }	
	    }
	}
	
	public IEnumerator removeRigids() {// removes rigidbodies from fragments after "waitForRemoveRigid" delay
	    if (waitForRemoveRigid > 0 && waitForRemoveRigid != waitForDestroy) {
	        yield return new WaitForSeconds(waitForRemoveRigid);
	        foreach(Transform child in fragmentd.FindChild("fragments")) {
	            child.GetComponent<Rigidbody>().isKinematic = true;
	        }
	    }
	}
	
	public IEnumerator removeColliders() {// removes colliders from fragments "waitForRemoveCollider" delay
	    if (waitForRemoveCollider > 0){
	        yield return new WaitForSeconds(waitForRemoveCollider);
	        foreach(Transform child in fragmentd.FindChild("fragments")) {
	            child.GetComponent<Collider>().enabled = false;
	        }
	    }
	}
}