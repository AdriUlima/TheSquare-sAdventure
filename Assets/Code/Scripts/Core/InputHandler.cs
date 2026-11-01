using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static GameManager;

public static class InputHandlerExtensions
{
    public static bool TryGetValue(this Vector2? nullable, out Vector2 value)
    {
        if (nullable.HasValue)
        {
            value = nullable.Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
}

public class InputHandler : MonoBehaviour
{
    void Start() => BehaviourPlus.SetInputHandler(this);

    #region Editor
    public Vector2? MousePos { get; private set; }
    public void GetMousePos(InputAction.CallbackContext c)
    {
        Vector2 aux = c.ReadValue<Vector2>();
        MousePos = IsMouseInsideScreen()? aux : null;

        bool IsMouseInsideScreen() => aux.x >= 0f && aux.x <= Screen.width && aux.y >= 0f && aux.y <= Screen.height;
    }

    public float CameraZoom { get; private set; }
    public void GetCameraZoom(InputAction.CallbackContext c)
    {
        CameraZoom = c.ReadValue<float>();
    }

    public PhasedInput PlaceTile { get; private set; } = new();
    public void GetPlaceTile(InputAction.CallbackContext c)
    {
        PlaceTile.SetPhase(c);
    }

    public bool DiscardTile { get; private set; }
    public void GetDiscardTile(InputAction.CallbackContext c)
    {
        DiscardTile = c.performed;
    }

    public PhasedInput<Vector2> CameraMov { get; private set; } = new();
    public void GetCameraMov_enabled(InputAction.CallbackContext c)
    {
        CameraMov.SetPhase(c);
    }
    public void GetCameraMov(InputAction.CallbackContext c)
    {
        CameraMov.TrySetValue(c);
    }

    public enum InputPhases { None, Started, Performed, Canceled }
    public class PhasedInput
    {
        public InputPhases Phase;
        public virtual void SetPhase(InputAction.CallbackContext c)
        {
            Phase = c.phase switch
            {
                InputActionPhase.Started => InputPhases.Started,
                InputActionPhase.Performed => InputPhases.Performed,
                InputActionPhase.Canceled => InputPhases.Canceled,
                _ => InputPhases.None
            };
        }
        public bool IsStarted => Phase is InputPhases.Started;
        public bool IsTriggered => Phase is InputPhases.Started or InputPhases.Performed;
        public bool IsNone => Phase is InputPhases.None;
    }

    public class PhasedInput<T> : PhasedInput where T : struct
    {
        public T Value { get; private set; }
        public void TrySetValue(InputAction.CallbackContext c)
        {
            Value = IsTriggered? c.ReadValue<T>() : default;
        }
    }
    #endregion

    #region Play
    public Vector2Int MoveDir { get; private set; }
    public void GetLeft(InputAction.CallbackContext c)
    {
        if (c.performed) MoveDir = Vector2Int.left;
    }
    public void GetRight(InputAction.CallbackContext c)
    {
        if (c.performed) MoveDir = Vector2Int.right;
    }
    public void GetUp(InputAction.CallbackContext c)
    {
        if (c.performed) MoveDir = Vector2Int.up;
    }
    public void GetDown(InputAction.CallbackContext c)
    {
        if (c.performed) MoveDir = Vector2Int.down;
    }
    #endregion

    void LateUpdate()
    {
        DiscardTile = false;
        MoveDir = Vector2Int.zero;
    }
}