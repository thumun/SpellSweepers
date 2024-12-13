using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BunnyController : MonoBehaviour
{
	public NavMeshAgent agent;
	[SerializeField]
	private float radius = 10.0f;
	[SerializeField]
	private float moveTimer = 2.0f;

	private bool cannotMove = false; 

	// Start is called before the first frame update
	void Start()
    {
		agent = this.GetComponent<NavMeshAgent>();
	}

    // Update is called once per frame
    void Update()
    {
		if (!cannotMove)
		{
			moveTimer -= Time.deltaTime;
		}

		if (moveTimer <= 0.0f)
		{
			cannotMove = true; 
			RandomDestination();
			moveTimer = 5.0f; 
			cannotMove = false; 
		}
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
