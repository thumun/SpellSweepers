using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConeLogic : MonoBehaviour
{

    public GameObject OVRRig;
    public GameObject bunnyCounter; 

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DustBunny"))
        {
            other.gameObject.SetActive(false);
            // Add to dust bunny counter?
			Debug.Log("dust bunny captured");
            GameManager.instance.CaptureDustBunny();
		}
        else if (other.CompareTag("Fractured")){
			other.gameObject.SetActive(false);
            Debug.Log("Cauldron cleaned up");
		}
        else if (other.CompareTag("KnockOver"))
        {
            // attach to cone 
            Debug.Log("knock over object attached to vacuum");
            if (!OVRRig.GetComponent<SpellCasting>().vacuumObjects.Contains(other.transform))
            {
				OVRRig.GetComponent<SpellCasting>().vacuumObjects.Add(other.transform);
				other.transform.SetParent(this.transform);
				other.transform.GetComponent<Rigidbody>().isKinematic = false;
				other.transform.GetComponent<Rigidbody>().useGravity = false;
				other.gameObject.tag = "Untagged";
			}
			
		}
	}
}
