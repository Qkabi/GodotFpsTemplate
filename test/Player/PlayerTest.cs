using Godot;
using System;

public partial class PlayerTest : Node3D {
    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
