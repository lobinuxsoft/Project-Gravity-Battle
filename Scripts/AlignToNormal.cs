using Godot;
using System;

public class AlignToNormal : Spatial
{

    [Export] private NodePath movementReference;

    private Movement _movement;
    
    public override void _Ready()
    {
        _movement = (Movement) Get(movementReference);
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      var oTransform = this.Transform;
      var cross = oTransform.basis.z.Cross(_movement.ContactNormal);

      var targetDir = oTransform.LookingAt(cross, Vector3.Up);

      this.Transform = targetDir;
  }
}
