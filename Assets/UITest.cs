using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SpellCasting;

public class UITest : MonoBehaviour
{

	[SerializeField]
	private Transform leftLaser;
	[SerializeField]
	private Transform rightLaser;


	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		OVRInput.Update();
		OVRInput.FixedUpdate();

		bool button_a_left = OVRInput.Get(OVRInput.Button.One);
		bool button_a_right = OVRInput.Get(OVRInput.Button.Two);

		if (button_a_left || button_a_right)
		{
			CheckHitObject();
			//Debug.Log($"Start Button Clicked: {CheckHitObject()}");
		}
	}

	private bool CheckHitObject()
	{
		if (leftLaser || rightLaser)
		{
			RaycastHit hitInfo = new RaycastHit();

			Ray leftRay = new Ray(leftLaser.position, leftLaser.forward);
			bool leftHit = Physics.Raycast(leftRay, out hitInfo);
			if (leftHit)
			{
				if (hitInfo.transform.CompareTag("StartBtn"))
				{
					SceneManager.LoadScene("MainGame");
					return true;
				}
				else if (hitInfo.transform.CompareTag("QuitBtn"))
				{
					Application.Quit();
				}
				//return hitInfo.transform;
			}

			Ray rightRay = new Ray(rightLaser.position, rightLaser.forward);
			bool rightHit = /*!leftHit &&*/ Physics.Raycast(rightRay, out hitInfo);
			if (rightHit)
			{
				if (hitInfo.transform.CompareTag("StartBtn"))
				{
					SceneManager.LoadScene("MainGame");
					return true;
				}
				else if (hitInfo.transform.CompareTag("QuitBtn"))
				{
					Application.Quit();
				}
				//return hitInfo.transform;
			}
		}
		return false;
	}
}


