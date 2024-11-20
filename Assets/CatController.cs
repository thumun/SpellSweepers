using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatController : MonoBehaviour
{

    public Camera cam; 
    public NavMeshAgent agent;
    public float radius; // setting range for random travel 

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();   
    }

    // Update is called once per frame
    void Update()
    {
        // 0.01 = distance threshold 
		if ((agent.remainingDistance <= 0.01f) && !agent.pathPending)
		{
			RandomDestination();
		}

        /*
		if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit; 

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }
        */

    }

    public void RandomDestination()
    {
		Vector3 randomDirection = Random.insideUnitSphere * radius;
		randomDirection += transform.position;
		NavMeshHit hit;
		Vector3 finalPosition = Vector3.zero;
		if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
		{
			finalPosition = hit.position;
		}

	    agent.SetDestination(finalPosition);
	}
}
