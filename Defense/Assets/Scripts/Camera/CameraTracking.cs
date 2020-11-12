using UnityEngine;

public class CameraTracking : MonoBehaviour {
    public float smoothSpeed = 5f;
    public Transform target;
    public Vector3 offset;

    public void FixedUpdate() {
        Vector3 TargetPos = target.position + offset;
        Vector3 smoothposition = Vector3.Lerp(transform.position, TargetPos, smoothSpeed * Time.deltaTime);
        transform.position = smoothposition;
    }
}