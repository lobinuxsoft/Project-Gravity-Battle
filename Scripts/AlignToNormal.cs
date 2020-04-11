using Godot;

public class AlignToNormal : RigidBody
{
    [Export] readonly Vector3 gravityDirection = Vector3.Down;
    [Export(PropertyHint.Range, "0,180")] private float speedToAlign = 90f;


    public override void _PhysicsProcess(float delta)
    {
        Vector3 upDir = AngularVelicityAlignTo(GlobalTransform.basis.y, -gravityDirection);

        AngularVelocity = upDir;
    }

    private Vector3 AngularVelicityAlignTo(Vector3 currentDirection, Vector3 directionToAlign)
    {
        Vector3 axisRotation = currentDirection.Cross(directionToAlign).Normalized();
        
        float rotationAngle = currentDirection.AngleTo(directionToAlign);
    
        return axisRotation * (rotationAngle * speedToAlign);
    }
}
