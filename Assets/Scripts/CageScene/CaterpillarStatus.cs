using UnityEditor.Rendering;
using UnityEngine;

public enum CaterpillarSpecies
{
    PapilioXuthus,
    PapilioMachaon,
    AcherontiaLachesis,
    TheretraOldenlandiae,
    AntheraeaYamamai
}

public class CaterpillarStatus : MonoBehaviour
{
    [Header("")]
    public CaterpillarSpecies species = CaterpillarSpecies.PapilioXuthus;
    [Range(1, 5)] public int instar = 1;
    public bool isPupa = false;
    public bool isAdult = false;

    [Header("")]
    public int[] leavesPerInstar = new int[5] { 2, 3, 4, 5, 5 };

    [Header("")]
    public int EatenToday = 0;
    public int NeededToday = 2;

    private void Awake()
    {
        RecalcNeededToday();
    }

    public void RecalcNeededToday()
    {
        int idx = Mathf.Clamp(instar, 1, 5) - 1;
        NeededToday = leavesPerInstar[idx];
    }

    public void IncrementEatenToday()
    {
        EatenToday += 1;
    }

    public void AdvanceDay()
    {
        EatenToday = 0;

        if (!isPupa && !isAdult)
        {
            if (instar < 5)
            {
                instar += 1;
            }
            else
            {
                isPupa = true;
            }
        }
        else if (isPupa && !isAdult)
        {
            isAdult = true;
        }

        RecalcNeededToday();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
