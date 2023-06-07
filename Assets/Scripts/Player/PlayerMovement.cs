using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; 
using static UnityEngine.InputSystem.InputAction;

public class PlayerMovement : MonoBehaviour
{
  public PlayerData Data;

  #region Variables
  //Components
  public Rigidbody2D RB { get; private set; }
  private TrailRenderer _trailRenderer; 
  public BoxCollider2D _collider = null;

  //Variables control the various actions the player can perform at any time.
  public bool IsFacingRight { get; private set; }
  public bool IsJumping { get; private set; }
  public bool IsWallJumping { get; private set; }
  public bool IsSliding { get; private set; }
  public bool IsDashing { get; private set; }
  public bool IsDashSleeping { get; private set; }

  //Timers (also all fields, could be private and a method returning a bool could be used)
  public float LastOnGroundTime { get; private set; }
  public float LastOnWallTime { get; private set; }
  public float LastOnWallRightTime { get; private set; }
  public float LastOnWallLeftTime { get; private set; }

  public float LastPunchedTime { get; private set; }
  public float _punchHoldTime { get; private set; }

  //Jump
  private bool _isJumpCut;
  private bool _isJumpFalling;

  //Dash
  private int _dashesLeft;
  private bool _dashRefilling;
  private Vector2 _lastDashDir;
  private bool _isDashAttacking;

  //Wall Jump
  private float _wallJumpStartTime;
  private int _lastWallJumpDir;

  private Vector2 _moveInput;
  public float LastPressedJumpTime { get; private set; }
  public float LastPressedDashTime { get; private set; }

  //Set all of these up in the inspector
  [Header("Checks")] 
    [SerializeField] private Transform _groundCheckPoint;
  //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
  [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
  [Space(5)]
  [SerializeField] private Transform _frontWallCheckPoint;
  [SerializeField] private Transform _pickUpLocation;
  [SerializeField] private Transform _rayCastFrom;
  [SerializeField] private Transform _backWallCheckPoint;
  [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);


  [Header("Layers & Tags")]
  [SerializeField] private LayerMask _groundLayer;

  [SerializeField]
  private SpriteRenderer playerMesh;
  public PlayerConfiguration playerConfig {get; private set; }
  private PlayerControls controls;
  #endregion

  [Header("Crouching")]
  [SerializeField] float _crouchSpeedMultiplier = 0.5f;
  [SerializeField] bool _playerIsCrouching = false;
  [SerializeField] [Range(0.0f, 1.8f)] float _headCheckRadiusMultiplier = 0.9f;
  [SerializeField] float _crouchTimeMulitplier = 10.0f;
  [SerializeField] float _playercrouchedHeightTolerance = 0.05f;
  [SerializeField]float _crouchAmount = 0.5f;
  float _playerFullHeight = 0.0f; // Note: Gets set in Awake()
  float _playerCrouchedHeight = 0.0f;  // Note: Gets set in Awake()
  Vector3 _playerCenterPoint = Vector3.zero;

  [Header("GrabObject")]
  [SerializeField] private LayerMask pickUpLayerMask;
  private PlayerMovement _grabbedBy; 
  public bool IsGrabbed; 
  private ObjectGrabbable objectGrabbable;
  private PlayerMovement playerMovement; 
  private int _punchesLeft; 
  private bool _punchRefilling; 

  private PlayerConfigurationManager playerConfigurationManager; 

  private void Awake()
  {
    RB = GetComponent<Rigidbody2D>();
    _collider = GetComponent<BoxCollider2D>(); 
    _trailRenderer = GetComponent<TrailRenderer>(); 
    controls = new PlayerControls();
    _playerFullHeight = _collider.size.y;
    _playerCrouchedHeight = _playerFullHeight - _crouchAmount;
    _punchesLeft = Data.punchAmount; 
    _dashesLeft = Data.dashAmount; 
    _punchHoldTime = 0.5f;
  }

  private void Start()
  {
    SetGravityScale(Data.gravityScale);
    IsFacingRight = true;
  }

  public void InitializePlayer(PlayerConfiguration config)
  {
    playerConfig = config;
    playerMesh.color = config.playerMaterial.color;
    playerConfig.Input.onActionTriggered += Input_onActionTriggered;
    playerConfigurationManager = PlayerConfigurationManager.Instance;
  }   

  public void OnDestroy(){
    playerConfig.Input.onActionTriggered -= Input_onActionTriggered; 
  }

  public void UpdatePlayerConfig(PlayerConfiguration config){ 
    playerConfig = config;
    playerMesh.color = config.playerMaterial.color;
  }

  private void Input_onActionTriggered(CallbackContext context)
  {
    if(!playerConfigurationManager.isPaused)
    {
      if (context.action.name == controls.Land.Move.name)
      {
        _moveInput = context.ReadValue<Vector2>(); 
        if (IsGrabbed) tryBreakFree(); 
      }

      if (context.action.name == controls.Land.Jump.name)
      {
        if(context.started){
          OnJumpInput(); 
        }
        if(context.performed){
          OnJumpUpInput(); 
        }
      }

      if (context.action.name == controls.Land.Dash.name)
      {
        if(context.started){
          OnDashInput(); 
        }
        breakFree(); 
      }

      if (context.action.name == controls.Land.Crouch.name){ 
        if(context.started){
          Crouch();
        }
        if(context.canceled){
          Uncrouch();
        }
      }

      if (context.action.name == controls.Land.Grab.name){
        if(context.started && objectGrabbable==null){
          GrabObject(); 
        }else if(context.started && objectGrabbable!=null){
          DropObject(); 
        }
      }

      if (context.action.name == controls.Land.Throw.name){
      if(context.started){
        PunchOrThrow(); 
      }

      }

      if (context.action.name == controls.Land.Pause.name){
        if(!playerConfigurationManager.isPaused)
          playerConfigurationManager.PauseGame(playerConfig.PlayerIndex);  
      }
    }
  }

  private void Update()
  {
  #region TIMERS
    LastOnGroundTime -= Time.deltaTime;
    LastOnWallTime -= Time.deltaTime;
    LastOnWallRightTime -= Time.deltaTime;
    LastOnWallLeftTime -= Time.deltaTime;

    LastPressedJumpTime -= Time.deltaTime;
    LastPressedDashTime -= Time.deltaTime;
    LastPunchedTime -= Time.deltaTime; 
    #endregion

    #region COMPUTE VARIABLES 
    _playerCenterPoint = RB.position + _collider.offset;
    #endregion 

    #region INPUT HANDLER
    if (_moveInput.x != 0)
      CheckDirectionToFace(_moveInput.x > 0);
      #endregion


    #region COLLISION CHECKS
    if (!IsDashing && !IsJumping)
    {
      //Ground Check
      if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) //checks if set box overlaps with ground
      {
        LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
      }		

      //Right Wall Check
      if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
            || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
        LastOnWallRightTime = Data.coyoteTime;

      //Left Wall Check
      if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
            || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
        LastOnWallLeftTime = Data.coyoteTime;

      //Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
      LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
    }
    #endregion

    #region JUMP CHECKS
    if (IsJumping && RB.velocity.y < 0)
    {
      IsJumping = false;

      if(!IsWallJumping)
        _isJumpFalling = true;
    }

    if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
    {
      IsWallJumping = false;
    }

    if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
    {
      _isJumpCut = false;

      if(!IsJumping)
        _isJumpFalling = false;
    }

    if(!IsDashing){ 

      //Jump
      if (CanJump() && LastPressedJumpTime > 0)
      {
        IsJumping = true;
        IsWallJumping = false;
        _isJumpCut = false;
        _isJumpFalling = false;
        Jump();
      }
      //WALL JUMP
      else if (CanWallJump() && LastPressedJumpTime > 0)
      {
        IsWallJumping = true;
        IsJumping = false;
        _isJumpCut = false;
        _isJumpFalling = false;
        _wallJumpStartTime = Time.time;
        _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

        WallJump(_lastWallJumpDir);
      }
    }
    #endregion

    #region DASH CHECKS 
    if(CanDash() && LastPressedDashTime > 0 && !IsDashSleeping){
      if(_moveInput != Vector2.zero)
        _lastDashDir = _moveInput; 
      else 
        _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left; 

      IsDashing = true; 
      IsJumping = true; 
      IsWallJumping = true; 
      _isJumpCut = false; 

      StartCoroutine(nameof(StartDash), _lastDashDir); 
      StartCoroutine(nameof(SleepDash)); 
    }
    #endregion 

    #region SLIDE CHECKS
    if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
      IsSliding = true;
    else
      IsSliding = false;
      #endregion



    #region GRAVITY
    if(!_isDashAttacking)
    {
      //Higher gravity if we've released the jump input or are falling
      if (IsSliding)
      {
        SetGravityScale(0);
      }
      else if (RB.velocity.y < 0 && _moveInput.y < 0)
      {
        //Much higher gravity if holding down
        SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
        //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
        RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
      }
      else if (_isJumpCut)
      {
        //Higher gravity if jump button released
        SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
        RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
      }
      else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
      {
        SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
      }
      else if (RB.velocity.y < 0)
      {
        //Higher gravity if falling
        SetGravityScale(Data.gravityScale * Data.fallGravityMult);
        //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
        RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
      }
      else
      {
        //Default gravity if standing on a platform or moving upwards
        SetGravityScale(Data.gravityScale);
      }
    }
    #endregion
  }

  private void FixedUpdate()
  {
    if(!IsDashing){
      //Handle Run
      if (IsWallJumping)
        Run(Data.wallJumpRunLerp);
      else
        Run(1);
    }else if(_isDashAttacking){
      Run(Data.dashEndRunLerp); 
    }

    //Handle Slide
    if (IsSliding)
      Slide();
  }

  #region INPUT CALLBACKS
  //Methods which whandle input detected in Update()
  public void OnJumpInput()
  {
    LastPressedJumpTime = Data.jumpInputBufferTime;
  }

  public void OnJumpUpInput()
  {
    if (CanJumpCut() || CanWallJumpCut())
      _isJumpCut = true;
  }
  public void OnDashInput()
  {
    LastPressedDashTime = Data.dashInputBufferTime;
  }
  #endregion

  #region GENERAL METHODS
  public void SetGravityScale(float scale)
  {
    RB.gravityScale = scale;
  }
  private void Sleep(float duration)
  {
    //Method used so we don't need to call StartCoroutine everywhere
    //nameof() notation means we don't need to input a string directly.
    //Removes chance of spelling mistakes and will improve error messages if any
    StartCoroutine(nameof(PerformSleep), duration);
  }

  private IEnumerator PerformSleep(float duration)
  {
    Time.timeScale = 0;
    yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
    Time.timeScale = 1;
  }
  #endregion

  //MOVEMENT METHODS
  #region RUN METHODS
  private void Run(float lerpAmount)
  {
    //Calculate the direction we want to move in and our desired velocity
    float targetSpeed = _moveInput.x * Data.runMaxSpeed;
    //We can reduce are control using Lerp() this smooths changes to are direction and speed
    targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

    #region Calculate AccelRate
    float accelRate;

    //Gets an acceleration value based on if we are accelerating (includes turning) 
    //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
    if (LastOnGroundTime > 0)
      accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
    else
      accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
      #endregion

    #region Add Bonus Jump Apex Acceleration
    //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
    if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
    {
      accelRate *= Data.jumpHangAccelerationMult;
      targetSpeed *= Data.jumpHangMaxSpeedMult;
    }
    #endregion

    #region Conserve Momentum
    //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
    if(Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
    {
      //Prevent any deceleration from happening, or in other words conserve are current momentum
      //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
      accelRate = 0; 
    }
    #endregion

    //Calculate difference between current velocity and desired velocity
    float speedDif = targetSpeed - RB.velocity.x;
    //Calculate force along x-axis to apply to thr player
    if(_playerIsCrouching){
      speedDif *= _crouchSpeedMultiplier; 
    }
    float movement = speedDif * accelRate;

    //Convert this to a vector and apply to rigidbody
    RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
  }

  private void Turn()
  {
    //stores scale and flips the player along the x axis, 
    Vector3 scale = transform.localScale; 
    scale.x *= -1;
    transform.localScale = scale;

    IsFacingRight = !IsFacingRight;
  }
  #endregion

  #region CROUCH METHODS
  private void Crouch()
  {
    if (_collider.size.y >= _playerCrouchedHeight + _playercrouchedHeightTolerance)
    {
      float time = Time.fixedDeltaTime * _crouchTimeMulitplier;
      float amount = Mathf.Lerp(0.0f, _crouchAmount, time);

      Vector2 tmp = _collider.size ;
      _collider.size = new Vector2(tmp.x, tmp.y - amount);
      _collider.offset = new Vector2(_collider.offset.x, _collider.offset.y + (amount * 0.5f));
      RB.position = new Vector2(RB.position.x, RB.position.y - amount);

      _playerIsCrouching = true;
    }
    else
    {
      EnforceExactCharHeight();
    }
  }

  private void Uncrouch()
  {
    if(_collider.size.y < _playerFullHeight - _playercrouchedHeightTolerance)
    {
      float sphereCastRadius = _collider.size.x * _headCheckRadiusMultiplier;
      float headroomBufferDistance = 0.05f;
      float sphereCastTravelDistance = (_collider.bounds.extents.y + headroomBufferDistance) - sphereCastRadius;
      if (!(Physics.SphereCast(_playerCenterPoint, sphereCastRadius, RB.transform.up, out _, sphereCastTravelDistance)))
      {
        float time = Time.fixedDeltaTime * _crouchTimeMulitplier;
        float amount = Mathf.Lerp(0.0f, _crouchAmount, time);

        Vector2 tmp = _collider.size; 
        _collider.size = new Vector2(tmp.x, tmp.y + amount);
        _collider.offset = new Vector2(_collider.offset.x, _collider.offset.y - (amount * 0.5f));
        RB.position = new Vector2(RB.position.x, RB.position.y + amount);
      }
    }
    else
    {
      _playerIsCrouching = false;
      EnforceExactCharHeight();
    }
  }

  private void EnforceExactCharHeight()
  {
    if (_playerIsCrouching)
    {
      Vector2 var = _collider.size ; 
      _collider.size = new Vector2(var.x,_playerCrouchedHeight) ; 
      _collider.offset = new Vector2(0.0f, _crouchAmount * 0.5f);
    }
    else
    {
      Vector2 var = _collider.size ; 
      _collider.size = new Vector2(var.x,_playerFullHeight) ; 
      _collider.offset = Vector2.zero;
    }
  }
  #endregion

  #region JUMP METHODS
  private void Jump()
  {
    //Ensures we can't call Jump multiple times from one press
    LastPressedJumpTime = 0;
    LastOnGroundTime = 0;

    #region Perform Jump
    //We increase the force applied if we are falling
    //This means we'll always feel like we jump the same amount 
    //(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
    float force = Data.jumpForce;
    if (RB.velocity.y < 0)
      force -= RB.velocity.y;

    RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    #endregion
  }

  private void WallJump(int dir)
  {
    //Ensures we can't call Wall Jump multiple times from one press
    LastPressedJumpTime = 0;
    LastOnGroundTime = 0;
    LastOnWallRightTime = 0;
    LastOnWallLeftTime = 0;

    #region Perform Wall Jump
    Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
    force.x *= dir; //apply force in opposite direction of wall

    if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
      force.x -= RB.velocity.x;

    if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
      force.y -= RB.velocity.y;

    //Unlike in the run we want to use the Impulse mode.
    //The default mode will apply are force instantly ignoring masss
    RB.AddForce(force, ForceMode2D.Impulse);
    #endregion
  }
  #endregion

  #region DASH 

  private IEnumerator SleepDash(){
    //We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
    float startTime = Time.time;
    IsDashSleeping = true; 
    while (Time.time - startTime <= Data.dashSleepTime)
    {
      yield return null;
    }
    IsDashSleeping = false; 
  }
  //Dash Coroutine
  private IEnumerator StartDash(Vector2 dir)
  {
    //Overall this method of dashing aims to mimic Celeste, if you're looking for
    // a more physics-based approach try a method similar to that used in the jump

    LastOnGroundTime = 0;
    LastPressedDashTime = 0;

    float startTime = Time.time;

    _dashesLeft--;
    _isDashAttacking = true;

    SetGravityScale(0);

    //We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
    while (Time.time - startTime <= Data.dashAttackTime)
    {
      RB.velocity = dir.normalized * Data.dashSpeed;
      //Pauses the loop until the next frame, creating something of a Update loop. 
      //This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
      yield return null;
    }

    startTime = Time.time;

    _isDashAttacking = false;

    //Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
    SetGravityScale(Data.gravityScale);
    RB.velocity = Data.dashEndSpeed * dir.normalized;

    while (Time.time - startTime <= Data.dashEndTime)
    {
      yield return null;
    }

    //Dash over
    IsDashing = false;
    // Fix Bug for in-ground dash
    IsJumping = false; 
  }

  //Short period before the player is able to dash again
  private IEnumerator RefillDash(int amount)
  {
    //SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
    _dashRefilling = true;
    yield return new WaitForSeconds(Data.dashRefillTime);
    _dashRefilling = false;
    _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
  }

  private IEnumerator RefillPunches(int amount)
  {
    //SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
    _punchRefilling = true;
    yield return new WaitForSeconds(Data.punchRefillTime);
    _punchRefilling = false;
    _punchesLeft = Mathf.Min(Data.punchAmount, _punchesLeft + 1);
  }
  #endregion

  #region OTHER MOVEMENT METHODS
  private void Slide()
  {
    //Works the same as the Run but only in the y-axis
    //THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
    float speedDif = Data.slideSpeed - RB.velocity.y;	
    float movement = speedDif * Data.slideAccel;
    //So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
    //The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
    movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif)  * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

    RB.AddForce(movement * Vector2.up);
  }
  #endregion


  #region CHECK METHODS
  public void CheckDirectionToFace(bool isMovingRight)
  {
    if (isMovingRight != IsFacingRight)
      Turn();
  }

  private bool CanJump()
  {
    return LastOnGroundTime > 0 && !IsJumping;
  }

  /* [WARNING] The position of the Collider can cause LastOnWallTime > 0 and therefore activate 
     This function in air (without a wall nearby) causing the player to be launched 
     */ 
  private bool CanWallJump()
  {
    return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && 
      (!IsWallJumping || (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) 
       || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
  }

  private bool CanJumpCut()
  {
    return IsJumping && RB.velocity.y > 0;
  }

  private bool CanWallJumpCut()
  {
    return IsWallJumping && RB.velocity.y > 0;
  }

  public bool CanSlide()
  {
    if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && LastOnGroundTime <= 0)
      return true;
    else
      return false;
  }

  private bool CanDash()
  {
    if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
    {
      StartCoroutine(nameof(RefillDash), 1);
    }

    return _dashesLeft > 0;
  }

  #endregion


  #region EDITOR METHODS
  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
    Gizmos.color = Color.blue;
    Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
    Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
  }
  private void OnDrawGizmos(){
    Gizmos.color = Color.green;
    Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
    Gizmos.color = Color.red;
    Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
    Gizmos.color = Color.blue;
    Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
  }
  #endregion

  #region GRAB OBJECT
  private RaycastHit2D getObjectInRay(){
    var dir = IsFacingRight ? Vector2.right : Vector2.left;
    Debug.DrawRay(_rayCastFrom.transform.position, dir * Data.pickUpDistance, Color.white);
    RaycastHit2D hitInfo = Physics2D.Raycast(
        _rayCastFrom.transform.position,
        dir,
        Data.pickUpDistance,
        pickUpLayerMask
        );
    return hitInfo; 
  }
  private void GrabObject(){
    if (LastPunchedTime > 0) return; 
    RaycastHit2D hitInfo = getObjectInRay(); 
    if (hitInfo.collider!=null) {
      if (hitInfo.transform.TryGetComponent(out objectGrabbable)) {
        objectGrabbable.Grab(_pickUpLocation.transform, hitInfo.transform, this);
        if (!IsGrabbed) objectGrabbable = null; 
      }
    }
  }

  public void setGrabbed(PlayerMovement grabbedBy){
    IsGrabbed = true; 
    _grabbedBy = grabbedBy; 
  } 

  public void setDropped(){
    IsGrabbed = false; 
    _grabbedBy = null; 
  }

  public bool ProbabilityCheck(float itemProbability)
  {
    float rnd = Random.Range(0f, 1f);
    if (rnd <= itemProbability)
      return true;
    else
      return false;
  }

  private void tryBreakFree(){
    if(ProbabilityCheck(Data.breakFreeProbability))
      breakFree();
  }

  private void breakFree(){
    if(!IsGrabbed) return; 
    if(!_grabbedBy) return; 
    _grabbedBy.DropObject();
  }

  private void DropObject(){
    // Currently carrying something, drop
    // Sanity check 
    if(objectGrabbable==null) return; 
    objectGrabbable.Drop();
    objectGrabbable = null;
  }
  #endregion GRAB OBJECT
  #region THROW OBJECT 
  private void PunchOrThrow(){
    LastPunchedTime = _punchHoldTime; 
    if (_punchesLeft < Data.punchAmount && !_punchRefilling)
    {
      StartCoroutine(nameof(RefillPunches), 1);
    }
    if (_punchesLeft > 0) {
      var dir = IsFacingRight ? Vector2.right : Vector2.left;
      if (objectGrabbable == null){
        RaycastHit2D hitInfo = getObjectInRay(); 
        if(hitInfo.collider != null){
          if(hitInfo.transform.TryGetComponent(out playerMovement)) {
            if(playerMovement != this) 
              playerMovement.isPushed(dir, Data.myStrength);
          }
        }
      }else{
        objectGrabbable.isPushed(dir, Data.myStrength); 
      }
      _punchesLeft--; 
    }
  }

  public void isPushed(Vector2 pushedDir, Vector2 strength){
    //Convert this to a vector and apply to rigidbody
    Vector2 upDir = RB.velocity.y >= 0 ? Vector2.up : Vector2.down; 
    Vector2 endVec = strength * pushedDir; 
    endVec.y *= upDir.y; 
    breakFree(); 
    RB.AddForce(endVec, ForceMode2D.Force);
  }

  #endregion THROW OBJECT  
}
