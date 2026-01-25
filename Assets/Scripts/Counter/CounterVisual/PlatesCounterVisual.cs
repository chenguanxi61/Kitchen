using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;
    [SerializeField] private PlatesCounter platesCounter;
    private List<GameObject> plateVisualGameObjectList;

    private void Awake()
    {
        plateVisualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        platesCounter.OnPlateSpawned += PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateRemoved += PlatesCounter_OnPlateRemoved;
    }
    
    private void PlatesCounter_OnPlateSpawned()
    {
        Transform plateVisualTransform = Instantiate(plateVisualPrefab, counterTopPoint);
        plateVisualTransform.localPosition = new Vector3(0f, 0.1f * plateVisualGameObjectList.Count, 0f);
        plateVisualGameObjectList.Add(plateVisualTransform.gameObject);
       
    }
    
    private void PlatesCounter_OnPlateRemoved()
    {
        GameObject plateVisualGameObject = plateVisualGameObjectList[plateVisualGameObjectList.Count - 1];
        plateVisualGameObjectList.RemoveAt(plateVisualGameObjectList.Count - 1);
        Destroy(plateVisualGameObject);
    }
}
