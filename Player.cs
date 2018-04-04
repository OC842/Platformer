using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float jumpHeight = 10;
    public float gravityMultiplier;
    public float airSpeedReduction;
    public float groundSpeedReduction;
    public float playerAcceleration;

    public float moveSpeed = 10;
    float gravity = -9.81f;
    Vector3 velocity;
    Vector3 cameraZoomOut = new Vector3(0, 0, -10);

    Controller2D controller;
    Transform pos;

    void Start()
    {
        controller = GetComponent<Controller2D>();
        pos = GetComponent<Transform>();
        MoveCameraWithPlayer();
        airSpeedReduction = 0.05f;
        groundSpeedReduction = 0.4f;
        playerAcceleration = 0.5f;
    }

    void Update()
    {
        if(controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if (controller.collisions.left || controller.collisions.right)
        {
            velocity.x = 0;
        }

        StartCoroutine("Jump");
        StartCoroutine("Move");

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        MoveCameraWithPlayer();
        
    }

    private void FixedUpdate()
    {
        gravity = -9.8f * gravityMultiplier;
    }

    IEnumerator Move()
    {

        float input = Input.GetAxis("Horizontal");

        if (input != 0 && Mathf.Sign(input) == Mathf.Sign(velocity.x))
        {
            velocity.x += input * playerAcceleration;

            if(velocity.x > moveSpeed)
            {
                velocity.x = moveSpeed;
            }else if(velocity.x < -moveSpeed)
            {
                velocity.x = -moveSpeed;
            }

            /*if(controller.collisions.leftSlope == true)
            {
                gravityMultiplier = 0;
            
                velocity = Mathf.Abs(velocity.x) * controller.collisions.slopeUnitVector;
            }
            if (controller.collisions.rightSlope == true)
            {
                gravityMultiplier = 0;

                velocity = Mathf.Abs(velocity.x) * controller.collisions.slopeUnitVector;
            }*/
        }
        else if (input != 0 && Mathf.Sign(input) != Mathf.Sign(velocity.x))
        {
            velocity.x += input * playerAcceleration;
        }
        else if (input == 0 && velocity.x != 0 && controller.collisions.below == true)
        {
            velocity.x -= groundSpeedReduction * velocity.x;

        }
        else if(input == 0 && velocity.x != 0 && controller.collisions.below == false)
        {
            velocity.x -= velocity.x * airSpeedReduction;

        }


        yield return null;
    }

    IEnumerator Jump()
    {
        if (velocity.y < 0 && controller.collisions.below == false)
        {
            gravityMultiplier = 2f;
        }
        else if (Input.GetAxis("Jump") == 0 && controller.collisions.below == false)
        {
            gravityMultiplier = 2.5f;
        }
        else if (Input.GetAxis("Jump") != 0 && controller.collisions.below == true)
        {
            velocity.y = jumpHeight;
            gravityMultiplier = 1;
        }
        else
        {
            gravityMultiplier = 1;
        }
        yield return null;
    }

    void MoveCameraWithPlayer()
    {
        Camera.main.transform.SetPositionAndRotation(pos.position + cameraZoomOut, Quaternion.identity);

        
    }


}

