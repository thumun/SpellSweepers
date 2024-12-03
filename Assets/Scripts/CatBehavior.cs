using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI; 
using UnityEngine;
using BTAI;
using UnityEngine.Experimental.XR.Interaction;
using System.Threading;
using UnityEngine.InputSystem.HID;

public class CatBehavior : MonoBehaviour
{
	// might also need loc of player for flee behavior 

	// navmesh -> using this for wander behavior 
	public NavMeshAgent agent;
	public Transform player;
	public float radius; // setting range for random travel 

	public bool slowDown = false; // should be toggled if controlled!!

	// behavior tree root 
	private Root btRoot = BT.Root();
	private float timer; 
	private bool causeMischief = false;
    
	// need to add anim controller for NPC 

	// overarching idea of cat behavior: 
	// wander around always -> via navemesh, set destination and check if path is viable; if path not viable then teleport 
	// occassionally choose to knock over objects that are in range (get distance)
	// water spritz -> add a condition to sequence (flee condition) 
	// anything else?? 

	// Start is called before the first frame update
	void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();

		btRoot.OpenBranch(
			BT.Selector(false).OpenBranch(
				BT.If(() => this.slowDown).OpenBranch(
					BT.RunCoroutine(Flee)
				),
				BT.If(() => this.causeMischief).OpenBranch(
					BT.RunCoroutine(KnockOver)
				)/*,*/
				//BT.RunCoroutine(Wander)
			)
		);

		timer = 0.0f; 
    }

    // Update is called once per frame
    void Update()
    {
        btRoot.Tick();

		// testing purposes --> testing the control spell 
		if (Input.GetKeyDown(KeyCode.W))
		{
			slowDown = true;
			Debug.Log($"Control Cat?: {slowDown}");
		}

		// testing purposes --> mischief 
		if (Input.GetKeyDown(KeyCode.A))
		{
			Mischief();
			Debug.Log($"Mischief: {slowDown}");
		}

		// maybe change this logic
		/*
		if (Mischief() && timer == 0.0f)
		{
			// initiate timer 
			timer += Time.deltaTime;
			if (timer > 5.0f)
			{
				causeMischief = true;
				timer = 0.0f;
			}
		}
		*/

		// if x time goes by, turn off flee behavior 

	}

	// is cat going to knock something over? 
	private void Mischief()
	{
		//causeMischief =  Random.value > 0.7f;
		//Debug.Log($"Michief bool: {causeMischief}");
		causeMischief = true;
	}

	// only happens if mischief true 
	IEnumerator<BTState> KnockOver()
	{
		Debug.Log("Initiate Knock Over");
		causeMischief = false;
		// find object in range with collision (sphere)
		bool objfound = false;
		var colliders = Physics.OverlapSphere(transform.position, 100);

		int rndIndx = -1; 

		while (!objfound)
		{
			rndIndx = Random.Range(0, colliders.Length);

			if (!colliders[rndIndx])
			{
				continue;
			}

			if (colliders[rndIndx].transform.CompareTag("KnockOver"))
			{
				agent.SetDestination(colliders[rndIndx].transform.position);
				// hopefully collider will just knock over? 
				objfound = true;
				yield return BTState.Success;
			}
		}

		yield return BTState.Failure;
	}

	// happens if control spell used on cat 
	IEnumerator<BTState> Flee()
	{
		Debug.Log("Initiate Flee"); 
		slowDown = false;

		if (Vector3.Distance(player.transform.position, agent.transform.position) <= 4.0f)
		{
			RandomDestination();
			yield return BTState.Success;
		}

		yield return BTState.Failure;
	}

    IEnumerator<BTState> Wander()
    {

		if ((agent.remainingDistance <= 0.01f) && !agent.pathPending)
		{
			RandomDestination();
		}

		yield return BTState.Success; // b/c needs to keep going 
    }

	// get rnd pt in traversable area 
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
