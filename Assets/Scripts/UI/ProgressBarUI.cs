using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField]private Image barImage;
    
    [SerializeField]private GameObject progressBarGameObject;
    private IHasProgressBar hasProgressBar;
    private void Start()
    {
        hasProgressBar = progressBarGameObject.GetComponent<IHasProgressBar>();
        hasProgressBar.OnProgressChanged += CuttingCounter_OnProgressChanged;
        barImage.fillAmount = 0f;
        hideProgressBar();
    }
    private void CuttingCounter_OnProgressChanged(float progress)
    {
        barImage.fillAmount = progress;
        if(progress == 0f||progress==1f)
        {
            hideProgressBar();
        }
        else
        {
            showProgressBar();
        }
    }
    
    public void showProgressBar()
    {
        gameObject.SetActive(true);
    }
    public void hideProgressBar()
    {
        gameObject.SetActive(false);
    }
}
