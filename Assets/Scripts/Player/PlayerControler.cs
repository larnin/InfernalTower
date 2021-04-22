﻿using UnityEngine;
using System.Collections;

public class PlayerControler : MonoBehaviour
{
    //speed
    [SerializeField] float m_maxSpeed = 5;
    [SerializeField] float m_groundAcceleration = 20;
    [SerializeField] float m_airAcceleration = 10;
    [SerializeField] float m_maxFallSpeed = 10;
    //ground
    [SerializeField] float m_groundCheckDistance = 0.2f;
    [SerializeField] LayerMask m_groundMask;
    //jump
    [SerializeField] float m_jumpSpeed = 10;
    [SerializeField] float m_maxJumpDuration = 0.2f;
    [SerializeField] float m_jumpBufferTimeBeforeLand = 0.2f;
    [SerializeField] float m_jumpBufferTimerAfterLand = 0.2f;
    [SerializeField] float m_jumpApexSpeed = 1;

    Rigidbody2D m_rigidbody = null;
    BoxCollider2D m_collider = null;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector2 m_direction;
    bool m_jumping;

    bool m_grounded = false;
    float m_outGroundTIme = -1;
    Transform m_parent = null;

    float m_jumpPressedTime = -1;
    float m_jumpTime = -1;

    private void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();

        m_collider = GetComponent<BoxCollider2D>();
        if (m_collider == null)
            m_collider = GetComponentInChildren<BoxCollider2D>();

        m_subscriberList.Add(new Event<StartJumpEvent>.LocalSubscriber(OnJump, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void FixedUpdate()
    {
        GetDirectionEvent direction = new GetDirectionEvent();
        Event<GetDirectionEvent>.Broadcast(direction, gameObject);
        m_direction = direction.direction;

        GetJumpEvent jump = new GetJumpEvent();
        Event<GetJumpEvent>.Broadcast(jump, gameObject);
        m_jumping = jump.jump;

        UpdateGrounded();

        UpdateSpeed();

        UpdateFallSpeed();

        UpdateJump();
    }

    void UpdateGrounded()
    {
        Vector2 size = m_collider.size;
        Vector2 offset = m_collider.offset;
        Vector2 pos = m_collider.transform.position;
        float rot = m_collider.transform.rotation.eulerAngles.z;
        float radRot = Mathf.Deg2Rad * rot;

        pos.y -= m_groundCheckDistance;
        pos.x += Mathf.Cos(rot) * offset.x + Mathf.Sin(rot) * offset.y;
        pos.y += Mathf.Sin(rot) * offset.x + Mathf.Cos(rot) * offset.y;

        var collisions = Physics2D.OverlapBoxAll(pos, size, rot, m_groundMask);

        if(collisions.Length == 0)
        {
            m_grounded = false;
            m_parent = null;
        }
        else
        {
            Transform oldParent = m_parent;
            m_parent = null;
            foreach(var c in collisions)
            {
                if (c.transform == transform)
                    continue;
                if(c.transform == oldParent)
                {
                    m_parent = oldParent;
                    break;
                }
                m_parent = c.transform;
            }
            m_grounded = m_parent != null;
        }

        if (transform.parent != m_parent)
            transform.SetParent(m_parent);

        m_outGroundTIme += Time.deltaTime;
        if (m_grounded)
            m_outGroundTIme = 0;
    }

    void UpdateSpeed()
    {
        float speed = m_rigidbody.velocity.x;
        float acceleration = GetAcceleration();

        float targetSpeed = m_maxSpeed * m_direction.x;

        if ((speed < 0 && targetSpeed > 0) || (speed > 0 && targetSpeed < 0))
            acceleration *= 2;

        if (speed > targetSpeed)
        {
            speed -= acceleration * Time.deltaTime;
            if (speed < targetSpeed)
                speed = targetSpeed;
        }
        else if(speed < targetSpeed)
        {
            speed += acceleration * Time.deltaTime;
            if (speed > targetSpeed)
                speed = targetSpeed;
        }

        Vector2 velocity = m_rigidbody.velocity;
        velocity.x = speed;
        m_rigidbody.velocity = velocity;
    }

    void UpdateFallSpeed()
    {
        Vector2 velocity = m_rigidbody.velocity;
        if (velocity.y < -m_maxFallSpeed)
            velocity.y = -m_maxFallSpeed;

        m_rigidbody.velocity = velocity;
    }

    void UpdateJump()
    {
        if(!m_jumping)
        {
            m_jumpPressedTime = -1;
            m_jumpTime = -1;
        }

        bool canJump = m_outGroundTIme < m_jumpBufferTimerAfterLand;
        bool jumpPressed = m_jumpPressedTime >= 0 && m_jumpPressedTime < m_jumpBufferTimeBeforeLand && m_jumping;

        bool applyVelocity = false;

        if (canJump && jumpPressed && m_jumpTime < 0)
            applyVelocity = true; //start jump
        if (m_jumpTime >= 0 && m_jumpTime < m_maxJumpDuration)
            applyVelocity = true; // longJump

        if(applyVelocity)
        {
            Vector2 velocity = m_rigidbody.velocity;
            velocity.y = m_jumpSpeed;
            m_rigidbody.velocity = velocity;
        }

        if (m_jumpPressedTime >= 0)
            m_jumpPressedTime += Time.deltaTime;
        if (m_jumpTime >= 0)
            m_jumpTime += Time.deltaTime;
        if (m_jumpTime > m_maxJumpDuration)
            m_jumpTime = -1;
    }

    float GetAcceleration()
    {
        if (m_grounded)
            return m_groundAcceleration;

        Vector2 velocity = m_rigidbody.velocity;
        if (Mathf.Abs(velocity.y) < m_jumpApexSpeed)
            return m_groundAcceleration;
        return m_airAcceleration;
    }

    void OnJump(StartJumpEvent e)
    {
        m_jumpPressedTime = 0;
    }
}
