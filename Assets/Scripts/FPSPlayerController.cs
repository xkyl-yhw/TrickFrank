using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class FPSPlayerController : MonoBehaviour {

    private CharacterController m_CharacterController;
    private Rigidbody m_rigidbody;
    [SerializeField]
    private Camera m_Camera;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private float m_StepInterval;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;

    class mouseMoving
    {
        public float XLimit = 80f;
        public float xSentitivity = 2f;
        public float ySentitivity = 2f;
        public float smoothTime=2f;
    }
    private Quaternion m_CharacterRotate;
    private Quaternion m_CameraRotate;
    private float CameraRotate;
    [SerializeField]
    private bool smooth;
    private Vector2 m_Input;
    private bool m_iswalking=false;
    private Vector3 m_moveDir;
    private Vector3 m_OriginalCameraPosition;
    private CollisionFlags m_CollisionFlags;

    public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                    new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                    new Keyframe(2f, 0f)); // sin curve for head bob
    public float HorizontalBobRange = 0.33f;
    public float VerticalBobRange = 0.33f;
    public float VerticaltoHorizontalRatio = 1f;

    private float m_CyclePositionX;
    private float m_CyclePositionY;
    private float m_BobBaseInterval;
    private float m_Time;


    void Start () {
        m_CharacterController = GetComponent<CharacterController>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_CharacterRotate = this.transform.localRotation;
        m_CameraRotate = m_Camera.transform.localRotation;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_Time = Bobcurve[Bobcurve.length - 1].time;
        m_BobBaseInterval = m_StepInterval;
    }
	
	void Update () {
        RotateView();
	}

    private void LateUpdate()
    {
        float speed;
        GetInput(out speed);
        Vector3 desiredDir = transform.forward * m_Input.y + transform.right * m_Input.x;
        RaycastHit hit;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hit,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredDir = Vector3.ProjectOnPlane(desiredDir, hit.normal).normalized;

        m_moveDir.x = desiredDir.x*speed;
        m_moveDir.z = desiredDir.z*speed;

        if (!m_CharacterController.isGrounded)
        {
            m_moveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_moveDir * Time.fixedDeltaTime);

        UpdateCameraPosition(speed);
    }

    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition = DoHeadBob(m_CharacterController.velocity.magnitude +
                                  (speed * (m_iswalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y;
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y;
        }
        m_Camera.transform.localPosition = newCameraPosition;

    }

    public Vector3 DoHeadBob(float speed)
    {
        float xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRange);
        float yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRange);

        m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
        m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticaltoHorizontalRatio;

        if (m_CyclePositionX > m_Time)
        {
            m_CyclePositionX = m_CyclePositionX - m_Time;
        }
        if (m_CyclePositionY > m_Time)
        {
            m_CyclePositionY = m_CyclePositionY - m_Time;
        }

        return new Vector3(xPos, yPos, 0f);
    }

    private void GetInput(out float speed)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool wasWalking = m_iswalking;
        speed = wasWalking ? walkSpeed : runSpeed;

        m_iswalking = !Input.GetKey(KeyCode.LeftShift);
        m_Input = new Vector2(horizontal, vertical);

        if (m_Input.sqrMagnitude > 1f)
        {
            m_Input.Normalize();
        }
    }

    private void RotateView()
    {
        mouseMoving mousemoving = new mouseMoving();
        float yRot = Input.GetAxis("Mouse X") * mousemoving.xSentitivity;
        float xRot = Input.GetAxis("Mouse Y") * mousemoving.ySentitivity;

        m_CharacterRotate *= Quaternion.Euler(0, yRot, 0);
        CameraRotate += xRot;
        CameraRotate = Mathf.Clamp(CameraRotate, -mousemoving.XLimit, mousemoving.XLimit);
        m_CameraRotate = Quaternion.Euler(-CameraRotate, 0, 0);

        if (smooth)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_CharacterRotate, mousemoving.smoothTime * Time.deltaTime);
            m_Camera.transform.localRotation = Quaternion.Slerp(m_Camera.transform.localRotation, m_CameraRotate, mousemoving.smoothTime * Time.deltaTime);
        }
        else
        {
            transform.localRotation = m_CharacterRotate;
            m_Camera.transform.localRotation = m_CameraRotate;
        }
    }
}
