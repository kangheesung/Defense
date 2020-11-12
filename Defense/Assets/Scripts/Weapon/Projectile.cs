using UnityEngine;

public class Projectile : MonoBehaviour {
    public bool isExplode = false;
    public bool UsePhysicsToTranslate = false;
    public bool UsePhysicsCollisions = false;

    private int ProjectileDamage = 1;
    private float ProjectileSpeed = 1.0f;
    private float ProjectileAccel = 0.0f;

    private Vector3 fwd = Vector3.zero;

    public float cameraShakeFactor = 3.0f;
    public float cameraShakeDuration = 0.5f;

    [HideInInspector]
    public bool usePooling;

    private float timeToLive = 3.0f;

    private CameraShake cs;

    private void Awake() {
        cs = FindObjectOfType<CameraShake>();
    }

    private void Start() {
        if (UsePhysicsToTranslate && !isExplode) {
            GetComponent<Rigidbody>().AddForce(Vector3.forward * ProjectileSpeed * 10);
        }
    }

    private void Update() {
        if (!UsePhysicsToTranslate) {
            if (ProjectileAccel > 0.0f)
                ProjectileSpeed += ProjectileAccel * Time.deltaTime;

            transform.Translate(Vector3.forward * ProjectileSpeed);

            fwd = transform.TransformDirection(Vector3.forward);

        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, fwd, out hit, ProjectileSpeed * 2.0f)) {
            if (hit.collider.tag != "Bullets") {
                destroyBullet(hit.collider);
            }    
        }

        timeToLive -= Time.deltaTime;
        if (timeToLive <= 0.0f) {
            if (isExplode) {

            } else {
                if (usePooling)
                    this.gameObject.SetActive(false);
                else
                    Destroy(this.gameObject);
            }
        }
    }

    public void destroyBullet(Collider hit) {
        PlayerState ps = hit.GetComponent<PlayerState>();
        if (ps != null) {
            ps.takeHealthDamage(ProjectileDamage);
            Debug.Log("asg");
        }
        if (usePooling)
            this.gameObject.SetActive(false);
        else
            Destroy(this.gameObject);
    }

    public void resetBullet() {
        if (GetComponentInChildren<TrailRenderer>()) {
            GetComponentInChildren<TrailRenderer>().Clear();
        }
    }

    public void setBullet(int damage, float speed, float accel, float time) {
        ProjectileDamage = damage;
        ProjectileSpeed = speed;
        ProjectileAccel = accel;
        timeToLive = time;
    }

    private void OnCollisionEnter(Collision collision) {
        if (UsePhysicsCollisions) {
            destroyBullet(collision.collider);
        }
    }
}