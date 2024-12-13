using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
            }
            return m_instance;
        }
    }

    private static GameManager m_instance;

    public bool isGameOver = false;
    public bool isLevelClear = false;

    // Dust bunny related
    public GameObject[] dustBunnies;
    private int dustBunnyCounter = 0;

    // Mana related
    [SerializeField]
    private float currentManaPoints = 100.0f;
    private float maxManaPoints = 100.0f;
    private bool castingVacuum = false;
    private float spellControlCost = 5.0f;
    private float spellVacuumCost = 10.0f;
    private float spellSlowDownCost = 5.0f;

    // Progress related
    private int currentProgressPoints = 0;
    private int maxProgressPoints = 60;
    private int levelOfChaos = 0;
    private bool cauldronFailed = false;
    private bool catHandled = false;

    public GameObject completionStar1;
    public GameObject completionStar2;
    public GameObject completionStar3;

    public GameObject canvasGameOver;
    public GameObject canvasLevelClear;

    [SerializeField]
    private float timer;
    private float maxTime;

    public GameObject clock;

    public Animator doorAnimator;
    
    // Start is called before the first frame update
    void Start()
    {
        maxTime = 300.0f;
        timer = maxTime;

        UIManager.instance.InitializeUI(dustBunnies.Length, maxTime);

        UIManager.instance.UpdateDustBunnyCounter(dustBunnyCounter);
        UIManager.instance.UpdateManaPoints(currentManaPoints / maxManaPoints);
        UIManager.instance.UpdateProgressPoints(currentProgressPoints);

        completionStar1.SetActive(false);
        completionStar2.SetActive(false);
        completionStar3.SetActive(false);

        canvasGameOver.SetActive(false);
        canvasLevelClear.SetActive(false);
    }

    void Update() {
        if (isGameOver) return;

        timer -= Time.deltaTime;
        UIManager.instance.UpdateTime(timer);

        if (timer <= 0.0f) {
            timer = 0.0f;
            UIManager.instance.UpdateTime(timer);
            GameOver();
        }
    }
    
    public bool TryCastSpell(int spellId) {
        bool updated = false;
        if (spellId == 0) {
            // Control spell
            if (currentManaPoints >= spellControlCost) {
                currentManaPoints -= spellControlCost;
                updated = true;
            }
        } else if (spellId == 1) {
            // Vacuum spell
            //if (castingVacuum) {
            //    castingVacuum = false;
            //    updated = true;
            //} else if (currentManaPoints >= spellVacuumCost) {
                castingVacuum = true;
                currentManaPoints -= spellVacuumCost;
                updated = true;
            //}
        } else if (spellId == 2) {
            // Slow down spell
            if (currentManaPoints >= spellSlowDownCost) {
                currentManaPoints -= spellSlowDownCost;
                updated = true;
            }
        }

        if (updated) {
            UIManager.instance.UpdateManaPoints(currentManaPoints / maxManaPoints);
        }
        return updated;
    }

    public void CauldronStatusUpdate(bool solved) {
        if (solved) {
            IncrementProgressPoints(30);
            UIManager.instance.UpdateProgressPoints(currentProgressPoints);
        } else {
            IncrementLevelOfChaos(10);
            cauldronFailed = true;
        }
        UIManager.instance.UpdateCauldronStatus(solved);
    }

    public void CaptureDustBunny() {
        //if (dustBunnyCounter >= dustBunnies.Length) return;

        dustBunnyCounter += 1;
        IncrementProgressPoints(10);
        GainMana(10.0f);
        UIManager.instance.UpdateDustBunnyCounter(dustBunnyCounter);
        UIManager.instance.UpdateProgressPoints(currentProgressPoints);
    }

    private void IncrementProgressPoints(int pts) {
        currentProgressPoints += pts;
        // TODO : Also check for major puzzles / tasks / other failure conditions
        if (currentProgressPoints > 0.6 * maxProgressPoints) {
            LevelClear();
        }
    }

    private void IncrementLevelOfChaos(int pts) {
        levelOfChaos += pts;
        // TODO : balance this arbitrary condition
        if (levelOfChaos >= 0.3f * maxProgressPoints) {
            GameOver();
        }
    }

    public void HandledCat() {
        catHandled = true;
        GainMana(50.0f);
        IncrementProgressPoints(40);
    }

    public void GainMana(float pts) {
        currentManaPoints += pts;
        if (currentManaPoints >= maxManaPoints) currentManaPoints = maxManaPoints;
        UIManager.instance.UpdateManaPoints(currentManaPoints / maxManaPoints);
    }

    public void GameOver() {
        isGameOver = true;
        clock.GetComponent<AudioClipPlayer>().StopClip();
        canvasGameOver.SetActive(true);
        Debug.Log("Game over");
    }

    public void LevelClear() {
        if (!isLevelClear) {
            clock.GetComponent<AudioClipPlayer>().StopClip();
            doorAnimator.Play("DoorOpen", 0, 0.0f);
        }
        isLevelClear = true;

        int numStars = 1;
        if (!cauldronFailed) numStars++;
        if (catHandled) numStars++;

        if (numStars == 1) {
            completionStar1.SetActive(true);
        } else if (numStars == 2) {
            completionStar2.SetActive(true);
            completionStar3.SetActive(true);
        } else if (numStars == 3) {
            completionStar1.SetActive(true);
            completionStar2.SetActive(true);
            completionStar3.SetActive(true);
        }
        canvasLevelClear.SetActive(true);

        Debug.Log("Level clear");
    }
}
