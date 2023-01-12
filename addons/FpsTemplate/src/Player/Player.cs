using Godot;
using Godot.Extensions;
using Godot.Extensions.Input;
using Godot.Extensions.Nodes;
using System;

namespace Qkabi.FPSTemplate;

public partial class Player : CharacterBody3D {
    private const float DEFAULT_MASS = 60;

    [Export(PropertyHint.Range, "20, 180, 1")] public float Mass = DEFAULT_MASS;
    [Export] public float GravityMultiplier = 3f;

    [Export] public float NormalSpeed = 10f;
    [Export] public float Acceleration = 8f;
    [Export] public float Deceleration = 10f;

    [Export(PropertyHint.Range, "0, 1, 0.05f")] public float AirControl = 0f;
    [Export] public float JumpHeight = 10f;

    [Export] public float SprintSpeed = 16;
    [Export] public float FovMultiplier = 1.1f;
    [Export] public float FovLerpMultiplier = 4f;
    [Export] public float FovLerpDuration = 0.2f;

    private Vector2 _inputAxis;
    private Vector3 _direction;
    private Vector3 _velocity;

    private float _gravity;

    private float _speed;
    private Camera3D _camera;
    private float _normalFov;
    private float _sprintFov;

    private float _impulseFactor;

    private Tween _tween;

    public event Action<float> SpeedUpdated = delegate { };

    public bool IsSprinting { get; private set; }

    public override void _Ready() {
        _gravity = this.GetGravity3D() * GravityMultiplier;

        _speed = NormalSpeed;
        _camera = GetNode<CameraController>("CameraController").Camera;
        _normalFov = _camera.Fov;
        _sprintFov = _normalFov * FovMultiplier;

        _impulseFactor = Mass / DEFAULT_MASS;

        _tween = CreateTween().Sine();
        _tween.Kill();

        SpeedUpdated += OnSpeedUpdated;
    }

    public override void _Process(double delta) {
        var dt = (float)delta;
        GetDirection();
        UpdateSpeed(dt);
        UpdateVelocity(dt);
        HandleCollision();
    }

    private void GetDirection() {
        _inputAxis = InputUtils.GetMoveAxis();
        var aim = GlobalTransform.basis;
        _direction = aim.z * -_inputAxis.y + aim.x * _inputAxis.x;
    }

    private void UpdateSpeed(float dt) {
        var canSprint = InputActions.Sprint.IsPressed() && _inputAxis.Length() >= 0.5f;
        var newSpeed = canSprint ? (IsOnFloor() ? SprintSpeed : _speed) : NormalSpeed;
        if (newSpeed != _speed) {
            _speed = newSpeed;
            SpeedUpdated(newSpeed);
        }
    }

    private void UpdateVelocity(float dt) {
        if (IsOnFloor()) {
            if (_velocity.y < 0) {
                _velocity.y = 0;
            }
            if (InputActions.Jump.IsJustPressed()) {
                _velocity.y = JumpHeight;
            }
        } else {
            _velocity.y -= _gravity * dt;
        }
        Accelerate(dt);
        Velocity = _velocity;
        MoveAndSlide();
    }

    private void Accelerate(float delta) {
        var tempVel = _velocity;
        tempVel.y = 0;

        var tempAccel = _direction.Dot(tempVel) > 0 ? Acceleration : Deceleration;
        tempAccel *= !IsOnFloor() ? AirControl : 1;

        var target = _direction * _speed;
        tempVel = tempVel.Lerp(target, tempAccel * (float)delta);
        _velocity.x = tempVel.x;
        _velocity.z = tempVel.z;
    }

    private void HandleCollision() {
        var collisionCount = GetSlideCollisionCount();
        for (int i = 0; i < collisionCount; i++) {
            var collision = GetSlideCollision(i);
            if (collision.GetCollider() is RigidBody3D rb) {
                rb.ApplyCentralImpulse(_velocity * _impulseFactor);
            }
        }
    }

    private void OnSpeedUpdated(float newSpeed) {
        IsSprinting = newSpeed == SprintSpeed;
        _tween.Kill();
        _tween = CreateTween().Sine();
        _tween.TweenProperty(_camera, Properties.Camera3D.Fov, IsSprinting ? _sprintFov : _normalFov, FovLerpDuration);
    }
}