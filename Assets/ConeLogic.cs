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
            string count = bunnyCounter.GetComponent<TextMeshPro>().text;
            int countNum = int.Parse(count);
            countNum += 1; 
			bunnyCounter.GetComponent<TextMeshPro>().text = countNum.ToString();
		}
        else if (other.CompareTag("KnockOver"))
        {
            // attach to cone 
            Debug.Log("knock over object attached to vacuum");
            OVRRig.GetComponent<SpellCasting>().vacuumObjects.Add(other.transform);
            other.transform.SetParent(this.transform);
			other.transform.GetComponent<Rigidbody>().isKinematic = false;
			other.transform.GetComponent<Rigidbody>().useGravity = false;
            other.gameObject.tag = "Untagged";
		}
	}
}
