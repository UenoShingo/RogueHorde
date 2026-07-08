using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputSystemUIBootstrap : MonoBehaviour
{
    static InputSystemUIBootstrap instance;
    EventSystem currentEventSystem;
    bool useManualFallback;
    Vector2 lastMoveInput;
    float nextMoveTime;
    const float MoveDeadZone = 0.5f;
    const float MoveRepeatDelay = 0.45f;
    const float MoveRepeatInterval = 0.12f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (null != instance) return;

        GameObject obj = new GameObject(nameof(InputSystemUIBootstrap));
        instance = obj.AddComponent<InputSystemUIBootstrap>();
        DontDestroyOnLoad(obj);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
        setupEventSystem();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    void Update()
    {
        setupEventSystem();
        updateManualFallback();
    }

    void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        setupEventSystem();
    }

    void setupEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (null == eventSystem)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }
        if (null == eventSystem) return;
        currentEventSystem = eventSystem;

        StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (null != legacyModule)
        {
            legacyModule.enabled = false;
        }

        InputSystemUIInputModule inputSystemModule
            = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (null == inputSystemModule)
        {
            inputSystemModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        inputSystemModule.enabled = true;
        assignDefaultActions(inputSystemModule);
        useManualFallback = null == inputSystemModule.actionsAsset;
    }

    void assignDefaultActions(InputSystemUIInputModule inputSystemModule)
    {
        if (null != inputSystemModule.actionsAsset) return;

        MethodInfo method = typeof(InputSystemUIInputModule).GetMethod(
            "AssignDefaultActions",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        method?.Invoke(inputSystemModule, null);
    }

    void updateManualFallback()
    {
        if (!useManualFallback) return;
        if (null == currentEventSystem) return;

        GameObject selected = currentEventSystem.currentSelectedGameObject;
        if (null == selected)
        {
            Selectable selectable = getFirstSelectable();
            if (null != selectable)
            {
                selectable.Select();
            }
            return;
        }

        if (isSubmitPressed())
        {
            ExecuteEvents.Execute(selected, new BaseEventData(currentEventSystem),
                ExecuteEvents.submitHandler);
        }

        if (isCancelPressed())
        {
            ExecuteEvents.Execute(selected, new BaseEventData(currentEventSystem),
                ExecuteEvents.cancelHandler);
        }

        Vector2 moveInput = getNavigateInput();
        if (MoveDeadZone * MoveDeadZone > moveInput.sqrMagnitude)
        {
            lastMoveInput = Vector2.zero;
            nextMoveTime = 0;
            return;
        }

        moveInput = getCardinalMove(moveInput);
        if (Vector2.zero != lastMoveInput && Time.unscaledTime < nextMoveTime)
        {
            return;
        }

        AxisEventData axisEventData = new AxisEventData(currentEventSystem)
        {
            moveVector = moveInput,
            moveDir = getMoveDirection(moveInput)
        };

        ExecuteEvents.Execute(selected, axisEventData, ExecuteEvents.moveHandler);
        nextMoveTime = Time.unscaledTime
            + ((Vector2.zero == lastMoveInput) ? MoveRepeatDelay : MoveRepeatInterval);
        lastMoveInput = moveInput;
    }

    Selectable getFirstSelectable()
    {
        foreach (Selectable selectable in FindObjectsOfType<Selectable>())
        {
            if (selectable.IsActive() && selectable.IsInteractable())
            {
                return selectable;
            }
        }

        return null;
    }

    bool isSubmitPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (null != keyboard)
        {
            if (keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame
                || keyboard.spaceKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        Gamepad gamepad = Gamepad.current;
        return null != gamepad
            && (gamepad.buttonSouth.wasPressedThisFrame
                || gamepad.startButton.wasPressedThisFrame);
    }

    bool isCancelPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (null != keyboard && keyboard.escapeKey.wasPressedThisFrame)
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        return null != gamepad && gamepad.buttonEast.wasPressedThisFrame;
    }

    Vector2 getNavigateInput()
    {
        Vector2 input = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (null != keyboard)
        {
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) input.y += 1;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) input.y -= 1;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) input.x += 1;
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) input.x -= 1;
        }

        Gamepad gamepad = Gamepad.current;
        if (null != gamepad)
        {
            input += gamepad.leftStick.ReadValue();
            input += gamepad.dpad.ReadValue();
        }

        return Vector2.ClampMagnitude(input, 1);
    }

    Vector2 getCardinalMove(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return new Vector2(Mathf.Sign(input.x), 0);
        }

        return new Vector2(0, Mathf.Sign(input.y));
    }

    MoveDirection getMoveDirection(Vector2 input)
    {
        if (0 < input.x) return MoveDirection.Right;
        if (0 > input.x) return MoveDirection.Left;
        if (0 < input.y) return MoveDirection.Up;
        if (0 > input.y) return MoveDirection.Down;
        return MoveDirection.None;
    }
}
