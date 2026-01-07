using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    struct CuttingAni
    {
        public const string CUT = "Cut";
    }
    [SerializeField] private CuttingCounter cuttingCounter;
    
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        cuttingCounter.OnProgressChanged += CuttingCounter_OnProgressChanged;
    }
    private void CuttingCounter_OnProgressChanged(float progress)
    {
        if(progress !=0)
            animator.SetTrigger(CuttingAni.CUT);
    }
}
