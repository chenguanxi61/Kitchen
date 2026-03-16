using System;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    private void Start()
    {
        Hide();

        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }

    private void OnDestroy()
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
        }

        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }

    private void Player_OnAnyPlayerSpawned()
    {
        if (Player.LocalInstance == null)
        {
            return;
        }

        Player.LocalInstance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
        Player.LocalInstance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEvent e)
    {
        if (e.SelectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        foreach (GameObject go in visualGameObjectArray)
        {
            go.SetActive(true);
        }
    }

    public void Hide()
    {
        foreach (GameObject go in visualGameObjectArray)
        {
            go.SetActive(false);
        }
    }
}
