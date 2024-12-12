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
    private Renderer liquidSurfaceRenderer;

    public List<GameObject> floatingObjects;
    private List<string> requiredRecipe = new List<string>();
    private List<string> currentFloaters = new List<string>();

    public GameObject liquidDrop;
    public GameObject[] liquidDropSpawnLocs;

    private float liquidSurfaceHeight;
    private Color liquidColor;
    private Color foamColor;

    public Color solvedLiquidColor;
    public Color solvedFoamColor;
    public Color explodingLiquidColor;
    public Color explodingFoamColor;

    private Color solvedLastLiquidColor;
    private Color solvedLastFoamColor;
    private bool solvedColorFinished = false;
    private float solvedColorTimer = 0.0f;
    private float solvedColorTimerMax = 5.0f;

    public int timeToExplode;
    private int explosionTime = 10000;
    // min scale = 0.13, max scale = 0.53;
    private float scaleDiff = 0.4f;

    private bool shouldExplode = false;
    private bool exploded = false;
    private bool solved = false;

    public float explosionForce;
    public float explosionRadius;
    public float upwardsModifier;

    private AudioClipPlayer audioPlayerBoiling;
    public GameObject audioPlayerExplosion;
    private float minVolume = 0.1f;
    private float maxVolume = 1.0f;
    private float solvedLastVolume;
    private float currentVolume;

    // Start is called before the first frame update
    void Start()
    {
        cauldronWhole.SetActive(true);
        cauldronFractured.SetActive(false);
        liquidSurface.SetActive(true);

        liquidSurfaceRenderer = liquidSurface.GetComponent<Renderer>();

        liquidSurfaceHeight = liquidSurface.transform.position.y;

        requiredRecipe.Add("IngRed");
        requiredRecipe.Add("IngBlue");

        liquidColor = new Color(0.0f, 0.0f, 0.0f);
        foamColor = new Color(0.0f, 0.0f, 0.0f);

        audioPlayerBoiling = GetComponent<AudioClipPlayer>();
        currentVolume = minVolume;
    }

    // Update is called once per frame
    void Update()
    {
        if (exploded) return;

        if (solved) {
            if (solvedColorFinished) return;

            solvedColorTimer += Time.deltaTime;
            solvedColorTimer = Mathf.Min(solvedColorTimer, solvedColorTimerMax);
            if (solvedColorTimer >= solvedColorTimerMax) solvedColorFinished = true;

            liquidColor = solvedLastLiquidColor / (float)solvedColorTimerMax * (float)(solvedColorTimerMax - solvedColorTimer) + solvedLiquidColor / (float)solvedColorTimerMax * (float)solvedColorTimer;
            liquidColor = solvedLastFoamColor / (float)solvedColorTimerMax * (float)(solvedColorTimerMax - solvedColorTimer) + solvedFoamColor / (float)solvedColorTimerMax * (float)solvedColorTimer;

            liquidSurfaceRenderer.material.SetColor("_BaseColor", liquidColor);
            liquidSurfaceRenderer.material.SetColor("_FoamColor", foamColor);

            currentVolume = solvedLastVolume / (float)solvedColorTimerMax * (float)(solvedColorTimerMax - solvedColorTimer) + minVolume / (float)solvedColorTimerMax * (float)solvedColorTimer;
            audioPlayerBoiling.ChangeVolume(currentVolume);
            return;
        }

        IncrementTimeToExplode(1);
        if (timeToExplode >= explosionTime) {
            shouldExplode = true;
        }

        if (shouldExplode && !exploded) {
            Explode();
        }

        liquidSurfaceRenderer.material.SetColor("_BaseColor", liquidColor);
        liquidSurfaceRenderer.material.SetColor("_FoamColor", foamColor);

        audioPlayerBoiling.ChangeVolume(currentVolume);
    }

    public float GetSurfaceHeight() {
        return liquidSurfaceHeight;
    }

    public void AddNewIngredient(string ingredient) {
        if (exploded || solved) return;

        if (ingredient == "ToxicBottle") {
            Explode();
            return;
        }
        
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
                solvedLastLiquidColor = liquidColor;
                solvedLastFoamColor = foamColor;
                GameManager.instance.CauldronStatusUpdate(true);
            }
        }
    }

    public void IncrementTimeToExplode(int t) {
        timeToExplode = Mathf.Min(explosionTime, timeToExplode + t);
        liquidSurface.transform.localScale += new Vector3(0, 0, 1) * scaleDiff / (float)explosionTime;

        liquidColor = explodingLiquidColor / (float)explosionTime * (float)timeToExplode;
        foamColor = explodingFoamColor / (float)explosionTime * (float)timeToExplode;

        currentVolume = maxVolume / (float)explosionTime * (float)timeToExplode;
    }

    void Explode()
    {
        if (exploded) return;

        audioPlayerBoiling.StopClip();

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
        audioPlayerExplosion.GetComponent<AudioClipPlayer>().PlayClip();

        exploded = true;
        GameManager.instance.CauldronStatusUpdate(false);
    }
}
