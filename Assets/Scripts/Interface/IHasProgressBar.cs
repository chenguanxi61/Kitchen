using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IHasProgressBar
{
    public event UnityAction<float> OnProgressChanged;
}
