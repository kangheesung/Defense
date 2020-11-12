using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    //  Movement
    [Header("Movement")]
    private Vector3 movement;
    //  ----------

    //  Speed
    [Header("Movement Speed")]
    public float normalSpeed = 5f;
    public float sprintSpeedPlus = 2f;
    public float crouchSpeedMinus = 2f;
    public float normalRotateSpeed = 8f;
    public float rotateSpeed = 8f;
    public float movementDeltaSpeed = 5f;   //  이동속도가 바뀌는 속도
    private float tempCurSpeed;             //  속도Lerp용 임시 변수
    private float curSpeed;
    public float sideMovementSpeedMinus = 1f;   //  옆걸음 이동속도 감소량
    public float backMovementSpeedMinus = 2f;   //  뒷걸음 이동속도 감소량

    //  ----------

    //  Stamina
    [Header("Stamina")]
    public float staminaMinusDelay = 0.1f;
    private float staminaTimer;
    [HideInInspector]
    public bool sprintDisable;      //  달리기 비활성화
    private bool aimSprintDisable;  //  aim달리기 비활성화
    //  ----------

    //  Aim
    [Header("Aim & Ik")]
    public GameObject Aim;
    public GameObject aimPoint;     //  aimray가 오브젝트에 맞았을때 표시될 오브젝트
    public Transform AimFinalPos;
    public Transform WeaponRIk;

    [HideInInspector]
    public bool aimRotate;      //  aim위치로 회전을 위한 bool변수
    //  ----------

    [HideInInspector]
    public bool disableMovement = false;

    //  Reference
    private Animator anim;
    private Rigidbody playerRigidbody;
    private AudioSource Audio;
    [HideInInspector]
    public int floorMask;       //  바닥 레이어 번호
    private float camRayLength = 100f;
    private Aim aim;
    private PlayerState ps;
    private InputManager im;
    private PlayerWeaponManager pwm;
    private PlayerIk pik;
    //  ----------

    private Quaternion tempRotation;    //  회전lerp용 임시 변수

    private void Awake() {
        //  초기화
        curSpeed = normalSpeed;
        tempRotation = transform.rotation;
        AimFinalPos = Aim.transform.GetChild(0).GetChild(0);

        //  레이어 마스크 설정
        floorMask = LayerMask.GetMask("PostProcessing");

        //  참조 설정
        anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        aim = FindObjectOfType<Aim>();
        ps = FindObjectOfType<PlayerState>();
        im = FindObjectOfType<InputManager>();
        pwm = FindObjectOfType<PlayerWeaponManager>();
        pik = FindObjectOfType<PlayerIk>();
    }

    private void Update() {
        staminaTimer += Time.deltaTime;
    }

    private void LateUpdate() {
        aimPoint.SetActive(false);
        float h = Input.GetAxisRaw(im.HorizontalAxis);
        float v = Input.GetAxisRaw(im.VerticalAxis);
        for (int i = 0; i < im.Aim.Length; i++) {
            if (Input.GetKey(im.Aim[i]) || aimRotate) {
                AimTurning();   //  aim으로 회전
                break;
            } else {
                Turnning(h, v); //  키보드 8방향으로 회전
                break;
            }
        }
    }

    private void FixedUpdate() {
        //  input axes 저장
        float h = Input.GetAxisRaw(im.HorizontalAxis);
        float v = Input.GetAxisRaw(im.VerticalAxis);

        resetAnim();
        if (!disableMovement) {
            Move(h, v);
        }
        curSpeed = Mathf.Lerp(curSpeed, tempCurSpeed, movementDeltaSpeed * Time.deltaTime);
        AimLookAt();    //  aim 이동, 회전
    }

    private void resetAnim() {
        aim.aimReduce(false);   //  에임 분산 애니메이션
        anim.SetBool("crouch", false);
        anim.SetBool("sprint", false);
    }

    private void Move(float h, float v) {
        Vector2 input = new Vector2(h, v);

        if (input.sqrMagnitude > 1) input.Normalize();  //  이동 벡터 정규화

        movement = transform.forward * input.y + transform.right * input.x;

        movement = transform.InverseTransformDirection(movement);   //  로컬 벡터를 월드로 바꿈

        animating(h, v);

        transform.position += movement * curSpeed * Time.deltaTime;
    }

    public void rollActive(bool act) {
        //  구르기
        pwm.curWeaponTransform.GetComponent<PlayerWeapon>().DisableShootingInput = act;
        disableMovement = act;
        if (act == false) { //  구르기가 끝났을 때 reloadORGrenadeAct 상태라면 사격 비활성화
            if (ps.pReloadORGrenadeAct == true) {
                pwm.curWeaponTransform.GetComponent<PlayerWeapon>().DisableShootingInput = true;
            }
        }
    }

    private void animating(float h, float v) {
        //  이동 anim, 속도 조절
        Vector3 localMove = transform.InverseTransformDirection(movement);  //  월드 벡터를 로컬로 바꿈

        if (localMove.z < -0.3f) {
            localMove.z = -1;
        } else if (localMove.z > 0.3f) {
            localMove.z = 1;
        }

        float turnAmount = localMove.x;
        float forwardAmount = localMove.z;

        if (forwardAmount < 0)  //  뒷걸음시 옆으로 이동하는 애니메이션 방향을 바꿈
            turnAmount *= -1;

        //  속도
        if (forwardAmount == 1) {
            tempCurSpeed = normalSpeed;
        } else if (forwardAmount == -1) {
            tempCurSpeed = normalSpeed - backMovementSpeedMinus;
        } else if (Mathf.Abs(turnAmount) != 1) {
            tempCurSpeed = normalSpeed - sideMovementSpeedMinus;
        }

        //  구르기
        for (int i = 0; i < im.Roll.Length; i++) {
            if (Input.GetKeyDown(im.Roll[i])) {
                if (ps.curStamina >= 10) {
                    rollActive(true);
                    ps.takeStaminaDamage(10);
                    anim.SetTrigger("Roll");
                    break;
                }
            }
        }
        //  앉기
        for (int i = 0; i < im.Crouch.Length; i++) {
            if (Input.GetKey(im.Crouch[i])) {
                anim.SetBool("crouch", true);
                aim.aimReduce(true);    //  에임 밀집 애니메이션
                tempCurSpeed = normalSpeed - crouchSpeedMinus;  //  속도 감소
                break;
            }
        }
        //  에임
        for (int i = 0; i < im.Aim.Length; i++) {
            if (Input.GetKey(im.Aim[i])) {
                aimSprintDisable = true;
                break;
            } else {
                aimSprintDisable = false;
            }
        }
        //  달리기
        for (int i = 0; i < im.Sprint.Length; i++) {
            if (Input.GetKey(im.Sprint[i]) && sprintDisable == false && aimSprintDisable == false) {
                if ((h != 0 || v != 0) && !anim.GetBool("crouch")) {    //  정지, 앉은 상태가 아님
                    anim.SetBool("sprint", true);
                    tempCurSpeed = normalSpeed + sprintSpeedPlus;   //  속도 증가
                    if (staminaTimer >= staminaMinusDelay && ps.curStamina > 0) {
                        staminaTimer = 0f;
                        ps.takeStaminaDamage(1);
                        if (ps.curStamina <= ps.maxStamina / 2) {
                            //  기력이 50% 이하

                        }
                        if (ps.curStamina <= 0) {
                            //  기력이 0 이하
                            sprintDisable = true;
                            ps.staminaSliderFill.color = new Color(255f / 255f, 0f / 255f, 0f / 255f, 255f / 255f); //  색 변경 (빨강)
                            Invoke("sprintOn", 3f); //  3초 뒤 sprintOn 실행
                        }
                    }
                }
                break;
            }
        }
        ConvertMoveInput();
    }

    private void sprintOn() {
        // 달리기 활성화
        sprintDisable = false;
        ps.staminaSliderFill.color = new Color(0f / 255f, 100f / 255f, 255f / 255f, 255f / 255f);   //  색 복구
    }

    private void ConvertMoveInput() {
        Vector3 localMove = transform.InverseTransformDirection(movement);

        float tempz = .8f;

        if (tempCurSpeed == normalSpeed) {
            tempz = .8f;
        }
        else if (tempCurSpeed == normalSpeed + sprintSpeedPlus) {
            tempz = 1f;
        }
        else if (tempCurSpeed == normalSpeed - sideMovementSpeedMinus) {
            tempz = .8f;
        }
        else if (tempCurSpeed == normalSpeed - backMovementSpeedMinus) {
            tempz = .7f;
        }
        else if (tempCurSpeed == normalSpeed - crouchSpeedMinus) {
            tempz = .6f;
        }
        

        if (localMove.z < -0.3f) {
            localMove.z = -tempz;
        } else if (localMove.z > 0.3f) {
            localMove.z = tempz;
        }

        float turnAmount = localMove.x;
        float forwardAmount = localMove.z;

        if (forwardAmount < 0) {    //  뒷걸음시 옆으로 이동하는 애니메이션 방향을 바꿈
            turnAmount *= -1;
        }

        anim.SetFloat("forward", forwardAmount, 0.15f, Time.deltaTime);
        anim.SetFloat("sideway", turnAmount, 0.15f, Time.deltaTime);
    }

    public void AimLookAt() {
        //  AIM의 위치를 마우스 위치로 하고 캐릭터를 바라봄
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, camRayLength, floorMask)) {
            Aim.transform.position = floorHit.point;
        }
        Aim.transform.LookAt(transform);
        Aim.transform.localEulerAngles = new Vector3(0, Aim.transform.localEulerAngles.y, 0);
    }

    private void AimTurning() {
        //  에임으로 캐릭터 회전
        AimLine();
        Vector3 playerToMouse = AimFinalPos.position - transform.position;
        playerToMouse.y = 0f;
        Quaternion ToRotation = Quaternion.LookRotation(playerToMouse);
        if ((int)transform.rotation.eulerAngles.y != (int)ToRotation.eulerAngles.y) {
            transform.rotation = Quaternion.Lerp(transform.rotation, ToRotation, rotateSpeed * Time.deltaTime);
        }
        //  회전이 +-20 안으로 완료되면 WeaponR을 AimFinalPos로 회전
        if (!anim.GetBool("Knife")) {
            if (!pwm.curWeaponTransform.GetComponent<PlayerWeapon>().DisableShootingInput) {
                if (
                    (int)transform.rotation.eulerAngles.y >= (int)ToRotation.eulerAngles.y - 20 &&
                    (int)transform.rotation.eulerAngles.y <= (int)ToRotation.eulerAngles.y + 20
                    ) {
                    WeaponRIk.LookAt(AimFinalPos);
                }
            }
        }
    }

    private void AimLine() {
        //  에임 Line 오브젝트와 닿으면 aimPoint 활성화
        if (pwm.curWeaponTransform.GetComponent<PlayerWeapon>().gunMuzzle == null) {
            return;
        }
        Transform tempMuzzle = pwm.curWeaponTransform.GetComponent<PlayerWeapon>().gunMuzzle.transform;
        Debug.DrawRay(tempMuzzle.position, transform.forward * 100);
        RaycastHit Hit;
        if (Physics.Raycast(tempMuzzle.position, transform.forward, out Hit, 100f, floorMask)) {
            aimPoint.SetActive(true);
            aimPoint.transform.position = Hit.point;
        }
    }

    private void Turnning(float h, float v) {
        //  키보드 8방향으로 캐릭터 회전
        if (!pwm.curWeaponTransform.GetComponent<PlayerWeapon>().DisableShootingInput) { //  사격 비활성화가 아니라면 WeaponRIk가 앞을 보게함
            WeaponRIk.localRotation = Quaternion.Lerp(WeaponRIk.localRotation, Quaternion.Euler(0, -90, 180), rotateSpeed * Time.deltaTime);
        }
        if (h < 0 && v == 0) {
            //  left
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, -90, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
        else if (h > 0 && v == 0) {
            //  right
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 90, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
        else if (h == 0 && v < 0) {
            //  down
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 180, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
        else if (h == 0 && v > 0) {
            //  up
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
        else if (h < 0 && v < 0) {
            //  left down
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, -135, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
        else if (h < 0 && v > 0) {
            //  left up
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, -45, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
        else if (h > 0 && v < 0) {
            //  right down
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 135, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
        else if (h > 0 && v > 0) {
            //  right up
            tempRotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 45, 0), rotateSpeed * Time.deltaTime);
            transform.rotation = tempRotation;
        }
    }
}