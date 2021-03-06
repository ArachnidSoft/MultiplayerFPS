﻿using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(ConfigurableJoint))]
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float lookSensitivity = 3f;

    [SerializeField]
    private float thrusterForce = 1000f;

    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }

    [SerializeField]
    private LayerMask environmentMask;

    [Header("Spring settings")]
    /*[SerializeField]
    private JointDriveMode jointMode = JointDriveMode.Position;*/
    [SerializeField]
    private float jointSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;


    private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;

    // Use this for initialization
    void Start ()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();
        SetJointSettings(jointSpring);
	}
	
	// Update is called once per frame
	void Update ()
    {
        //setting target position for spring. This corrects physics for applying gravity when flying
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f, environmentMask))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        }
        else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }
        //calculate movement velocity as a 3d vector
        float xMov = Input.GetAxis("Horizontal");
        float zMov = Input.GetAxis("Vertical");

        Vector3 movHorizontal = transform.right * xMov;
        Vector3 movVertical = transform.forward * zMov;

        //final movement vector
        Vector3 velocity = (movHorizontal + movVertical) * speed;

        //Animate movement
        animator.SetFloat("ForwardVelocity", zMov);

        motor.Move(velocity);

        //calculate rotation as a 3d vector
        float yRot = Input.GetAxisRaw("Mouse X");

        Vector3 rotation = new Vector3(0f, yRot, 0f) * lookSensitivity;

        //Apply rotation
        motor.Rotate(rotation);

        //calculate camera rotation as a 3d vector
        float xRot = Input.GetAxisRaw("Mouse Y");

        float _cameraRotation = xRot * lookSensitivity;

        //Apply rotation
        motor.RotateCamera(_cameraRotation);




        //Calculate thrusterforce based on player input
        Vector3 _thrusterForce = Vector3.zero;
        //Apply thruster force
        if (Input.GetButton("Jump") && thrusterFuelAmount > 0f)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if(thrusterFuelAmount >= 0.01f)
            {
                _thrusterForce = Vector3.up * thrusterForce;
                SetJointSettings(0f);
            }
            
        }
        else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;

            SetJointSettings(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);

        motor.ApplyThruster(_thrusterForce);
    }

    private void SetJointSettings(float _jointSpring)
    {
        joint.yDrive = new JointDrive
        {
            //mode = jointMode,
            positionSpring = jointSpring,
            maximumForce = jointMaxForce
        };
    }

}
