using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjective : MonoBehaviour
{
    public float locationMin_X;
    public float locationMax_X;
    public float locationMin_Z;
    public float locationMax_Z;
    public Transform tf;
    public Vector3 objectiveLocation;
    private void Start()
    {
        objectiveLocation = tf.position;
    }
    public void RandomLocation()
    {
        float randomX = Random.Range(locationMin_X, locationMax_X);
        float randomZ = Random.Range(locationMin_Z, locationMax_Z);
        objectiveLocation = new Vector3(randomX, 1.2f, randomZ);
        tf.position = objectiveLocation;
    }
}
