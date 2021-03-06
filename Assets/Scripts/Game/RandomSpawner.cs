﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public float locationMin_X;
    public float locationMax_X;
    public float locationMin_Z;
    public float locationMax_Z;
    [HideInInspector] public Transform tf;
    public Vector3 spawnLocation;
    public bool isTurret;
    private void Awake()
    {
        tf = GetComponent<Transform>();
        spawnLocation = tf.position;
    }
    public void RandomLocation()
    {
        float randomX = Random.Range(locationMin_X, locationMax_X);
        float randomZ = Random.Range(locationMin_Z, locationMax_Z);

        if (isTurret) spawnLocation = new Vector3(randomX, 1.8f, randomZ);

        else spawnLocation = new Vector3(randomX, 1f, randomZ);

        tf.position = spawnLocation;
    }
}
