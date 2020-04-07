using Godot;
using System;

public class TPCamera : Spatial
{
    [Export] private readonly NodePath _springArmNode = "";
    private SpringArm _springArm;

    public override async void _Ready()
    {
        await ToSignal(Owner, "ready");
        GD.Print($"Termino la espera...");
        
        _springArm = GetNode<SpringArm>(_springArmNode);
        //GD.Print($"Nodo {_springArm.Name}");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
