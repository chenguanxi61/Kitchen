using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialUI : MonoBehaviour
{
    public static TutorialUI Instance { get; private set; }

    private GameObject tutorialGameObject;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject gameObject = new GameObject("TutorialUIController");
        Instance = gameObject.AddComponent<TutorialUI>();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        UnsubscribeGameEvents();
    }

    private void Start()
    {
        SceneManager_sceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        tutorialGameObject = null;
        BindTutorialGameObject();
        SubscribeGameEvents();

        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Update()
    {
        if (tutorialGameObject == null)
        {
            BindTutorialGameObject();
        }
    }

    private void BindTutorialGameObject()
    {
        if (tutorialGameObject != null)
        {
            return;
        }

        foreach (GameObject rootGameObject in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform foundTutorialUI = FindChildByName(rootGameObject.transform, "TutorialUI");
            if (foundTutorialUI != null && foundTutorialUI.gameObject != gameObject)
            {
                tutorialGameObject = foundTutorialUI.gameObject;
                return;
            }
        }
    }

    private Transform FindChildByName(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform foundChild = FindChildByName(child, childName);
            if (foundChild != null)
            {
                return foundChild;
            }
        }

        return null;
    }

    private void SubscribeGameEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
            GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        }

        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
            GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        }
    }

    private void UnsubscribeGameEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
        }

        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
        }
    }

    private void GameManager_OnStateChanged(GameManager.State state)
    {
        if (state == GameManager.State.WaitingToStart ||
            state == GameManager.State.CountdownToStart)
        {
            Show();
            return;
        }

        if (state == GameManager.State.GamePlaying)
        {
            Hide();
        }
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (IsShowing())
        {
            Hide();
        }
    }

    public void Show()
    {
        BindTutorialGameObject();
        if (tutorialGameObject != null)
        {
            tutorialGameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (tutorialGameObject != null)
        {
            tutorialGameObject.SetActive(false);
        }
    }

    public bool IsShowing()
    {
        return tutorialGameObject != null && tutorialGameObject.activeSelf;
    }
}
