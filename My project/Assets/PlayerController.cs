using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : MonoBehaviour, IDamagable
{

    public float speed;
    public float jumpForce;
    public GameObject snowBallPrefab;
    public float initialHealth;
    public float maxHealth;
    public float maxSize;
    public GameObject snowBulletPrefab;
    public float shootForce;
    
    private Vector2 _input;
    private Rigidbody _rb;
    private bool isGrounded;
    private Vector3 _playerFordward;
    private GameObject _snowBall;
    private Camera _mainCamera;
    public float[] sizes = { 0.5f, 1f, 1.5f, 2f, 2.5f };
    public int[] speeds = { 6, 5 ,4, 3, 2 };
    public int[] jumpForces = { 12, 9, 7, 6, 5 };

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody>();
        AssignValues(initialHealth);
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();

        if (!_snowBall && Input.GetKeyDown(KeyCode.Space) && isGrounded )
        {
            Jump();
        }
        if (isGrounded && Input.GetKeyDown(KeyCode.E))
        {
            SpawnDespawnSnowBall();
        }
        if (_snowBall && Input.GetKeyDown(KeyCode.R))
        {
            ConsumeSnowBall();
        }
        if (!_snowBall && Input.GetMouseButtonDown(0))
        {
           Shoot();
        }
        
       
    }

    void AssignValues(float value)
    {
        // Calcula el tama�o de cada rango
        float rangeSize = (float)maxHealth / sizes.Length;

        // Determina en qu� rango cae el valor dado
        int index = Mathf.FloorToInt(value / rangeSize);
        index = Mathf.Clamp(index, 0, sizes.Length - 1); // Asegura que el �ndice est� en el rango de valores disponibles
        transform.localScale = new Vector3(sizes[index], sizes[index], sizes[index]);

        speed = speeds[index];
        jumpForce = jumpForces[index];
    }

    void MovePlayer()
    {
        float x = -Input.GetAxis("Horizontal");
        isGrounded = isGroundPlayer();
        if (!isGrounded )
        {
            x *= 0.8f;
        }
        _rb.velocity = new Vector2(x * speed, _rb.velocity.y);
        //_rb.AddForce(-Vector3.up*2f, ForceMode.Acceleration);
        
        if (x != 0)
        {
            transform.LookAt(transform.position + Vector3.right * (x > 0 ? 1 : -1));
        }
    }

    void Jump()
    {
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void ConsumeSnowBall()
    {
        var inc = _snowBall.GetComponent<snowBall>().GetStadistics();
        Debug.Log(inc);
        var incHealth = initialHealth + inc*3;
        initialHealth = Math.Min(maxHealth, incHealth);

        AssignValues(initialHealth);
        //var incSize = Math.Min(inc / 3,maxSize);
        
        Destroy(_snowBall);
    }

    void SpawnDespawnSnowBall()
    {
        if (!_snowBall)
        {
            var transform1 = transform;
            var position = new Vector3(
                transform1.position.x + transform1.forward.x * transform1.lossyScale.x *1.01f,
                transform1.position.y - transform1.lossyScale.y / 2f + snowBallPrefab.transform.lossyScale.y / 2f,
                transform1.position.z);
            _snowBall = Instantiate(snowBallPrefab, position, Quaternion.identity);
            // transform.position = new Vector3(transform.position.x + _parentTransform.forward.x* 0.00005f, heightLand + transform.lossyScale.y/2f, transform.position.z);
            _snowBall.transform.SetParent(transform);
        }
        else
            Destroy(_snowBall);
    }
    void Shoot()
    {
        if (transform.lossyScale.x < 0.1f || initialHealth == 1) return;
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var direcction = hit.point - transform.position;
            direcction = new Vector3(direcction.x, direcction.y, 0);
            direcction.Normalize();
            var bullet = Instantiate(snowBulletPrefab, transform.position + Vector3.up * transform.lossyScale.y / 2f, Quaternion.identity);
            var rbBullet = bullet.GetComponent<Rigidbody>();
            rbBullet.AddForce(direcction * shootForce, ForceMode.Impulse);
        }
        initialHealth -= 1f;
        //if(transform.lossyScale.x > 0.1f)
        //var a = transform.localScale.x - transform.localScale.x / initialHealth;
        //if(a>0.1f)
        //    transform.localScale -= transform.localScale/initialHealth;
        AssignValues(initialHealth);
        //var incSize = Math.Min(inc / 3,maxSize);
    }

    public void Damage(float damage)
    {
        initialHealth -= damage;
        if (initialHealth < 0)
            Destroy(gameObject);
    }

    private bool isGroundPlayer()
    {
        Debug.Log(transform.lossyScale);
        Debug.DrawRay(transform.position, -transform.up*2.1f, Color.red, 1f);
        return Physics.BoxCast(transform.position + Vector3.up *0.1f,transform.lossyScale/2f, Vector3.down, Quaternion.identity,transform.lossyScale.y/2f + 0.2f, LayerMask.GetMask("suelo"));
        //return Physics.Raycast(transform.position + Vector3.right * transform.lossyScale.x / 2f, Vector3.down, transform.lossyScale.y / 2f + 0.1f, LayerMask.GetMask("suelo"))
        //    || Physics.Raycast(transform.position - Vector3.right * transform.lossyScale.x / 2f, Vector3.down, transform.lossyScale.y / 2f + 0.1f, LayerMask.GetMask("suelo"));
    }
}
