using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _turnSpeed = 360;
    public bool isMainPlayer = true;

    [HideInInspector] public Vector3 _input;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _input = Vector3.zero;
    }

    private void Update()
    {
        GatherInput();
        Look();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GatherInput()
    {
        if (isMainPlayer)
        {
            _input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (UDP.instance) UDP.instance.SendString(new MovementData(_input, name));
        }
    }

    private void Look()
    {
        if (_input == Vector3.zero) return;

        var rot = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _turnSpeed * Time.deltaTime);
    }

    private void Move()
    {
        float inputSpeed;
        if (_input.magnitude >= 1)
            inputSpeed = 1;
        else
            inputSpeed = _input.magnitude;

        _rb.MovePosition(transform.position + transform.forward * inputSpeed * _speed * Time.deltaTime);
        animator.SetFloat("Speed", inputSpeed);
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
