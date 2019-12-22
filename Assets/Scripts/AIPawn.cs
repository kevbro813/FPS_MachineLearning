using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPawn : MonoBehaviour
{
    private Transform tf;
    public float moveSpeed = 10f;
    public float rotationSpeed = 10f;
    private void Start()
    {
        tf = GetComponent<Transform>(); 
    }
    public void MoveForward(float input)
    {
        tf.Translate((tf.forward * input * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void MoveBack(float input)
    {
        tf.Translate((tf.forward * -input * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void MoveRight(float input)
    {
        tf.Translate((tf.right * input * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void MoveLeft(float input)
    {
        tf.Translate((tf.right * -input * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void RotateRight(float inputRotation)
    {
        tf.Rotate((Vector3.up * inputRotation * rotationSpeed * Time.deltaTime), Space.Self);
    }
    public void RotateLeft(float inputRotation)
    {
        tf.Rotate((Vector3.up * -inputRotation * rotationSpeed * Time.deltaTime), Space.Self);
    }

    // BOOLEAN VERSION OF ACTIONS USED WITH REINFORCEMENT ALGORITHM
    public void B_MoveForward(bool input)
    {
        if (input)
        {
            tf.Translate((tf.forward * moveSpeed * Time.deltaTime), Space.Self);
        }      
    }
    public void B_MoveBack(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.forward * moveSpeed * Time.deltaTime), Space.Self);
        }     
    }
    public void B_NoVertical(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.forward * 0), Space.Self);
        }
    }
    public void B_MoveRight(bool input)
    {
        if (input)
        {
            tf.Translate((tf.right * moveSpeed * Time.deltaTime), Space.Self);
        }       
    }
    public void B_MoveLeft(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.right * moveSpeed * Time.deltaTime), Space.Self);
        }       
    }
    public void B_NoLateral(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.right * 0), Space.Self);
        }
    }
    public void B_RotateRight(bool input)
    {
        if (input)
        {
            tf.Rotate((Vector3.up * rotationSpeed * Time.deltaTime), Space.Self);
        }        
    }
    public void B_RotateLeft(bool input)
    {
        if (input)
        {
            tf.Rotate((-Vector3.up * rotationSpeed * Time.deltaTime), Space.Self);
        } 
    }
    public void B_NoRotation(bool input)
    {
        if (input)
        {
            tf.Rotate((-Vector3.up * 0), Space.Self);
        }
    }
}
