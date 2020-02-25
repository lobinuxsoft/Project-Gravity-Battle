using UnityEngine;
using UnityEngine.Serialization;

public class MoveAgent : MonoBehaviour
{
    /*
     * Medidas a tener en cuenta
     * Velocidad maxima de una persona:
     *     Caminando = 7 km/h = 1.94 m/s
     *     Trotando = 15 km/h = 4.16 m/s
     *     Corriendo = 24 km/h = 6.6 m/s
     */
    [SerializeField, Range(0f, 100f)] private float maxSpeed = 4.16f;    // Unidades en metros / segundos
    [SerializeField] private bool isBoosting = false;
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f, maxAirAcceleration = 1f;    // Unidades en metros / segundos
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 2f;
    [SerializeField, Range(0, 5)] private int maxAirJumps = 0;
    [SerializeField, Range(0, 90)] private float maxGroundAngle = 40f, maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 100f;
    [SerializeField, Range(0f, 10f)] private float speedGroundRotAlign = 5f;
    [SerializeField, Min(0f)] private float probeDistance = 1f;
    [SerializeField] private LayerMask probeMask = -1, stairsMask = -1;

    private Rigidbody _body;
    private Vector3 _velocity, _desiredVelocity;
    private bool _desiredJump;
    //bool _onGround;
    private int _groundContactCount = 0, _steepContactCount;
    private bool OnGround => _groundContactCount > 0;
    private bool OnSteep => _steepContactCount > 0;
    
    private int _jumpPhase;
    private Vector3 _contactNormal, _steepNormal;
    private float _minGroundDotProduct, _minStairsDotProduct;
    private int _stepsSinceLastGrounded, _stepsSinceLastJump;

    void OnValidate () {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        _minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update () {
        Vector2 playerInput;
        isBoosting = Input.GetKey(KeyCode.LeftShift);
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        var finalSpeed = (!isBoosting) ? maxSpeed : maxSpeed * 1.6f;
        
        _desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * finalSpeed;
        
        _desiredJump |= Input.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();

        if (_desiredJump) {
            _desiredJump = false;
            Jump();
        }
        
        _body.velocity = _velocity;
        ClearState();
    }
    
    /// <summary>
    /// Vuelve las variables referentes al contacto a su estado base.
    /// </summary>
    void ClearState ()
    {
        _groundContactCount = _steepContactCount = 0;
        _contactNormal = _steepNormal = Vector3.zero;
    }
    
    /// <summary>
    /// Realiza los calculos de movimiento.
    /// </summary>
    void UpdateState () {
        _stepsSinceLastGrounded += 1;
        _stepsSinceLastJump += 1;
        _velocity = _body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts()) {
            _stepsSinceLastGrounded = 0;
            
            if (_stepsSinceLastJump > 1) {
                _jumpPhase = 0;
            }
            
            if (_groundContactCount > 1)
            {
                _contactNormal.Normalize();
            }
        }
        else {
            _contactNormal = Vector3.up;
        }
        
        // Rotar el objeto para que quede alineado con el suelo
        SetGroundDirection(_contactNormal);
    }
    
    /// <summary>
    /// Realiza el saldo, saltos en el aire y salto contra muros.
    /// </summary>
    void Jump () {
        
        Vector3 jumpDirection;
        if (OnGround) {
            jumpDirection = _contactNormal;
        }
        else if (OnSteep) {
            jumpDirection = _steepNormal;
            _jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && _jumpPhase <= maxAirJumps) {
            if (_jumpPhase == 0) {
                _jumpPhase = 1;
            }
            jumpDirection = _contactNormal;
        }
        else {
            return;
        }
		
        //if (OnGround || jumpPhase < maxAirJumps) {
        _stepsSinceLastJump = 0;
        _jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
        if (alignedSpeed > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        _velocity += jumpDirection * jumpSpeed;
        //}
    }
    
    void OnCollisionEnter (Collision collision) {
        EvaluateCollision(collision);
    }
    
    void OnCollisionStay (Collision collision) {
        EvaluateCollision(collision);
    }

    /// <summary>
    /// Evalua que tipo de colision resive
    /// </summary>
    /// <param name="collision"></param>
    void EvaluateCollision (Collision collision) {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDot)
            {
                _groundContactCount += 1;
                _contactNormal += normal;
            }
            else if (normal.y > -0.01f) {
                _steepContactCount += 1;
                _steepNormal += normal;
            }
        }
    }
    
    Vector3 ProjectOnContactPlane (Vector3 vector) {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }
    
    /// <summary>
    /// Ajusta la velocidad tanto en suelo, como en el aire.
    /// </summary>
    void AdjustVelocity () {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        
        float currentX = Vector3.Dot(_velocity, xAxis);
        float currentZ = Vector3.Dot(_velocity, zAxis);
        
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
        
        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    
    /// <summary>
    /// Se encarga de mantener al objeto en contacto con el suelo.
    /// </summary>
    /// <returns></returns>
    bool SnapToGround () {
        if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2) {
            return false;
        }
        
        float speed = _velocity.magnitude;
        if (speed > maxSnapSpeed) {
            return false;
        }
        
        if (!Physics.Raycast(_body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask)) {
            return false;
        }
        
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer)) {
            return false;
        }
        
        _groundContactCount = 1;
        _contactNormal = hit.normal;
        float dot = Vector3.Dot(_velocity, hit.normal);
        
        if (dot > 0f) {
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }
    
    private void SetGroundDirection(Vector3 value)
    {
        Vector3 bodyUp = _body.transform.up;
        Quaternion resultRotation = Quaternion.FromToRotation(bodyUp, value) * _body.rotation;
        _body.rotation = Quaternion.Lerp(_body.rotation, resultRotation, Time.deltaTime * speedGroundRotAlign);
    }
    
    float GetMinDot (int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
    }
    
    bool CheckSteepContacts () {
        if (_steepContactCount > 1) {
            _steepNormal.Normalize();
            if (_steepNormal.y >= _minGroundDotProduct) {
                _groundContactCount = 1;
                _contactNormal = _steepNormal;
                return true;
            }
        }
        return false;
    }
}
