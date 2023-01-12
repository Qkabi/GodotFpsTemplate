using Godot;

namespace Qkabi.FPSTemplate;

public partial class CameraController : Node3D {
    [Export] public float Sensitivity = 2f;
    [Export] public float YLimit = 90f;

    private Node3D? _parent;

    private Vector2 _mouseAxis;
    private Vector3 _rotation;

    public Camera3D Camera { get; private set; }

    public override void _Ready() {
        Camera = GetNode<Camera3D>("Camera3D");

        Sensitivity /= 1000;
        YLimit = Mathf.DegToRad(YLimit);

        _parent = GetParentOrNull<Node3D>();
    }

    public override void _Input(InputEvent @event) {
        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured) {
            _mouseAxis = motion.Relative;
            HandleRotation();
        }
    }

    public override void _Process(double delta) {
        var axis = InputUtils.GetLookAxis();
        if (axis != Vector2.Zero) {
            _mouseAxis = axis * 1000 * (float)delta;
            HandleRotation();
        }
    }

    private void HandleRotation() {
        _rotation.y -= _mouseAxis.x * Sensitivity;
        _rotation.x = Mathf.Clamp(_rotation.x - _mouseAxis.y * Sensitivity, -YLimit, YLimit);
        Rotation = new Vector3(_rotation.x, Rotation.y, Rotation.z);
        if (_parent != null) {
            _parent.Rotation = new Vector3(_parent.Rotation.x, _rotation.y, _parent.Rotation.z);
        }
    }
}
