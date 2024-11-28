using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CauldronController : MonoBehaviour
{
    public static CauldronController instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<CauldronController>();
            }
            return m_instance;
        }
    }

    private static CauldronController m_instance;

    public GameObject cauldronWhole;
    public GameObject cauldronFractured;

    public GameObject liquidSurface;
    public List<GameObject> floatingObjects;
    private List<string> requiredRecipe = new List<string>();
    private List<string> currentFloaters = new List<string>();

    public GameObject liquidDrop;
    public GameObject[] liquidDropSpawnLocs;

    private float liquidSurfaceHeight;

    private int timeToExplode;
    private int explosionTime = 20000;
    // min scale = 0.13, max scale = 0.53;
    private float scaleDiff = 0.4f;

    private bool shouldExplode = false;
    private bool exploded = false;
    private bool solved = false;

    public float explosionForce;
    public float explosionRadius;
    public float upwardsModifier;

    // Start is called before the first frame update
    void Start()
    {
        cauldronWhole.SetActive(true);
        cauldronFractured.SetActive(false);
        liquidSurface.SetActive(true);

        liquidSurfaceHeight = liquidSurface.transform.position.y;

        requiredRecipe.Add("IngRed");
        requiredRecipe.Add("IngBlue");
    }

    // Update is called once per frame
    void Update()
    {
        if (exploded || solved) return;

        IncrementTimeToExplode(1);
        if (timeToExplode >= explosionTime) {
            shouldExplode = true;
        }

        if (shouldExplode && !exploded) {
            Explode();
        }
    }

    public float GetSurfaceHeight() {
        return liquidSurfaceHeight;
    }

    public void AddNewIngredient(string ingredient) {
        if (exploded || solved) return;
        
        currentFloaters.Add(ingredient);
        if (!requiredRecipe.Contains(ingredient)) {
            // Accelerate time to explode
            IncrementTimeToExplode(1000);
        } else {
            bool allContains = true;
            foreach (string ing in requiredRecipe) {
                if (!currentFloaters.Contains(ing)) {
                    allContains = false;
                    break;
                }
            }
            if (allContains) {
                solved = true;
            }
        }
    }

    public void IncrementTimeToExplode(int t) {
        timeToExplode = Mathf.Min(explosionTime, timeToExplode + t);
        liquidSurface.transform.localScale += new Vector3(0, 0, 1) * scaleDiff / (float)explosionTime;
    }

    void Explode()
    {
        cauldronWhole.SetActive(false);
        cauldronFractured.SetActive(true);
        liquidSurface.SetActive(false);

        foreach (Transform child in cauldronFractured.transform) {
            Rigidbody rb = child.gameObject.GetComponent<Rigidbody>();
            rb.AddExplosionForce(explosionForce, cauldronFractured.transform.position, explosionRadius, upwardsModifier);
        }

        foreach(GameObject loc in liquidDropSpawnLocs) {
            GameObject drop = Instantiate(liquidDrop, loc.transform.position, new Quaternion(0, 0, 0, 0));
            Rigidbody drb = drop.gameObject.GetComponent<Rigidbody>();
            drb.AddExplosionForce(explosionForce / 5.0f, cauldronFractured.transform.position, explosionRadius, upwardsModifier);
        }

        foreach(GameObject obj in floatingObjects) {
            obj.GetComponent<IngredientController>().CauldronExploded();
        }

        exploded = true;
    }
}