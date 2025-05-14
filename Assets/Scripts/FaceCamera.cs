using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform cameraTransform;
    [SerializeField] private bool lockToCorner = true;
    [SerializeField] private Vector3 cornerOffset = new Vector3(0.1f, 0.1f, 0.5f);

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (lockToCorner)
        {
            transform.position = cameraTransform.position +
                                cameraTransform.forward * cornerOffset.z +
                                cameraTransform.right * -cornerOffset.x +
                                cameraTransform.up * -cornerOffset.y;
        }
        transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
                        cameraTransform.rotation * Vector3.up);
    }
}