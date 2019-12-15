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
        tf.Translate((-tf.forward * input * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void MoveRight(float input)
    {
        tf.Translate((tf.right * input * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void MoveLeft(float input)
    {
        tf.Translate((-tf.right * input * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void Rotation(float inputRotation)
    {
        tf.Rotate((Vector3.up * inputRotation * rotationSpeed * Time.deltaTime), Space.Self);
    }
}
