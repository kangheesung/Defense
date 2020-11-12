using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour {
    private enum pStateEnum {
        normal, bleeding
    };
    private pStateEnum pstate;
    public bool pReloadORGrenadeAct;    //  reloadORGrenade 상태 확인 변수

    //  Health
    [Header("Health")]
    public int maxHealth = 100;
    private int curHealth;
    public Slider healthSlider;
    public Image healthSliderFill;
    private Color fullHealthColor = Color.green;
    private Color zeroHealthColor = Color.red;
    public int healthPlusAmount = 0;
    public float healthPlusDelay = 0.5f;
    private float healthTimer;
    //----------

    //  Stamina
    [Header("Stamina")]
    public int maxStamina = 50;
    [HideInInspector]
    public int curStamina;
    public Slider staminaSlider;
    public Image staminaSliderFill;
    public int staminaPlusAmount = 1;
    public float staminaPlusDelay = 0.5f;
    private float staminaTimer;
    //----------

    //  Bleeding
    private int bleedingDamage;
    private int bleedingDelay;
    private int bleedingCount;
    private int bleedingMaxCount;
    //----------

    //  Reference
    private AudioSource Audio;
    private Animator anim;
    private CameraShake cs;
    //----------

    private void Awake() {
        //  초기화
        curHealth = maxHealth;
        curStamina = maxStamina;
        //  참조
        anim = GetComponent<Animator>();
        cs = FindObjectOfType<CameraShake>();
    }

    private void Update() {
        if (Input.GetKey(KeyCode.V)) {
            takeHealthDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            setBleeding(1, 1, 5);
        }

        if (Input.GetKeyDown(KeyCode.H)) {
            bleedingOff();
        }

        staminaTimer += Time.deltaTime;

        //  스태미너 자연회복
        if (staminaTimer >= staminaPlusDelay && !anim.GetBool("sprint") && curStamina < maxStamina) {
            curStamina += staminaPlusAmount;
            setStaminaUi();
            staminaTimer = 0f;
        }
        //  체력 자연회복
        if (healthTimer >= healthPlusDelay && curHealth < maxHealth) {
            curHealth += healthPlusAmount;
            setHealthUi();
            healthTimer = 0f;
        }
    }
    
    public void bleedingOff() {
        //  출혈 치료
        pstate = pStateEnum.normal;
        //
    }

    public void setBleeding(int damage, int delay, int max) {
        // 출혈 설정(데미지, 딜레이, 횟수)
        if (pstate == pStateEnum.bleeding) {
            //
            return;
        }
        pstate = pStateEnum.bleeding;
        bleedingDamage = damage;
        bleedingDelay = delay;
        bleedingMaxCount = max;
        bleedingCount = 0;

        bleeding();
    }

    private void bleeding() {
        if (bleedingCount >= bleedingMaxCount || pstate == pStateEnum.normal) {
            pstate = pStateEnum.normal;
            return;
        }
        bleedingCount++;
        takeHealthDamage(bleedingDamage);
        Invoke("bleeding", bleedingDelay);
    }

    private void setHealthUi() {
        healthSlider.value = curHealth;
        healthSliderFill.color = Color.Lerp(zeroHealthColor, fullHealthColor, curHealth / maxHealth);
    }

    private void setStaminaUi() {
        staminaSlider.value = curStamina;
    }

    public void takeHealthDamage(int amount) {
        // 체력 감소(양)
        curHealth -= amount;
        setHealthUi();
        cs.Shake(5f, 0.2f);
        anim.SetTrigger("damage");
    }

    public void takeStaminaDamage(int amount) {
        // 스테미너 감소(양)
        curStamina -= amount;
        setStaminaUi();
    }
}