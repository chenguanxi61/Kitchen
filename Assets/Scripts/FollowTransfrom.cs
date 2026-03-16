using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransfrom : MonoBehaviour
{
    private Transform targetTransfrom;

    public void SetTargetTransfrom(Transform transfrom)
    {
        targetTransfrom = transfrom;
    }

    private void LateUpdate()
    {
        if (targetTransfrom == null)
        {
            return;
        }
        
        transform.position = targetTransfrom.position;
        transform.rotation = targetTransfrom.rotation;
    }
}
