using UnityEngine;

public class LeafVisualController : MonoBehaviour
{
    public GameObject freshModel;
    public GameObject wiltedModel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowFresh()
    {
        if (freshModel != null) freshModel.SetActive(true);
        if (wiltedModel != null) wiltedModel.SetActive(false);
    }

    public void PlayEatenTransition()
    {
        //TODO:Animator再生+終端で非表示に置き換える
        if (freshModel != null) freshModel.SetActive(false);
    }

    public void ShowWilted()
    {
        if (freshModel != null) freshModel.SetActive(false);
        if (wiltedModel != null) wiltedModel.SetActive(true);
    }
}
