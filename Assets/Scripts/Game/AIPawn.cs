using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPawn : MonoBehaviour
{
    private Transform tf;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    private void Awake()
    {
        tf = GetComponent<Transform>(); 
    }
    // BOOLEAN VERSION OF ACTIONS USED WITH REINFORCEMENT ALGORITHM
    public void MoveForward(bool input)
    {
        if (input)
        {
            tf.Translate((tf.forward * moveSpeed * Time.deltaTime), Space.Self);
        }      
    }
    public void MoveBack(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.forward * moveSpeed * Time.deltaTime), Space.Self);
        }     
    }
    public void MoveRight(bool input)
    {
        if (input)
        {
            tf.Translate((tf.right * moveSpeed * Time.deltaTime), Space.Self);
        }       
    }
    public void MoveLeft(bool input)
    {
        if (input)
        {
            tf.Translate((-tf.right * moveSpeed * Time.deltaTime), Space.Self);
        }       
    }
    public void RotateRight(bool input)
    {
        if (input)
        {
            tf.Rotate((Vector3.up * rotationSpeed * Time.deltaTime), Space.Self);
        }        
    }
    public void RotateLeft(bool input)
    {
        if (input)
        {
            tf.Rotate((-Vector3.up * rotationSpeed * Time.deltaTime), Space.Self);
        } 
    }
    public void NoMovement(bool input)
    {
        // Do nothing.
    }
}
