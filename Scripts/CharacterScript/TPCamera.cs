using Godot;
using System;

public class TPCamera : Spatial
{
    
    [Export] private NodePath targetToFollow = "";
    [Export] private NodePath pivotNode = "";
    [Export] private NodePath springArmNode = "";
    [Export] private NodePath cameraNode = "";
    [Export(PropertyHint.Range, "1,100")] private float cameraDistance = 5;
    [Export(PropertyHint.Range, "1,100")] private float cameraLerpSpeed = 1;

    private Spatial pivot;
    private SpringArm springArm;
    private Spatial target;
    private InterpolatedCamera interpolatedCamera;

    public override async void _Ready()
    {
        await ToSignal(Owner, "ready");

        target = GetNode<Spatial>(targetToFollow);
        pivot = GetNode<Spatial>(pivotNode);
        springArm = GetNode<SpringArm>(springArmNode);
        interpolatedCamera = GetNode<InterpolatedCamera>(cameraNode);
    }

    public override void _PhysicsProcess(float delta)
    {
        springArm.SpringLength = cameraDistance;
        interpolatedCamera.Speed = cameraLerpSpeed;
        
        pivot.Translation = target.Translation;
    }
}
