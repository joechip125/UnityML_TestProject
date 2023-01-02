using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 8.0f;
    private float _timeAlive;
    public Vector3 moveVector;
    public float moveSpeed = 4.0f;
    

    private void FixedUpdate()
    {
        _timeAlive += Time.deltaTime;

        transform.position += moveVector * (moveSpeed * Time.deltaTime);

        if (_timeAlive >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Unit"))
        {
            gameObject.SetActive(false);
        }
    }
}
