using Godot;
using System;

public class Movement : RigidBody
{
    [Export(PropertyHint.Range, "0,100")] private float maxSpeed = 10f, maxAcceleration = 10f, maxAirAcceleration = 1f;
    [Export(PropertyHint.Range, "0,100")] private float jumpHeight = 2f;
    [Export(PropertyHint.Range, "0,5")] int maxAirJumps = 0;
    [Export(PropertyHint.Range, "0,90")] float maxGroundAngle = 25f;
    [Export(PropertyHint.Range, "0,180")] float rotOffset = 90f;

    private Vector3 velocity, desiredVelocity, contactNormal;
    private bool desiredJump;
    int jumpPhase;
    float minGroundDotProduct;

    //bool onGround;
    int groundContactCount;

    public Vector3 direction = Vector3.Forward;

    bool OnGround => groundContactCount > 0;

    public Vector3 ContactNormal => contactNormal;

    public override void _Ready()
    {
        OnValidate();
    }

    public override void _Process(float delta)
    {
        Vector2 playerInput;
        playerInput.x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        playerInput.y = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");
        playerInput = playerInput.Clamped(1f);

        direction.x = playerInput.x;
        direction.z = playerInput.y;
        direction.Normalized();

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        desiredJump |= Input.IsActionJustPressed("jump");
    }

    public override void _IntegrateForces(PhysicsDirectBodyState bodyState)
    {
        velocity = bodyState.LinearVelocity;
        
        UpdateState(bodyState);
        AdjustVelocity(bodyState);
        
        if (desiredJump) {
            desiredJump = false;
            Jump(bodyState);
        }
        bodyState.LinearVelocity = velocity;
        //onGround = false;
        ClearState();

        EvaluateColision(bodyState);
        
        AlignToNormal(contactNormal);
    }
    
    void Jump(PhysicsDirectBodyState bodyState)
    {
        if (OnGround || jumpPhase < maxAirJumps) {
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(2f * (bodyState.TotalGravity.Length() * GravityScale) * jumpHeight);
            float alignedSpeed = velocity.Dot(contactNormal);
            if (alignedSpeed > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            
            //velocity.y += jumpSpeed;
            velocity += contactNormal * jumpSpeed;
        }
    }

    private void EvaluateColision(PhysicsDirectBodyState bodyState)
    {
        for (int i = 0; i < bodyState.GetContactCount(); i++) {
            Vector3 normal = bodyState.GetContactLocalNormal(i);
            //onGround |= normal.y >= minGroundDotProduct;
            if (normal.y >= minGroundDotProduct) {
                //onGround = true;
                groundContactCount += 1;
                contactNormal += normal;
            }
        }
    }
    
    void UpdateState (PhysicsDirectBodyState bodyState) {
        velocity = bodyState.LinearVelocity;
        if (OnGround) {
            jumpPhase = 0;
            if (groundContactCount > 1) {
                contactNormal.Normalized();
            }
        }
        else {
            contactNormal = Vector3.Up;
        }
    }
    
    void OnValidate () {
        minGroundDotProduct = Mathf.Cos(Mathf.Deg2Rad(maxGroundAngle));
    }
    
    Vector3 ProjectOnContactPlane (Vector3 vector) {
        return vector - contactNormal * vector.Dot(contactNormal);
    }
    
    void AdjustVelocity (PhysicsDirectBodyState bodyState) {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.Right).Normalized();
        Vector3 zAxis = ProjectOnContactPlane(-Vector3.Forward).Normalized();
        
        float currentX = velocity.Dot(xAxis);
        float currentZ = velocity.Dot(zAxis);
        
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * bodyState.Step;

        float newX = Mathf.MoveToward(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveToward(currentZ, desiredVelocity.z, maxSpeedChange);
        
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    
    void ClearState () {
        //onGround = false;
        groundContactCount = 0;
        contactNormal = Vector3.Zero;
    }

    private void AlignToNormal(Vector3 normal)
    {
        var rotDegrees = RotationDegrees.Normalized();
        
        if (OnGround)
        {
            rotDegrees.z = Mathf.Rad2Deg(Vector2.Zero.AngleToPoint(new Vector2(normal.x, normal.y))) + rotOffset;
            rotDegrees.x = Mathf.Rad2Deg(Vector2.Zero.AngleToPoint(new Vector2(normal.z, normal.y))) + rotOffset;
        }
        else
        {
            rotDegrees.z = Mathf.Rad2Deg(Vector2.Zero.AngleToPoint(new Vector2(normal.x, normal.y)));
            rotDegrees.x = Mathf.Rad2Deg(Vector2.Zero.AngleToPoint(new Vector2(normal.z, normal.y)));
        }

        RotationDegrees = rotDegrees;
    }
    
}
