using UnityEngine;

public class CameraShake : MonoBehaviour {
    [Header("Shooting Shake Settings")]
    public bool isShaking = false;
    public float shakeFactor = 3f;
    public float shakeSmoothness = 5f;
    public float actualShakeTimer = 0.2f;

    [Header("Explosion Shake Settings")]
    public bool isExpShaking = false;
    public float shakeExpFactor = 5f;
    public float shakeExpSmoothness = 3f;
    public float actualExpShakeTimer = 1.0f;
    
    void LateUpdate() {
        if (isShaking) {
            if (actualShakeTimer >= 0.0f) {
                actualShakeTimer -= Time.deltaTime;
                Vector3 newPos = transform.localPosition + CalculateRandomShake(shakeFactor, false);
                transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, shakeSmoothness * Time.deltaTime);
            } else {
                isShaking = false;
                actualShakeTimer = 0.2f;
            }
        }
        if (isExpShaking) {
            if (actualExpShakeTimer >= 0.0f) {
                actualExpShakeTimer -= Time.deltaTime;
                Vector3 newPos = transform.localPosition + CalculateRandomShake(shakeExpFactor, true);
                transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, shakeExpSmoothness * Time.deltaTime);
            } else {
                isExpShaking = false;
                actualExpShakeTimer = 1.0f;
            }
        }
    }

    private Vector3 CalculateRandomShake(float shakeFac, bool isExplosion) {
        Vector3 randomShakePos = new Vector3(Random.Range(-shakeFac, shakeFac), Random.Range(-shakeFac, shakeFac), Random.Range(-shakeFac, shakeFac));
        if (isExplosion)
            return randomShakePos * (actualExpShakeTimer / 0.2f);
        else
            return randomShakePos * (actualShakeTimer / 1.0f); 
    }

    public void Shake(float factor, float duration) {
        isShaking = true;
        shakeFactor = factor;
        actualShakeTimer = duration;
    }

    public void ExplosionShake(float factor, float duration) {
        isExpShaking = true;
        shakeExpFactor = factor;
        actualExpShakeTimer = duration;
    }
}