using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [MenuItem("Tools/Scan Missing Scripts")]
    public static void Scan()
    {
        GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();

        foreach (var go in gos)
        {
            var components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null)
                {
                    Debug.LogError("Missing component on: " + go.name, go);
                }
            }
        }
    }
}
