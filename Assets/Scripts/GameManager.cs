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

    // Dust bunny related
    public GameObject[] dustBunnies;
    private int dustBunnyCounter = 0;

    // Mana related
    private int currentManaPoints = 100;
    private int maxManaPoints = 100;
    private bool castingVacuum = false;
    public int spellControlCost = 10;
    public int spellVacuumCost = 20;
    public int spellSlowDownCost = 15;

    // Progress related
    private int currentProgressPoints = 0;
    private int maxProgressPoints = 100;
    private int levelOfChaos = 0;

    // For UI
    private string dustBunniesLengthString;
    private string maxManaString;
    
    // Start is called before the first frame update
    void Start()
    {
        dustBunniesLengthString = dustBunnies.Length.ToString();
        maxManaString = maxManaPoints.ToString();

        UIManager.instance.InitializeUI(dustBunnies.Length, maxManaPoints, maxProgressPoints);

        UIManager.instance.UpdateDustBunnyCounter(dustBunnyCounter);
        UIManager.instance.UpdateManaPoints(currentManaPoints);
        UIManager.instance.UpdateProgressPoints(currentProgressPoints);
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
            if (castingVacuum) {
                castingVacuum = false;
                updated = true;
            } else if (currentManaPoints >= spellVacuumCost) {
                castingVacuum = true;
                currentManaPoints -= spellVacuumCost;
                updated = true;
            }
        } else if (spellId == 2) {
            // Slow down spell
            if (currentManaPoints >= spellSlowDownCost) {
                currentManaPoints -= spellSlowDownCost;
                updated = true;
            }
        }

        if (updated) {
            UIManager.instance.UpdateManaPoints(currentManaPoints);
        }

        return updated;
    }

    public void CauldronStatusUpdate(bool solved) {
        if (solved) {
            currentProgressPoints += 30;
            UIManager.instance.UpdateProgressPoints(currentProgressPoints);
        } else {
            levelOfChaos += 10;
        }
    }

    public void CaptureDustBunny() {
        if (dustBunnyCounter >= dustBunnies.Length) return;

        dustBunnyCounter += 1;
        currentProgressPoints += 10;
        UIManager.instance.UpdateDustBunnyCounter(dustBunnyCounter);
        UIManager.instance.UpdateProgressPoints(currentProgressPoints);
    }
}
