using Godot;
using System;

public class AnimationControl : AnimationTree
{
    [Export] private NodePath rigidBodyReference;
    [Export] private float maxVelocity;
    [Export] private string moveParameter;

    private RigidBody _rigidBody;
    
    public override void _Ready()
    {
        _rigidBody = GetNode<RigidBody>(rigidBodyReference);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      Set(moveParameter, _rigidBody.LinearVelocity.Length() / maxVelocity);
  }
}
