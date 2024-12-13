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
    public TextMeshPro textProgressPoints;
    public TextMeshPro textCauldron;
    public TextMeshPro textTimer;

    private string maxBunnyCounterString;
    private string maxProgressString;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeUI(int maxBunnyCounter_) {
        maxBunnyCounterString = " / " + maxBunnyCounter_.ToString();
        maxProgressString = " %";
        textCauldron.text = "Unstable";
    }

    public void UpdateDustBunnyCounter(int counter) {
        textDustBunnyCounter.text = counter.ToString() + maxBunnyCounterString;
    }

    public void UpdateProgressPoints(int progress) {
        textProgressPoints.text = progress.ToString() + maxProgressString;
    }

    public void UpdateCauldronStatus(bool solved) {
        if (solved) {
            textCauldron.text = "Stable!";
        } else {
            textCauldron.text = "Exploded :(";
        }
    }

    public void UpdateTime(float time) {
        textTimer.text = Mathf.RoundToInt(time).ToString() + "s";
    }
}
