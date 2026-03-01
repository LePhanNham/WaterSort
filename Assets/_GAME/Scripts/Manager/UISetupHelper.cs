using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISetupHelper : MonoBehaviour
{
    private void Awake()
    {
        CheckAndSetupEventSystem();
        CheckAndSetupGraphicRaycaster();
    }

    private void CheckAndSetupEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }

    private void CheckAndSetupGraphicRaycaster()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
        }
        
        if (canvas != null)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            
            if (raycaster == null)
                canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
        else
        {
            Debug.LogError("Không tìm thấy Canvas trong scene!");
        }
    }
}
