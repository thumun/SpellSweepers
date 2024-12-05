using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
    }

    private static UIManager m_instance;

    public TextMeshPro textDustBunnyCounter;
    public TextMeshPro textManaPoints;
    public TextMeshPro textProgressPoints;

    private string maxBunnyCounterString;
    private string maxManaString;
    private string maxProgressString;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeUI(int maxBunnyCounter_, int maxMana_, int maxProgress_) {
        maxBunnyCounterString = " / " + maxBunnyCounter_.ToString();
        maxManaString = " / " + maxMana_.ToString();
        maxProgressString = " / " + maxProgress_.ToString();
    }

    public void UpdateDustBunnyCounter(int counter) {
        textDustBunnyCounter.text = counter.ToString() + maxBunnyCounterString;
    }

    public void UpdateManaPoints(int mana) {
        textManaPoints.text = mana.ToString() + maxManaString;
    }

    public void UpdateProgressPoints(int progress) {
        textProgressPoints.text = progress.ToString() + maxProgressString;
    }
}
