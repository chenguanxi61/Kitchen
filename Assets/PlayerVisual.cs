using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private MeshRenderer headMeshRenderer;
    [SerializeField] private MeshRenderer bodyMeshRenderer;
    
    private Material material;

    private void Awake()
    {
        EnsureMaterial();
    }
    
    public void SetPlayerColor(Color color)
    {
        if (!EnsureMaterial())
        {
            Debug.LogWarning($"[PlayerVisual] Failed to set color on {name}: missing renderer or material.");
            return;
        }

        material.color = color;
        headMeshRenderer.material.color = color;
        bodyMeshRenderer.material.color = color;
    }

    private bool EnsureMaterial()
    {
        if (headMeshRenderer == null || bodyMeshRenderer == null)
        {
            MeshRenderer[] meshRendererArray = GetComponentsInChildren<MeshRenderer>(true);
            if (meshRendererArray.Length >= 2)
            {
                if (headMeshRenderer == null)
                {
                    headMeshRenderer = meshRendererArray[0];
                }

                if (bodyMeshRenderer == null)
                {
                    bodyMeshRenderer = meshRendererArray[1];
                }
            }
        }

        if (headMeshRenderer == null || bodyMeshRenderer == null)
        {
            return false;
        }

        if (material == null)
        {
            material = new Material(headMeshRenderer.material);
            headMeshRenderer.material = material;
            bodyMeshRenderer.material = material;
        }

        return true;
    }
}
