using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    //float angle = transform.eulerAngles.z % 360f;
    //if (angle < 0f)
    //    angle += 360f;

    //float rad = Mathf.Atan2(vectorToTarget.x, vectorToTarget.z);
    //rad *= Mathf.Rad2Deg;

    //rad = rad % 360;
    //if (rad < 0)
    //{
    //    rad = 360 + rad;
    //}

    //rad = 90f - rad;
    //if (rad < 0f)
    //{
    //    rad += 360f;
    //}
    //rad = 360 - rad;
    //rad -= angle;
    //if (rad < 0)
    //    rad = 360 + rad;
    //if (rad >= 180f)
    //{
    //    rad = 360 - rad;
    //    rad *= -1f;
    //}
    //rad *= Mathf.Deg2Rad;

    //inputs[1] = rad / (Mathf.PI);

    //rad = Mathf.Atan2(vectorToTarget.z, vectorToTarget.x);
    //rad *= Mathf.Rad2Deg;

    //rad = rad % 360;
    //if (rad < 0)
    //{
    //    rad = 360 + rad;
    //}

    //rad = 90f - rad;
    //if (rad < 0f)
    //{
    //    rad += 360f;
    //}
    //rad = 360 - rad;
    //rad -= angle;
    //if (rad < 0)
    //    rad = 360 + rad;
    //if (rad >= 180f)
    //{
    //    rad = 360 - rad;
    //    rad *= -1f;
    //}
    //rad *= Mathf.Deg2Rad;

    //inputs[2] = rad / (Mathf.PI);

    //inputs[0] = distance;
    //if (initialized == true)
    //{
    //    float[] inputs = new float[8];

    //    for (int input = 0; input < inputs.Length; input++)
    //    {
    //        inputs[input] = distancesToObjects[input];
    //    }
    //}

    //inputs[0] = -Vector3.Distance(tf.position, GameManager.instance.objective.GetComponent<Transform>().position);

    //Vector3 vectorToTarget = ttf.position - tf.position;

    //// Find the distance between the two vectors in float to compare with maxViewDistance
    //targetDistance = Vector3.Distance(ttf.position, tf.position);

    //        // Find the angle between the direction our agent is facing (forward in local space) and the vector to the target.
    //        float angleToTarget = Vector3.Angle(vectorToTarget, tf.forward);

    //        if (angleToTarget<fieldOfView && targetDistance<maxViewDistance)
    //        {
    //            int environmentLayer = LayerMask.NameToLayer("Obstacle");          // Add Walls layer to variable
    //int playerLayer = LayerMask.NameToLayer("Objective");              // Add Player layer to variable
    //int layerMask = (1 << playerLayer) | (1 << environmentLayer);      // Create layermask

    //RaycastHit hit;

    //            // Raycast to detect objective within field of view with max view distance as a limit
    //            if (Physics.Raycast(tf.position, vectorToTarget, out hit, maxViewDistance, layerMask))
    //            {

    //                if (hit.collider.CompareTag("Objective"))
    //                {
    //                    net.AddFitness(10f);
    //                    Debug.DrawRay(tf.position, vectorToTarget, Color.red, maxViewDistance);
    //                }
    //            }
    //        }

    //        float angle = transform.eulerAngles.z % 360f;
    //        if (angle< 0f)
    //            angle += 360f;

    //        float rad = angleToTarget % 360;
    //        if (rad< 0)
    //        {
    //            rad = 360 + rad;
    //        }

    //        rad = 90f - rad;
    //        if (rad< 0f)
    //        {
    //            rad += 360f;
    //        }
    //        rad = 360 - rad;
    //        rad -= angle;
    //        if (rad< 0)
    //            rad = 360 + rad;
    //        if (rad >= 180f)
    //        {
    //            rad = 360 - rad;
    //            rad *= -1f;
    //        }
    //        rad *= Mathf.Deg2Rad;

    //        inputs[0] = rad / 360;
    //        Debug.Log(inputs[0]);
    // Raycast to detect if a wall is directly in front of the AI
    //if (Physics.Raycast(tf.position, tf.forward, out hit, colDetectRange, layerMask))
    //{
    //    if (hit.collider.CompareTag("Obstacle"))
    //    {
    //        Debug.DrawRay(tf.position, vectorToTarget, Color.red, maxViewDistance);
    //    }
    //}

    //hit = new RaycastHit[8];
    //    int hitL = hit.Length;

    //// Directions Start forward then moves clockwise
    //directions = new Vector3[] { new Vector3(0, 0, distance), new Vector3(distance, 0, distance),
    //        new Vector3(distance, 0, 0), new Vector3(distance, 0, -distance),  new Vector3(0, 0, -distance),
    //        new Vector3(-distance, 0, -distance), new Vector3(-distance, 0, 0), new Vector3(-distance, 0, distance) };

    //    while (hitL > 0)
    //    {
    //        hitL--;
    //        if (Physics.Raycast(tf.position, directions[hitL], out hit[hitL], distance))
    //        {
    //Debug.DrawRay(tf.position, directions[i], Color.blue, distance);

    //    distancesToObjects[hitL] = hit[hitL].distance;

    //    if (hit[hitL].collider.CompareTag("Objective"))
    //    {
    //        float proxBonus = distance - hit[hitL].distance;
    //        float fitnessBonus = (float)Math.Tanh(proxBonus);
    //        net.AddFitness(fitnessBonus);
    //    }
    //    if (hit[hitL].collider.CompareTag("Obstacle"))
    //    {
    //        float proxBonus = hit[hitL].distance - distance;
    //        float fitnessBonus = (float)Math.Tanh(proxBonus);
    //        net.AddFitness(-fitnessBonus);
    //    }
    //    }
    //}



    //Raycast to detect if a wall is directly in front of the AI
    //if (Physics.Raycast(tf.position, tf.forward, out hit, collisionDetectRange, layerMask))
    //{
    //    if (hit.collider.CompareTag("Obstacle"))
    //    {
    //        Debug.Log("Collision Detection");
    //        net.AddFitness(GameManager.instance.obstacleProximityPenalty);
    //        Debug.DrawRay(tf.position, vectorToTarget, Color.blue, maxViewDistance);
    //    }
    //}
}
