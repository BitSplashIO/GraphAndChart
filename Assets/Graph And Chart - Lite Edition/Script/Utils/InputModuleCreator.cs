using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventSystem))]
public class InputModuleCreator : MonoBehaviour
{
    void Awake()
    {
        BaseInputModule existingModule = GetComponent<BaseInputModule>();
        if (existingModule != null)
            return;

        SetupInputModule();
    }

    void SetupInputModule()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

#elif ENABLE_LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM
        gameObject.AddComponent<StandaloneInputModule>();

#elif ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        gameObject.AddComponent<StandaloneInputModule>(); 
#endif
    }
}