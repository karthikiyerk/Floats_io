using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puncture : MonoBehaviour
{
    [SerializeField] private float forceMagnitude;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem airLeakFX;

    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * forceMagnitude * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    private void OnEnable()
    {
        airLeakFX.Play();
    }

    private void OnDisable()
    {
        airLeakFX.Pause();
    }
}
