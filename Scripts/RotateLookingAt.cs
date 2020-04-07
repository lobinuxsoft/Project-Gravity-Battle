using Godot;
using System;

public class RotateLookingAt : Spatial
{
    [Export] private NodePath referenceMovement;
    [Export(PropertyHint.Range, "0, 100")] private float rotSpeed = 2f;

    private Movement movement;

    private Vector3 dir;
    
    public override void _Ready()
    {
        movement = (Movement)GetNode(referenceMovement);
        dir = Transform.basis.z;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      if (movement.direction.Length() != 0)
      {
          dir = -movement.direction;
      }
      
      var transOrigin = Transform;
      var targetRot = Transform.LookingAt(dir.Normalized(), Vector3.Up);
      transOrigin.basis = Transform.basis.Slerp(targetRot.basis, delta * rotSpeed).Orthonormalized();
      
      Transform = transOrigin;

      var rot = RotationDegrees;

      rot.x = 0;
      rot.z = 0;

      RotationDegrees = rot;
  }
}
