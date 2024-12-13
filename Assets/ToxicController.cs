using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class ToxicController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private float timer = 20.0f; 
    private float bunnyTimer; 
    private bool initiateDustBunny = false;
    private float radius = 10.0f;
    [SerializeField]
    private GameObject bunny; 
	//public NavMeshAgent agent;

	// add dust bunny counter var  

	void Start()
    {
        bunnyTimer = timer;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initiateDustBunny)
        {
			bunnyTimer -= Time.deltaTime;
		}
       
		if (bunnyTimer <= 0.0f)
        {
            initiateDustBunny = true;
            SpawnBunnies();
            bunnyTimer = timer; 

		}

        // check if the y pos and destroy 
        if (this.transform.position.y < -1.30f)
        {
            DestroyObj(); // or just plain destroy? 
        }
    }

    void DestroyObj()
    {
        gameObject.SetActive(false);
    }

    void SpawnBunnies()
    {
		// based on navmesh to make things easier
		Vector3 randomDirection = Random.insideUnitSphere * radius;
		randomDirection += transform.position;
		NavMeshHit hit;
		Vector3 finalPosition = Vector3.zero;

		if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
		{

			finalPosition = hit.position;
		}

		// instantiate a dust bunny at final position  
        Instantiate(bunny, finalPosition, Quaternion.identity);

		initiateDustBunny = false; 
    }
}
