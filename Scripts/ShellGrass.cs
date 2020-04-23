using Godot;
using System;

public class ShellGrass : MultiMeshInstance
{
    [Export(PropertyHint.Range, "1, 100")] private int detail = 5;
    [Export(PropertyHint.Range, "0, 2")] private float separation = .1f;
    [Export] Vector3 offsetDir = Vector3.Up;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Multimesh.InstanceCount = detail;
        
        if (Multimesh.InstanceCount > 0)
        {
            for (int i = 0; i < Multimesh.InstanceCount; i++)
            {
                var position = Transform;
                position.origin = offsetDir;
                Multimesh.SetInstanceTransform(i,  position);
            }
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      if (detail != Multimesh.InstanceCount)
      {
          Multimesh.InstanceCount = detail;
      }
      
      if (Multimesh.InstanceCount > 0)
      {
          for (int i = 0; i < Multimesh.InstanceCount; i++)
          {
              var position = Transform;
              
              position.origin += offsetDir * ( (float)i/(float)detail) * separation;
              Multimesh.SetInstanceTransform(i,  position);
          }
      }
  }
}
