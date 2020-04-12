using Godot;

public class RotateLookingAt : Spatial
{
    [Export] private readonly NodePath referenceMovement = "";
    [Export(PropertyHint.Range, "0, 100")] private readonly float rotSpeed = 2f;

    private Movement movement;

    private Vector3 dir;
    
    public override void _Ready()
    {
        movement = GetNode<Movement>(referenceMovement);
        dir = Transform.basis.z;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      if (movement.Direction.Length() != 0)
      {
          dir = -movement.Direction;
      }
      
      Transform transOrigin = GlobalTransform;
      Transform targetRot = Transform.LookingAt(dir.Normalized(), Vector3.Up);
      transOrigin.basis = GlobalTransform.basis.Slerp(targetRot.basis, delta * rotSpeed).Orthonormalized();
      
      GlobalTransform = transOrigin;

      Vector3 rot = RotationDegrees;

      rot.x = 0;
      rot.z = 0;

      RotationDegrees = rot;
  }
}
