using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using System.Net;
using System;

public class NewParaChute : MonoBehaviour
{
   
    public float rotateSpeed = 50f;
    public float moveSpeed = 5f;

    public float speed = 12.5f;
    public float drag = 6;
    public float freefallDrag = 1;

    public Rigidbody rb;

    private Vector3 rot;

    public bool paraPressed = false;
    public bool isPlaneGrounded = true;
    public bool isGrounded = false;
    public bool hasJumped = false;

    
    public GameObject parachute;

    //[Range(0, 180)] public int soldierRotx;
    [Range(0, 180)] public int soldierRotz0;
    [Range(0, 180)] public int soldierRotz1;

  

    public EncoderRead _encoder;
    //public EncoderRead _encoder2;


    private void Start()
    {
        UduinoManager.Instance.pinMode(0, PinMode.Servo);
        UduinoManager.Instance.pinMode(1, PinMode.Servo);

        UduinoManager.Instance.pinMode(13, PinMode.Output);
        //UduinoManager.Instance.pinMode(10, PinMode.Output);
        




        soldierRotz0 = 90;
        soldierRotz1 = 90;

       


        rb = GetComponent<Rigidbody>();
        rot = transform.eulerAngles;

        UduinoManager.Instance.digitalWrite(13, State.HIGH);
    }

  
    private void Update()
    {
        

        if (Input.GetKeyDown(KeyCode.Space))
            hasJumped = true;
     

        if (isPlaneGrounded && !hasJumped)
        {

           
          //  hasJumped = true;
            
            //isGrounded = false;

            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

            rb.AddForce(movement * speed);
            

        }
        

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f, 25 << LayerMask.NameToLayer("Ground")))
        {
            Debug.Log("Ground is being dedected");
          //  Grounded();
            isGrounded = true;


        }

        if (isGrounded == true)
            Grounded();

        if (hasJumped)
        {

            UduinoManager.Instance.digitalWrite(13, State.LOW);

            // Apply freefall drag
            rb.drag = freefallDrag;

            // Check if player has deployed the parachute
            if (Input.GetKeyDown(KeyCode.P))
            {
                isPlaneGrounded = false;
                parachute.SetActive(true);
               // UduinoManager.Instance.digitalWrite(13, State.HIGH);
                paraPressed = true;
            }

            // If the parachute is deployed, slow down the fall
            if (paraPressed)
            {
                
                if (_encoder.DataTransfered0 > 0 && _encoder.DataTransfered1 > 0)
                {
                    rot.x += 20 * Time.deltaTime;
                    rot.x = Mathf.Clamp(rot.x, 0, 45);
                }
                else if (_encoder.DataTransfered0 == 0 && _encoder.DataTransfered1 == 0)
                {
                    rot.x -= 20 * Time.deltaTime;
                    rot.x = Mathf.Clamp(rot.x, 0, 45);
                }

                if (_encoder.DataTransfered0 > 0)
                {
                    rot.z = -_encoder.DataTransfered0;

                    rot.z = Mathf.Clamp(rot.z, -40, 40);
                    soldierRotz0 = 90 + (int)rot.z;
                }
                else if (_encoder.DataTransfered1 > 0)
                {
                    rot.z = _encoder.DataTransfered1;
                    rot.z = Mathf.Clamp(rot.z, -40, 40);
                    soldierRotz1 = 90 + (int)rot.z;
                }

                //rot.z = Mathf.Clamp(rot.z, -20, 20);

                transform.rotation = Quaternion.Euler(rot);

                // Calculate the percentage of parachute deployment
                float percentage = rot.x / 45;

                // Calculate the modified drag and speed based on the percentage
                float mod_drag = (percentage * -2) + drag;
                float mod_speed = percentage * (speed - 10f) + 10f;

                // Apply the modified drag and speed
                rb.drag = mod_drag;
                Vector3 localV = transform.InverseTransformDirection(rb.velocity);
                localV.z = mod_speed;
                rb.velocity = transform.TransformDirection(localV);

                                
              
                //UduinoManager.Instance.analogWrite(0, soldierRotz0);
                //UduinoManager.Instance.analogWrite(1, soldierRotz1);

                
            }
            else
            {
                // Apply regular drag and speed during freefall
                rb.drag = freefallDrag;
                Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
                float rotationY = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
                transform.Rotate(new Vector3(0f, rotationY, 0f));

               

            }
        }

         
        //else
        //{
           
        //    // Player is grounded
        //    if (isGrounded)
        //    {
        //        isGrounded = true;
                
               
        //        rb.velocity = Vector3.zero;
        //        rb.drag = drag;
        //    }
        //}
    }

    public void Grounded()
    {
        UduinoManager.Instance.digitalWrite(13, State.HIGH);

        hasJumped = false;

        paraPressed = false;
    }
}
 