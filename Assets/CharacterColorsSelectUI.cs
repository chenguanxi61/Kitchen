using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorsSelectUI : MonoBehaviour
{
    [SerializeField] private Button[] colorButtonArray;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstanceInScene()
    {
        GameObject root = GameObject.Find("CharacterColorsSelectUI");
        if (root != null && root.GetComponent<CharacterColorsSelectUI>() == null)
        {
            root.AddComponent<CharacterColorsSelectUI>();
        }
    }

    private void Awake()
    {
        if (colorButtonArray == null || colorButtonArray.Length == 0)
        {
            colorButtonArray = GetComponentsInChildren<Button>(true);
        }

        ApplyButtonColorsFromPlayerColorList();

        for (int i = 0; i < colorButtonArray.Length; i++)
        {
            int buttonIndex = i;
            colorButtonArray[i].onClick.AddListener(() => OnColorButtonClicked(buttonIndex));
        }
    }

    private void Start()
    {
        ApplyButtonColorsFromPlayerColorList();

        if (KitchGameMultiPlayer.Instance != null)
        {
            KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged += Instance_OnPlayerDataNetWorkListChanged;
        }

        UpdateSelectedVisual();
    }

    private void OnDestroy()
    {
        if (KitchGameMultiPlayer.Instance != null)
        {
            KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged -= Instance_OnPlayerDataNetWorkListChanged;
        }
    }

    private void Instance_OnPlayerDataNetWorkListChanged()
    {
        UpdateSelectedVisual();
    }

    private void OnColorButtonClicked(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= colorButtonArray.Length || KitchGameMultiPlayer.Instance == null)
        {
            return;
        }

        if (buttonIndex >= KitchGameMultiPlayer.Instance.GetPlayerColorCount())
        {
            return;
        }

        ulong localClientId = Unity.Netcode.NetworkManager.Singleton.LocalClientId;
        if (!KitchGameMultiPlayer.Instance.IsColorIndexAvailableForClient(buttonIndex, localClientId))
        {
            return;
        }

        KitchGameMultiPlayer.Instance.SetPlayerColorByIndex(buttonIndex);
    }

    private void UpdateSelectedVisual()
    {
        if (KitchGameMultiPlayer.Instance == null || Unity.Netcode.NetworkManager.Singleton == null)
        {
            return;
        }

        Color currentColor = KitchGameMultiPlayer.Instance.GetPlayerColor(Unity.Netcode.NetworkManager.Singleton.LocalClientId);
        Color32 currentColor32 = currentColor;

        for (int i = 0; i < colorButtonArray.Length; i++)
        {
            Button button = colorButtonArray[i];
            if (button == null || button.targetGraphic == null)
            {
                continue;
            }

            if (i >= KitchGameMultiPlayer.Instance.GetPlayerColorCount())
            {
                continue;
            }

            Color32 buttonColor = KitchGameMultiPlayer.Instance.GetPlayerColor(i);
            bool isSelected = buttonColor.r == currentColor32.r &&
                              buttonColor.g == currentColor32.g &&
                              buttonColor.b == currentColor32.b;

            bool isColorAvailable = KitchGameMultiPlayer.Instance.IsColorIndexAvailableForClient(
                i, Unity.Netcode.NetworkManager.Singleton.LocalClientId);
            button.interactable = isSelected || isColorAvailable;

            Transform selectedTransform = button.transform.Find("Selected");
            if (selectedTransform != null)
            {
                selectedTransform.gameObject.SetActive(isSelected);
            }
        }
    }

    private void ApplyButtonColorsFromPlayerColorList()
    {
        if (KitchGameMultiPlayer.Instance == null || colorButtonArray == null)
        {
            return;
        }

        int playerColorCount = KitchGameMultiPlayer.Instance.GetPlayerColorCount();

        for (int i = 0; i < colorButtonArray.Length; i++)
        {
            Button button = colorButtonArray[i];
            if (button == null || button.targetGraphic == null)
            {
                continue;
            }

            bool hasColor = i < playerColorCount;
            button.gameObject.SetActive(hasColor);

            if (hasColor)
            {
                button.targetGraphic.color = KitchGameMultiPlayer.Instance.GetPlayerColor(i);
            }
        }
    }
}
