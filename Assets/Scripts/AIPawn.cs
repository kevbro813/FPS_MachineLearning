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
    public void ForwardBackMovement(float inputVertical)
    {
        tf.Translate((tf.forward * inputVertical * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void LateralMovement(float inputHorizontal)
    {
        tf.Translate((tf.right * inputHorizontal * moveSpeed * Time.deltaTime), Space.Self);
    }
    public void Rotation(float inputRotation)
    {
        tf.Rotate((Vector3.up * inputRotation * rotationSpeed * Time.deltaTime), Space.Self);
    }
}
