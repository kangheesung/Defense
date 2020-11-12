using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Defense;

public class PlayerWeapon : MonoBehaviour {
    public int weaponCode;
    public Weapon weapon;       //  무기 종류
    public bool weaponClone;    //  같은 무기 확인 clone 변수
    public bool weaponSling;    //  무기 운반 상태 확인 변수
    public WeaponStats weaponStats; //  weaponStats 데이터
    private string weaponName = "Rifle";
    private string weaponDesc = "";
    private weaponFireType weaponFireType;  //  발사 타입
    private weaponType weaponType;  //  무기 타입 (ray, collider, projectile)
    private bool weaponMuzzle;
    private bool weaponLowerMount;
    private bool weaponSight;
    private bool weaponMagazine;
    public Transform weaponIkOffset;

    public List<Transform> weaponAttachmentData;    //  Database Attachment Transform

    public List<Transform> weaponAttachmentTransform;   //  Weapon Attachment Transform

    public GameObject bulletPrefab;
    public GameObject weaponPrefab;
    public GameObject weaponUIPrefab;

    //  Stats
    private float timeBetweenBullets;   //  연사 속도
    private int projectileCount;        //  투사체 개수
    private int damagePerShot;          //  데미지
    private float bulletSpeed;          //  투사체 속도
    private float bulletAcceleration;   //  투사체 가속
    private float bulletTimeToLive;     //  투사체 시간
    private int bulletCount;            //  투사체 수
    public bool usePooling = true;
    public int poolingMulti = 1;
    public Transform ShellSpitOffset;   //  탄피 배출 오프셋
    //  Spread
    private float minSpreadX;
    private float minSpreadY;
    private float maxSpreadX;
    private float maxSpreadY;
    private float deltaSpread;
    private float minusSpread;
    private bool spreadMinusOff;    //  spread 감소 비활성화
    private float curSpreadX;
    private float curSpreadY;
    private float spreadTime;
    private float spreadTimer;
    //  VFX
    private ParticleSystem gunParticle;
    public GameObject gunMuzzle;
    public LineRenderer gunLine;
    private Light[] gunLight;

    [HideInInspector]
    public bool DisableShootingInput = false;

    [HideInInspector]
    public bool meleeAttack = false;    //  근접 공격 변수

    private float timer;
    private float effectsDisplayTime = 0.2f;
    
    //  Bullet
    private int ActualGameBullet = 0;   //  현재 탄환
    private GameObject[] GameBullets;   //  pooling 탄환 리스트
    //  ----------

    //  Reference
    private Animator anim;
    private AudioSource gunAudio;
    private Aim aim;
    private InputManager im;
    private CameraShake cs;
    private PlayerMovement pm;
    private PlayerWeaponManager pwm;
    private PlayerState ps;
    private PlayerIk pik;
    //  ----------

    private void Awake() {
        curSpreadX = minSpreadX;
        curSpreadY = minSpreadY;

        //  참조 설정
        anim = transform.root.GetComponent<Animator>();
        gunAudio = transform.root.GetComponent<AudioSource>();
        aim = FindObjectOfType<Aim>();
        im = FindObjectOfType<InputManager>();
        cs = FindObjectOfType<CameraShake>();
        pm = FindObjectOfType<PlayerMovement>();
        pwm = FindObjectOfType<PlayerWeaponManager>();
        ps = FindObjectOfType<PlayerState>();
        pik = FindObjectOfType<PlayerIk>();
    }

    private void Start() {
        //  weaponAttachmentTransform 설정
        for (int i = 0; i < this.transform.childCount; i++) {
            if (this.transform.GetChild(i).GetComponent<MeshRenderer>() != null) {
                for (int j = 0; j < this.transform.GetChild(i).GetChild(0).childCount; j++) {
                    weaponAttachmentTransform.Add(this.transform.GetChild(i).GetChild(0).GetChild(j));
                }
                break;
            }
        }
        //  weaponAttachmentData 설정
        for (int i = 0; i < weaponAttachmentTransform.Count; i++) {
            for (int j = 0; j < ItemDatabase.instance.transform.GetChild(0).childCount; j++) {
                if (weaponAttachmentTransform[i].name == ItemDatabase.instance.transform.GetChild(0).GetChild(j).name) {
                    weaponAttachmentData.Add(ItemDatabase.instance.transform.GetChild(0).GetChild(j));
                }
            }
        }
        //  weaponStats 데이터로 변수 초기화
        GetWeaponStats();

        if (weaponType == weaponType.Collider) return;
        //  총구 파티클 설정
        if (gunParticle != null) {
            gunParticle = Instantiate(weaponStats.gunParticle, gunMuzzle.transform.position, gunMuzzle.transform.rotation);
            gunParticle.transform.SetParent(gunMuzzle.transform);
        }
        //  pooling 설정
        if (usePooling) {
            GameBullets = new GameObject[bulletCount * projectileCount * poolingMulti];
            GameObject BulletsParent = new GameObject(weaponName + "_Bullets");

            for (int i = 0; i < (bulletCount * projectileCount * poolingMulti); i++) {
                GameBullets[i] = Instantiate(bulletPrefab, gunMuzzle.transform.position, gunMuzzle.transform.rotation) as GameObject;
                GameBullets[i].SetActive(false);
                GameBullets[i].name = weaponName + "_Bullet_" + i.ToString();
                GameBullets[i].transform.parent = BulletsParent.transform;

                GameBullets[i].GetComponent<Projectile>().usePooling = true;
                GameBullets[i].GetComponent<Projectile>().setBullet(damagePerShot, bulletSpeed, bulletAcceleration, bulletTimeToLive);
            }
        }
    }

    private void Update() {
        //  Aim 높이 총구 높이로 설정
        if (gunMuzzle != null)
            aim.aimHeight(gunMuzzle.transform.position.y);
        timer += Time.deltaTime;
        spreadTimer += Time.deltaTime;

        spreadMinusOff = false;

        //  이펙트 종료
        if (timer >= timeBetweenBullets * effectsDisplayTime) {
            DisableEffects();
        }
        
        if (DisableShootingInput || weaponSling) {
            return;
        }
        //  단발
        if (weaponFireType == weaponFireType.Single) {
            for (int i = 0; i < im.Fire.Length; i++) {
                for (int j = 0; j < im.Aim.Length; j++) {
                    if (Input.GetKeyDown(im.Fire[i]) && Input.GetKey(im.Aim[j]) && timer >= timeBetweenBullets) {
                        if (!anim.GetBool("sprint")) {
                            Shoot();
                            aim.aimShake();
                            cs.Shake(3f, 0.2f);
                            anim.SetTrigger("shoot");
                        }
                        break;
                    }
                }
            }
        //  자동
        } else if (weaponFireType == weaponFireType.Auto) {
            for (int i = 0; i < im.Fire.Length; i++) {
                for (int j = 0; j < im.Aim.Length; j++) {
                    if (Input.GetKey(im.Fire[i]) && Input.GetKey(im.Aim[j]) && timer >= timeBetweenBullets) {
                        if (!anim.GetBool("sprint")) {
                            Shoot();
                            aim.aimShake();
                            cs.Shake(3f, 0.2f);
                            anim.SetTrigger("shoot");
                        }
                        break;
                    }
                }
            }
        }
        //  재장전
        for (int i = 0; i < im.Reload.Length; i++) {
            if (Input.GetKeyDown(im.Reload[i])) {
                if (!anim.GetBool("sprint") && pwm.curWeaponTransform != pwm.Knife) {
                    anim.SetTrigger("reload");
                    shootingActive(false);
                }
                break;
            }
        }
        //  Spread
        if (spreadMinusOff == true) {
            spreadTimer = -spreadTime;
        }
        if (spreadTimer >= spreadTime) {
            spreadTimer = 0f;
            pm.rotateSpeed = pm.normalRotateSpeed;
            if (curSpreadX >= minSpreadX) {
                curSpreadX -= minusSpread;
            }
            if (curSpreadY >= minSpreadY) {
                curSpreadY -= minusSpread;
            }
            curSpreadX = Mathf.Clamp(curSpreadX, minSpreadX, maxSpreadX);
            curSpreadY = Mathf.Clamp(curSpreadY, minSpreadY, maxSpreadY);
        }
        //Debug.Log("X : " + curSpreadX);
        //Debug.Log("Y : " + curSpreadY);
    }

    public void shootingActive(bool act) {
        //  사격 설정
        DisableShootingInput = !act;
        ps.pReloadORGrenadeAct = !act;
        pik.ikActive = act;
    }

    private void GetWeaponStats() {
        //  weaponStats 데이터로 변수 설정
        weaponName = weaponStats.name;
        weaponDesc = weaponStats.weaponDesc;
        weaponFireType = weaponStats.weaponFireType;
        weaponType = weaponStats.weaponType;

        weaponMuzzle = weaponStats.weaponMuzzle;
        weaponLowerMount = weaponStats.weaponLowerMount;
        weaponSight = weaponStats.weaponSight;
        weaponMagazine = weaponStats.weaponMagazine;

        timeBetweenBullets = weaponStats.timeBetweenBullets;
        projectileCount = weaponStats.projectileCount;
        damagePerShot = weaponStats.damagePerShot;
        bulletSpeed = weaponStats.bulletSpeed;
        bulletAcceleration = weaponStats.bulletAcceleration;
        bulletTimeToLive = weaponStats.bulletTimeToLive;
        bulletCount = weaponStats.bulletCount;

        minSpreadX = weaponStats.minSpreadX;
        minSpreadY = weaponStats.minSpreadY;
        maxSpreadX = weaponStats.maxSpreadX;
        maxSpreadY = weaponStats.maxSpreadY;
        deltaSpread = weaponStats.deltaSpread;
        minusSpread = weaponStats.minusSpread;
        spreadTime = weaponStats.spreadTime;

        gunLight = weaponStats.gunLight;
    }

    public void DisableEffects() {
        //  라인 렌더러와 라이트를 비활성화
        if (gunLine != null) {
            gunLine.enabled = false;
        }
        if (gunLight != null) {
            for (int i = 0; i < gunLight.Length; i++) {
                gunLight[i].enabled = false;
            }
        }
    }

    private void Shoot() {
        //  사격
        timer = 0f;
        gunAudio.Play();
        if (gunParticle != null) {
            gunParticle.Stop();
            gunParticle.Play();
        }
        for (int i = 0; i < gunLight.Length; i++) {
            gunLight[i].enabled = true;
        }
        //  투사체 기반
        if (weaponType == weaponType.Projectile) {
            spreadMinusOff = true;

            float randomNumberX;
            float randomNumberY;
            
            for (int i = 0; i < projectileCount; i++) {
                GameObject Bullet;

                randomNumberX = Random.Range(-curSpreadX, curSpreadX);
                randomNumberY = Random.Range(-curSpreadY, curSpreadY);

                if (usePooling) {
                    Bullet = GameBullets[ActualGameBullet];
                    Bullet.transform.position = gunMuzzle.transform.position;
                    Bullet.transform.rotation = gunMuzzle.transform.rotation;
                    Bullet.transform.Rotate(randomNumberX, randomNumberY, 0);
                    Bullet.GetComponent<Rigidbody>().isKinematic = false;
                    Bullet.GetComponent<Collider>().enabled = true;
                    Bullet.GetComponent<Projectile>().usePooling = true;
                    Bullet.GetComponent<Projectile>().setBullet(damagePerShot, bulletSpeed, bulletAcceleration, bulletTimeToLive);
                    Bullet.GetComponent<Projectile>().resetBullet();
                    Bullet.SetActive(true);
                    ActualGameBullet += 1;
                    if (ActualGameBullet >= GameBullets.Length)
                        ActualGameBullet = 0;
                } else {
                    Bullet = Instantiate(bulletPrefab, gunMuzzle.transform.position, gunMuzzle.transform.rotation);
                    Bullet.transform.Rotate(randomNumberX, randomNumberY, 0);
                    Bullet.GetComponent<Projectile>().usePooling = false;
                    Bullet.GetComponent<Projectile>().setBullet(damagePerShot, bulletSpeed, bulletAcceleration, bulletTimeToLive);
                    Bullet.SetActive(true);
                    Bullet.GetComponent<Rigidbody>().isKinematic = false;
                    Bullet.GetComponent<Collider>().enabled = true;

                }
            }
            if (curSpreadX <= maxSpreadX) {
                curSpreadX += deltaSpread;
            }
            if (curSpreadY <= maxSpreadY) {
                curSpreadY += deltaSpread;
            }
            curSpreadX = Mathf.Clamp(curSpreadX, minSpreadX, maxSpreadX);
            curSpreadY = Mathf.Clamp(curSpreadY, minSpreadY, maxSpreadY);
        }
        //  Collider 기반
        else if (weaponType == weaponType.Collider) {
            pm.disableMovement = true;
            meleeAttack = true;
        }
    }

    private void OnTriggerEnter(Collider other) {
        //  근접공격 확인
        if (meleeAttack) {
            PlayerState ps = other.GetComponent<PlayerState>();
            if (ps != null) {
                ps.takeHealthDamage(damagePerShot);
            }
            Debug.Log(other.name);
        }
    }
}