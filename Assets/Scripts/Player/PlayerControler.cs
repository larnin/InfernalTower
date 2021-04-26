using UnityEngine;
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
    //bump on wall and ceil
    [SerializeField] float m_bumpHeadCorrection = 0.5f;
    [SerializeField] float m_bumpWallCorrection = 0.5f;
    [SerializeField] LayerMask m_bumpMask;

    Rigidbody2D m_rigidbody = null;
    BoxCollider2D m_collider = null;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector2 m_direction;
    bool m_jumping;

    bool m_grounded = false;
    float m_outGroundTime = -1;
    Transform m_parent = null;

    float m_jumpPressedTime = -1;
    float m_jumpTime = -1;
    int m_jumpCount = 0;

    Vector2 m_oldVelocity;

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

        m_oldVelocity = m_rigidbody.velocity;
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

        m_outGroundTime += Time.deltaTime;
        if (m_grounded)
            m_outGroundTime = 0;
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
        if (m_jumpPressedTime == -1 && m_grounded)
            m_jumpCount = 0;

        if(!m_jumping)
        {
            m_jumpPressedTime = -1;
            m_jumpTime = -1;
        }

        bool canJump = m_outGroundTime < m_jumpBufferTimerAfterLand;
        bool jumpPressed = m_jumpPressedTime >= 0 && m_jumpPressedTime < m_jumpBufferTimeBeforeLand && m_jumping;

        bool applyVelocity = false;
        
        if ((canJump && jumpPressed && m_jumpTime < 0) //first jump on ground
            || (jumpPressed && m_jumpTime < 0)) //air jump
        {
            m_jumpPressedTime = -1;

            CanJumpEvent jumpEvent = new CanJumpEvent(m_jumpCount + 1);
            Event<CanJumpEvent>.Broadcast(jumpEvent, gameObject, true);
            if (jumpEvent.allowed)
            {
                m_jumpCount++;
                applyVelocity = true; //start jump
            }
        }
        if (m_jumpTime >= 0 && m_jumpTime < m_maxJumpDuration)
            applyVelocity = true; // longJump

        if(applyVelocity)
        {
            Vector2 velocity = m_rigidbody.velocity;
            velocity.y = m_jumpSpeed;
            m_rigidbody.velocity = velocity;
            m_outGroundTime = m_jumpBufferTimerAfterLand + 1;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        const float checkSize = 0.02f;

        Vector2 size = m_collider.size;
        Vector2 offset = m_collider.offset;
        Vector2 pos = m_collider.transform.position;
        float rot = m_collider.transform.rotation.eulerAngles.z;
        float radRot = Mathf.Deg2Rad * rot;
        
        pos.x += Mathf.Cos(rot) * offset.x + Mathf.Sin(rot) * offset.y;
        pos.y += Mathf.Sin(rot) * offset.x + Mathf.Cos(rot) * offset.y;
        pos += m_oldVelocity * Time.deltaTime;

        if (Physics2D.OverlapBox(pos, size, rot, m_bumpMask) == null)
            return;
        
        //nearly vertical jump
        if (m_oldVelocity.y > 0 && m_oldVelocity.y > Mathf.Abs(m_oldVelocity.x * 3))
        {
            var contact = collision.contacts[0];
            if (Mathf.Abs(contact.normal.y) < Mathf.Abs(contact.normal.x) * 3)
                return;

            int nbTest = Mathf.CeilToInt(m_bumpHeadCorrection / checkSize);
            for(int i = 0; i < nbTest * 2; i++)
            {
                int index = i / 2;
                int direction = i % 2 == 0 ? 1 : -1;
                Vector2 testOffset = new Vector2(index * checkSize * direction, 0);
                var testPos = pos + testOffset;
                if(Physics2D.OverlapBox(testPos, size, rot, m_bumpMask) == null)
                {
                    transform.position = transform.position + new Vector3(testOffset.x, testOffset.y, 0);
                    m_rigidbody.velocity = m_oldVelocity;
                    return;
                }
            }
        }

        //nearly horizontal
        if(m_oldVelocity.y <= 0 && Mathf.Abs(m_oldVelocity.x) > Mathf.Abs(m_oldVelocity.y * 2))
        {
            var contact = collision.contacts[0];
            if (Mathf.Abs(contact.normal.x) < Mathf.Abs(contact.normal.y) * 2)
                return;

            int nbTest = Mathf.CeilToInt(m_bumpWallCorrection / checkSize);
            for(int i = 0; i < nbTest; i++)
            {
                Vector2 testOffset = new Vector2(0, i * checkSize);
                var testPos = pos + testOffset;
                if(Physics2D.OverlapBox(testPos, size, rot, m_bumpMask) == null)
                {
                    transform.position = transform.position + new Vector3(testOffset.x, testOffset.y, 0);
                    m_rigidbody.velocity = m_oldVelocity;
                    return;
                }
            }
        }
    }
}
