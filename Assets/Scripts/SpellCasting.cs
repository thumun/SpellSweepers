/*
 * MiVRy - 3D gesture recognition library plug-in for Unity.
 * Version 2.11
 * Copyright (c) 2024 MARUI-PlugIn (inc.)
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY 
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine.UI;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
using TMPro; 
#if UNITY_ANDROID
using UnityEngine.Networking;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Linq.Expressions;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System.Security;
using UnityEditor.Search;
using UnityEditor.PackageManager;
using TMPro;
using UnityEngine.SceneManagement;


#endif

public class SpellCasting : MonoBehaviour
{
    // The file from which to load gestures on startup.
    // For example: "Assets/GestureRecognition/sample_gestures.dat"
    [SerializeField] private string LoadGesturesFile;

    // File where to save recorded gestures.
    // For example: "Assets/GestureRecognition/my_custom_gestures.dat"
    [SerializeField] private string SaveGesturesFile;

    // Name (ID) of your MiVRy license.
    // Leave emtpy for free version of MiVRy.
    [SerializeField] public string LicenseName;

    // License key of your MiVRy license.
    // Leave emtpy for free version of MiVRy.
    [SerializeField] public string LicenseKey;

    // NEHA'S VARIABLES! 
    //***********************************************************
    public GameObject startest;
    [SerializeField]
    private Transform leftLaser;
	[SerializeField]
	private Transform rightLaser;

    public Transform selectedObject;
    private Color originalObjColor;
    private Color selectedColor = new Color(255.0f/255.0f, 234.0f/255.0f, 0);
    private float selectTimer = 1.0f;

    // for control spell
    private bool ctrlActive = false;
    // for slow down spell 
    private bool slowActive = false;
    // for vacuum spell 
    private bool vacuumActive = false; 

    private bool itemDropped = false; // hate this variable

    public List<Transform> vacuumObjects = new List<Transform>();

    private List<Vector3> trackingPos = new List<Vector3>();
    private float velocity = 70f; 

    [SerializeField]
    private GameObject active_controller_move = null;
    private GameObject active_controller_cone = null; 

    public GameObject RightHandRef = null;
    public GameObject RightPtrRef = null;
    public GameObject RightMoveRef = null; 

    public GameObject LeftHandRef = null;
	public GameObject LeftPtrRef = null;
	public GameObject LeftMoveRef = null;

    public GameObject coneRT = null;
    public GameObject coneLT = null;

    private float textTimer = 0.0f;

    [SerializeField]
    private GameObject centerEye = null; 

    internal enum SPELLS { NONE, CONTROL, SLOWDOWN, VACUUM }
    internal SPELLS currentSpell = SPELLS.NONE;
	//************************************************************

	// The gesture recognition object:
	// You can have as many of these as you want simultaneously.
	private GestureRecognition gr = new GestureRecognition();

    // The text field to display instructions.
	[SerializeField]
	private TextMeshPro HUDText;

	// The game object associated with the currently active controller (if any):
	[SerializeField]
	private GameObject active_controller = null;

	// The pointing tip of the active_controller (used for visualization).
	[SerializeField]
	private GameObject active_controller_pointer = null;

    // The game object associated with the currently active controller (if any):
    private bool button_a_pressed = false;

    // ID of the gesture currently being recorded,
    // or: -1 if not currently recording a new gesture,
    // or: -2 if the AI is currently trying to learn to identify gestures
    // or: -3 if the AI has recently finished learning to identify gestures
    private int recording_gesture = -1; 

    // Last reported recognition performance (during training).
    // 0 = 0% correctly recognized, 1 = 100% correctly recognized.
    private double last_performance_report = 0; 

    // Temporary storage for objects to display the gesture stroke.
    List<string> stroke = new List<string>(); 

    // Temporary counter variable when creating objects for the stroke display:
    int stroke_index = 0; 

    // List of Objects created with gestures:
    List<GameObject> created_objects = new List<GameObject>();

    // Handle to this object/script instance, so that callbacks from the plug-in arrive at the correct instance.
    GCHandle me;

    // Database of all controller models in the scene
    private Dictionary<string, GameObject> controller_gameobjs = new Dictionary<string, GameObject>();

    // Helper function to set the currently active controller model
    void SetActiveControllerModel(string side, string type)
    {
        GameObject controller_oculus = controller_gameobjs["controller_oculus_" + side];

        controller_oculus.SetActive(false);
    
        if (type.Contains("Oculus")) // "Oculus Touch Controller OpenXR"
        {
            controller_oculus.SetActive(true);
        }
        else
        {
            Debug.Log("Error with type of headset");
        }
    }

    // Helper function to handle new VR controllers being detected.
    void DeviceConnected(InputDevice device)
    {
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
        {
            SetActiveControllerModel("left", device.name);
        }
        else if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
        {
            SetActiveControllerModel("right", device.name);
        }
    }

    // Initialization:
    void Start ()
    {
        //HUDText = SpellUI.GetComponent<TextMeshProUGUI>();

        // Set the welcome message.
        /*
        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        HUDText.text = "Welcome to MARUI Gesture Plug-in!\n"
                      + "Press the trigger to draw a gesture. Available gestures:\n"
                      + "1 - a circle/ring (creates a cylinder)\n"
                      + "2 - swipe left/right (rotate object)\n"
                      + "3 - shake (delete object)\n"
                      + "4 - draw a sword from your hip,\nhold it over your head (magic)\n"
                      + "or: press 'A'/'X'/Menu button\nto create new gesture.";
        */

        me = GCHandle.Alloc(this);

        if (this.LicenseName != null && this.LicenseKey != null && this.LicenseName.Length > 0)
        {
            if (this.gr.activateLicense(this.LicenseName, this.LicenseKey) != 0)
            {
                Debug.LogError("Failed to activate license");
            }
        }

        // Load the set of gestures.
        if (LoadGesturesFile == null)
        {
            LoadGesturesFile = "Samples/Sample_OneHanded_Gestures.dat";
        }

        // Find the location for the gesture database (.dat) file
#if UNITY_EDITOR
        // When running the scene inside the Unity editor,
        // we can just load the file from the Assets/ folder:
        string gesture_file_path = "Assets/GestureRecognition";
#elif UNITY_ANDROID
        // On android, the file is in the .apk,
        // so we need to first "download" it to the apps' cache folder.
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        string gesture_file_path = activity.Call <AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
        UnityWebRequest request = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + LoadGesturesFile);
        request.SendWebRequest();
        while (!request.isDone) {
            // wait for file extraction to finish
        }
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            HUDText.text = "Failed to extract sample gesture database file from apk.";
            return;
        }
        string path = gesture_file_path + "/" + LoadGesturesFile;
        try {
            Directory.CreateDirectory(path);
            Directory.Delete(path);
        } catch (Exception) { }
        try {
            File.WriteAllBytes(path, request.downloadHandler.data);
        } catch (Exception e) {
            HUDText.text = "Exception writing temporary file: " + e.ToString();
            return;
        }
#else
        // This will be the case when exporting a stand-alone PC app.
        // In this case, we can load the gesture database file from the streamingAssets folder.
        string gesture_file_path = Application.streamingAssetsPath;
#endif
        gesture_file_path = gesture_file_path + "/" + LoadGesturesFile;
        int ret = gr.loadFromFile(gesture_file_path);
        if (ret != 0)
        {
            byte[] file_contents = File.ReadAllBytes(gesture_file_path);
            if (file_contents == null || file_contents.Length == 0)
            {
                //HUDText.text = $"Could not find gesture database file ({gesture_file_path}).";
                return;
            }
            ret = gr.loadFromBuffer(file_contents);
            if (ret != 0)
            {
                //HUDText.text = $"Failed to load sample gesture database file ({ret}).";
                return;
            }
        }

        // Reset the skybox tint color
        RenderSettings.skybox.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f, 1.0f));

        controller_gameobjs["controller_oculus_left"] = GameObject.Find("controller_oculus_left");
        controller_gameobjs["controller_oculus_right"] = GameObject.Find("controller_oculus_right");

        controller_gameobjs["controller_oculus_left"].SetActive(false);
        controller_gameobjs["controller_oculus_right"].SetActive(false);

        InputDevices.deviceConnected += DeviceConnected;
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        foreach (var device in devices)
            DeviceConnected(device);

        GameObject star = GameObject.Find("star");
        star.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
        GameObject controller_dummy = GameObject.Find("controller_dummy");
        controller_dummy.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
    }
    

    // Update:
    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            Application.Quit();
        }
		
		OVRInput.Update();
		OVRInput.FixedUpdate();

		float trigger_left = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ? 1.0f : 0.0f;
		float trigger_right = OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) ? 1.0f : 0.0f;

#else
        float escape = Input.GetAxis("escape");
        if (escape > 0.0f)
        {
            Application.Quit();
        }
        float trigger_left = Input.GetAxis("LeftControllerTrigger");
        float trigger_right = Input.GetAxis("RightControllerTrigger");
#endif

		bool button_a_left = OVRInput.Get(OVRInput.Button.One);
        bool button_a_right = OVRInput.Get(OVRInput.Button.Two);

        if (button_a_left || button_a_right)
        {
			bool debugHit = checkUIHit();
		}

		// if button pressed -> select object (if object exists with knock over tag) 
		if ((button_a_left || button_a_right) && currentSpell != SPELLS.NONE)
        {
            if (selectTimer >= 0.1f)
            {
                if (selectedObject != null && currentSpell == SPELLS.CONTROL)
                {
                    ctrlActive = false;
                    currentSpell = SPELLS.NONE;
                    itemDropped = true;
                }

                else if (currentSpell == SPELLS.VACUUM)
                {
                    // vacuum drop all items 
                    UnVacuum();
                    vacuumActive = false;
                    currentSpell = SPELLS.NONE;
                }
                else
                {
                    bool debugHit = CheckHitObject();
                    if (debugHit)
                    {
                        Debug.Log($"item selected: {selectedObject.name}");
                    }
                    else
                    {
                        Debug.Log("nothing selected");
                        // deactivate spell 
                        ctrlActive = false;
                        vacuumActive = false;
                        slowActive = false;
                        currentSpell = SPELLS.NONE;
                    }
                }

				selectTimer = 0.0f;
			}

			selectTimer += Time.deltaTime;
			
        }

        if (currentSpell == SPELLS.CONTROL && selectedObject != null)
        {
			ControlSpell();
		}

        else if (currentSpell == SPELLS.SLOWDOWN && selectedObject != null)
        {
            SlowDown();
		}

        else if (currentSpell == SPELLS.NONE && selectedObject != null && itemDropped)
        {
            ItemThrow();

		}

        VacuumSpell();

        
        if (textTimer > 0.0f)
        {
            textTimer -= Time.deltaTime;
        }
        else if (textTimer <= 0.0f)
        {
            textTimer = 0.0f;
            HUDText.gameObject.SetActive(false);
        }
        

		GameObject hmd = Camera.main.gameObject; // alternative: GameObject.Find("Main Camera");
        Vector3 hmd_p = hmd.transform.position;
        Quaternion hmd_q = hmd.transform.rotation;

        // If the user is not yet dragging (pressing the trigger) on either controller, he hasn't started a gesture yet.
        if (active_controller == null) {
            // If the user presses either controller's trigger, we start a new gesture.
            if (trigger_right > 0.0f) {
                active_controller = RightHandRef;
                active_controller_pointer = RightPtrRef;
                active_controller_move = RightMoveRef;
                active_controller_cone = coneRT;

			} else if (trigger_left > 0.0f) {
				active_controller = LeftHandRef;
				active_controller_pointer = LeftPtrRef;
				active_controller_move = LeftMoveRef;
                active_controller_cone = coneLT;

			} else {
                // If we arrive here, the user is pressing neither controller's trigger:
                // nothing to do.
                return;
            }
            // If we arrive here: either trigger was pressed, so we start the gesture.
            gr.startStroke(hmd_p, hmd_q, recording_gesture);
        }

        // If we arrive here, the user is currently dragging with one of the controllers.
        // Check if the user is still dragging or if he let go of the trigger button.
        if (trigger_left > 0.0f || trigger_right > 0.0f) {
            // The user is still dragging with the controller: continue the gesture.
            gr.updateHeadPosition(hmd_p, hmd_q);
            Vector3 p = active_controller.transform.position;
            Quaternion q = active_controller.transform.rotation;
            gr.contdStrokeQ(p, q);
            // Show the stroke by instatiating new objects
            p = active_controller_pointer.transform.position;
            GameObject star_instance = Instantiate(startest);
            GameObject star = new GameObject("stroke_" + stroke_index++);
            star_instance.name = star.name + "_instance";
            star_instance.transform.SetParent(star.transform, false);
            System.Random random = new System.Random();
            star.transform.position = new Vector3(p.x + (float)random.NextDouble() / 80, p.y + (float)random.NextDouble() / 80, p.z + (float)random.NextDouble() / 80);
            star.transform.rotation = new Quaternion((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f).normalized;
            //star.transform.rotation.Normalize();
            float star_scale = (float)random.NextDouble() + 0.3f;
            star.transform.localScale = new Vector3(star_scale, star_scale, star_scale);
            stroke.Add(star.name);
            return;
        }
        // else: if we arrive here, the user let go of the trigger, ending a gesture.
        active_controller = null;

        // Delete the objects that we used to display the gesture.
        foreach (string star in stroke) {
            GameObject star_object = GameObject.Find(star);
            if (star_object != null) {
                Destroy(star_object);
            }
        }
        stroke.Clear();
        stroke_index = 0;

        double similarity = 0; // This will receive a value of how similar the performed gesture was to previous recordings.
        Vector3 pos = Vector3.zero; // This will receive the position where the gesture was performed.
        double scale = 0; // This will receive the scale at which the gesture was performed.
        Vector3 dir0 = Vector3.zero; // This will receive the primary direction in which the gesture was performed (greatest expansion).
        Vector3 dir1 = Vector3.zero; // This will receive the secondary direction of the gesture.
        Vector3 dir2 = Vector3.zero; // This will receive the minor direction of the gesture (direction of smallest expansion).
        int gesture_id = gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);

        // if (similarity < ???) {
        //     ...maybe this is not the gesture I was looking for...
        // }

        // else: if we arrive here, we're not recording new sampled for custom gestures,
        // but instead have identified a new gesture.
        // Perform the action associated with that gesture.

        if (gesture_id < 0)
        {
            // Error trying to identify any gesture
            HUDText.text = "Failed to identify gesture.";
        }
        else if (gesture_id == 0)
        {
            // "loop"-gesture: create cylinder
            SpellcastTxt("Identified Control Spell");
            //HUDText.text = "Identified Control Spell";
            if (GameManager.instance.TryCastSpell(0)) {
                currentSpell = SPELLS.CONTROL;
                //ctrlActive = !ctrlActive;
                ctrlActive = true;
            }
		}
        else if (gesture_id == 1 || gesture_id == 2)
        {
            // "swipe left"-gesture: rotate left
            SpellcastTxt("Identified Vaccuum Spell");
            //HUDText.text = "Identified Vaccuum Spell";
            if (GameManager.instance.TryCastSpell(1)) {
                currentSpell = SPELLS.VACUUM;
                //vacuumActive = !vacuumActive;
                vacuumActive = true;
            }
		}
        else if (gesture_id == 4 || gesture_id == 3)
        {
            SpellcastTxt("Identified Slow Down Spell");

			//HUDText.text = "Identified Slow Down Spell";
            if (GameManager.instance.TryCastSpell(2)) {
                currentSpell = SPELLS.SLOWDOWN;
                //slowActive = !slowActive; 
                slowActive = true;
            }
		}
        else
        {
            // Other ID: one of the user-registered gestures:
            //HUDText.text = "Unknown Gesture: " + (gesture_id);
            SpellcastTxt("Unknown Spell");
        }
    }

    // might need to check the tag for each selected obj 

    void ControlSpell()
    {
        if (ctrlActive)
        {
			// check if item was selected 
			if (selectedObject != null && (selectedObject.CompareTag("KnockOver") || selectedObject.CompareTag("ToxicBottle")))
			{
				/*
				// get active controller item 
				// obj position = the test position 
				//selectedObject.transform.position = active_controller_move.transform.position;
				selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, active_controller_move.transform.position, Time.deltaTime);
                selectedObject.GetComponent<Rigidbody>().isKinematic = false;
                selectedObject.GetComponent<Rigidbody>().useGravity = false; 
                */

                selectedObject.GetComponent<Rigidbody>().useGravity = false;    
				selectedObject.position = active_controller_move.transform.position;
                selectedObject.rotation = active_controller_move.transform.rotation;
                
                if (trackingPos.Count > 15)
                {
                    trackingPos.RemoveAt(0);
                }
                trackingPos.Add(selectedObject.transform.position);

				Debug.Log("Moving");
			}
			else
			{
				Debug.Log("Need to select an item");
			}
		}
    }

    void ItemThrow()
    {
		if (itemDropped)
		{
			//selectedObject.GetComponent<Rigidbody>().useGravity = true;

			if (trackingPos.Count > 0)
			{
				Vector3 dir = trackingPos[trackingPos.Count - 1] - trackingPos[0];
				selectedObject.GetComponent<Rigidbody>().AddForce(dir * velocity);
				selectedObject.GetComponent<Rigidbody>().useGravity = true;
				selectedObject.GetComponent<Rigidbody>().isKinematic = false;
				trackingPos.Clear();

				selectedObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = originalObjColor;

				selectedObject = null;

				itemDropped = false;
			}
		}
	}

    void VacuumSpell()
    {
        if (vacuumActive)
        {
			// check if spell is active 
			active_controller_cone.SetActive(true);
			//coneRT.SetActive(vacuumActive);

			if (vacuumObjects.Count > 0)
			{
				foreach (Transform item in vacuumObjects)
				{
					item.position = coneRT.transform.position;
					item.rotation = coneRT.transform.rotation;
				}
			}
		}
        
	}

    void UnVacuum()
    {
        foreach (var item in vacuumObjects)
        {
            GameObject lvl = GameObject.Find("Level");
			item.transform.SetParent(lvl.transform);
			//item.transform.GetComponent<Rigidbody>().isKinematic = true;
			//item.GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, 0) /** velocity*/);
			item.transform.GetComponent<Rigidbody>().useGravity = true;
			item.gameObject.tag = "KnockOver";
		}
        vacuumObjects.Clear();
        active_controller_cone.SetActive(false);
    }

    void SlowDown()
	{
		// one time use spell 
        if (slowActive)
        {
			if (selectedObject != null && selectedObject.CompareTag("Cauldron"))
			{
                // lower time 
                //CauldronController cauldron = selectedObject.GetComponent<CauldronController>();
                CauldronController.instance.timeToExplode += 500;
                //cauldron.timeToExplode -= 500;
				slowActive = false;
				//selectedObject.gameObject.GetComponent<Renderer>().material.color = originalObjColor;
				selectedObject = null;
                currentSpell = SPELLS.NONE;
			}
            else if (selectedObject != null && selectedObject.CompareTag("Cat"))
            {
                // activate flee condition 
                CatBehavior cat = selectedObject.GetComponent<CatBehavior>();
                cat.slowDown = true; 
                slowActive = false;
				//selectedObject.gameObject.GetComponent<Renderer>().material.color = originalObjColor;
				selectedObject = null;
                currentSpell = SPELLS.NONE;
			}
		}
	}
    
    // this is called when the player clicks A or X on the controller 
    // this will select an object 
	private bool CheckHitObject()
	{
		if (leftLaser || rightLaser)
		{
		    RaycastHit hitInfo = new RaycastHit();

			Ray leftRay = new Ray(leftLaser.position, leftLaser.forward);
			bool leftHit = Physics.Raycast(leftRay, out hitInfo);
			if (leftHit)
			{
                if (hitInfo.transform.CompareTag("KnockOver") || hitInfo.transform.CompareTag("ToxicBottle") || hitInfo.transform.CompareTag("Cauldron") || hitInfo.transform.CompareTag("Cat"))
                {
					// select object 
					selectedObject = hitInfo.transform;

					originalObjColor = selectedObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color;
					selectedObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = selectedColor;

					return true;
				}
				else if (hitInfo.transform.CompareTag("Cauldron") || hitInfo.transform.CompareTag("Cat"))
				{
					selectedObject = hitInfo.transform;
					return true;
				}
				else if (hitInfo.transform.CompareTag("StartBtn"))
                {
					SceneManager.LoadScene(0);
					return false;
                }
				//return hitInfo.transform;
			}
            
			Ray rightRay = new Ray(rightLaser.position, rightLaser.forward);
			bool rightHit = /*!leftHit &&*/ Physics.Raycast(rightRay, out hitInfo);
			if (rightHit)
			{
				if (hitInfo.transform.CompareTag("KnockOver") || hitInfo.transform.CompareTag("ToxicBottle"))
				{
					// select object 
					selectedObject = hitInfo.transform;

					originalObjColor = selectedObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color;
					selectedObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = selectedColor;

					return true; 
				}
                else if (hitInfo.transform.CompareTag("Cauldron") || hitInfo.transform.CompareTag("Cat"))
                {
					selectedObject = hitInfo.transform;
                    return true;
				}
                else if (hitInfo.transform.CompareTag("StartBtn"))
                {
					SceneManager.LoadScene(0);
					return false;
                }
				//return hitInfo.transform;
			}
		}
		return false;
	}

    private bool checkUIHit()
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
					SceneManager.LoadScene(0);
					return false;
				}
				//return hitInfo.transform;
			}

			Ray rightRay = new Ray(rightLaser.position, rightLaser.forward);
			bool rightHit = /*!leftHit &&*/ Physics.Raycast(rightRay, out hitInfo);
			if (rightHit)
			{
				if (hitInfo.transform.CompareTag("StartBtn"))
				{
					SceneManager.LoadScene(0);
					return false;
				}
				//return hitInfo.transform;
			}
		}
		return false;
	}

    void SpellcastTxt(string changetxt)
    {
		//HUDText.transform.position = this.transform.forward + this.transform.position;
		HUDText.transform.position = centerEye.transform.forward + centerEye.transform.position;
        HUDText.transform.rotation = centerEye.transform.rotation; 
		HUDText.text = changetxt;
        HUDText.gameObject.SetActive(true);
        textTimer = 1.0f;
	}
}
