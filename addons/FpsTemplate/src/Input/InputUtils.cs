using Godot;

namespace Qkabi.FPSTemplate;

public static class InputUtils {
    public static Vector2 GetLookAxis() {
        return Input.GetVector(InputActions.LookLeft,
                               InputActions.LookRight,
                               InputActions.LookDown,
                               InputActions.LookUp);
    }

    public static Vector2 GetMoveAxis() {
        return Input.GetVector(InputActions.MoveLeft,
                               InputActions.MoveRight,
                               InputActions.MoveBack,
                               InputActions.MoveForward);
    }
}