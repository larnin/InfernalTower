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
    //wall jump
    [SerializeField] float m_wallCheckDistance = 0.2f;
    [SerializeField] float m_wallFallSpeed = 1;
    [SerializeField] float m_maxWallDuration = 10;
    [SerializeField] LayerMask m_wallSlideMask;
    //dash
    [SerializeField] float m_dashSpeed = 20;
    [SerializeField] float m_dashDuration = 0.5f;

    Rigidbody2D m_rigidbody = null;
    BoxCollider2D m_collider = null;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector2 m_direction;
    bool m_jumping;

    bool m_grounded = false;
    float m_outGroundTime = -1;
    Transform m_parent = null;

    bool m_onWall = false;
    bool m_wallRight = false;
    float m_onWallTime = -1;
    float m_outWallTime = -1;

    float m_jumpPressedTime = -1;
    float m_jumpTime = -1;
    int m_jumpCount = 0;
    int m_wallJumpCount = 0;
    Vector2 m_jumpDirection = Vector2.zero;

    float m_dashPressedTime = -1;
    float m_dashingTime = -1;
    int m_dashCount = 0;
    Vector2 m_dashDirection = Vector2.zero;

    Vector2 m_oldVelocity;
    Vector2 m_oldPosition;

    private void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();

        m_collider = GetComponent<BoxCollider2D>();
        if (m_collider == null)
            m_collider = GetComponentInChildren<BoxCollider2D>();

        m_subscriberList.Add(new Event<StartJumpEvent>.LocalSubscriber(OnJump, gameObject));
        m_subscriberList.Add(new Event<StartDashEvent>.LocalSubscriber(OnDash, gameObject));
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
        UpdateOnWall();

        UpdateOnWallFall();

        UpdateSpeed();

        UpdateFallSpeed();

        UpdateJump();

        UpdateDash();

        m_oldVelocity = m_rigidbody.velocity;
        m_oldPosition = transform.position;
    }

    void UpdateGrounded()
    {
        Vector2 pos, size;
        float rot;
        GetBoxSizeAndRot(out pos, out size, out rot);
        Vector2 dir = new Vector2(0, -1);

        var collisions = Physics2D.BoxCastAll(pos, size, rot, dir, m_groundCheckDistance, m_groundMask);

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
                if (c.normal.y < 0 || c.normal.y < Mathf.Abs(c.normal.x) * 3)
                    continue;//not enoght vertical
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

    void UpdateOnWall()
    {
        if(m_grounded)
        {
            m_onWall = false;
            m_onWallTime = -1;
            m_outWallTime = -1;
            return;
        }

        Vector2 pos, size;
        float rot;
        GetBoxSizeAndRot(out pos, out size, out rot);

        Vector2[] testDir = new Vector2[] {pos + new Vector2(m_wallCheckDistance, 0), pos - new Vector2(m_wallCheckDistance, 0)};

        bool wasOnWall = m_onWall;
        m_onWall = false;

        for(int i = 0; i < testDir.Length; i++)
        {
            int wasOnWallIndex = m_wallRight ? 0 : 1;
            if (wasOnWall && i != wasOnWallIndex)
                continue;
            
            var dir = testDir[i] / m_wallCheckDistance;

            var collisions = Physics2D.BoxCastAll(pos, size, rot, dir, m_wallCheckDistance, m_groundMask);

            var slideCollisions = Physics2D.BoxCastAll(pos, size, rot, dir, m_wallCheckDistance, m_wallSlideMask);

            if (collisions.Length == 0)
                continue;

            bool onWall = false;
            foreach(var c in collisions)
            {
                if (c.transform == transform)
                    continue;
                if (Mathf.Abs(c.normal.x) < Mathf.Abs(c.normal.y) * 3)
                    continue;//not enoght vertical
                onWall = true;
                break;
            }

            if (onWall)
            {
                foreach (var c in slideCollisions)
                {
                    if (c.transform == transform)
                        continue;
                    if (Mathf.Abs(c.normal.x) < Mathf.Abs(c.normal.y) * 3)
                        continue;//not enoght vertical
                    onWall = false;
                    break;
                }
            }

            if(onWall)
            {
                m_onWall = true;
                m_wallRight = i == 0;
                break;
            }
        }
        
        if(!m_onWall)
        {
            if(m_outWallTime >= 0)
                m_outWallTime += Time.deltaTime;
        }
        else
        {
            m_onWallTime += Time.deltaTime;
            m_outWallTime = 0;
        }
    }

    void UpdateOnWallFall()
    {
        if (!m_onWall)
            return;

        //don't stick on wall on dash or jump
        if (m_dashingTime > 0 || m_jumpTime > 0)
            return;

        if(m_onWallTime < m_maxWallDuration)
        {
            var velocity = m_rigidbody.velocity;
            if (velocity.y < 0)
                velocity.y = 0;
            m_rigidbody.velocity = velocity;
            var position = transform.position;
            position.y = m_oldPosition.y;
            transform.position = position;
        }
        else
        {
            var velocity = m_rigidbody.velocity;
            if (velocity.y < -m_wallFallSpeed)
                velocity.y = -m_wallFallSpeed;
            m_rigidbody.velocity = velocity;
        }
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
        {
            m_jumpCount = 0;
            m_wallJumpCount = 0;
        }

        if(!m_jumping)
        {
            m_jumpPressedTime = -1;
            m_jumpTime = -1;
        }

        bool canJump = m_outGroundTime < m_jumpBufferTimerAfterLand;
        bool jumpPressed = m_jumpPressedTime >= 0 && m_jumpPressedTime < m_jumpBufferTimeBeforeLand && m_jumping && m_dashingTime < 0;

        bool canWallJump = m_outWallTime >= 0 && m_outWallTime < m_jumpBufferTimerAfterLand;

        bool applyVelocity = false;
        
        if ((canJump && jumpPressed && m_jumpTime < 0) //first jump on ground
            || (jumpPressed && m_jumpTime < 0 && !canWallJump)) //air jump
        {
            m_jumpPressedTime = -1;

            CanJumpEvent jumpEvent = new CanJumpEvent(m_jumpCount + 1);
            Event<CanJumpEvent>.Broadcast(jumpEvent, gameObject, true);
            if (jumpEvent.allowed)
            {
                m_jumpCount++;
                applyVelocity = true; //start jump
                m_jumpDirection = Vector2.up * m_jumpSpeed;
            }
        }
        if(canWallJump && jumpPressed && m_jumpTime < 0) //wall jump
        {
            m_jumpPressedTime = -1;
            CanJumpEvent jumpEvent = new CanJumpEvent(m_wallJumpCount + 1, true);
            Event<CanJumpEvent>.Broadcast(jumpEvent, gameObject, true);
            if(jumpEvent.allowed)
            {
                m_wallJumpCount++;
                applyVelocity = true;
                bool jumpUp = m_direction.y > 0.1f && m_direction.y > Mathf.Abs(m_direction.x * 3);
                if(jumpUp)
                    m_jumpDirection = Vector2.up * m_jumpSpeed;
                else
                {
                    //diagonal jump
                    float speedDir = m_jumpSpeed / Mathf.Sqrt(2);
                    m_jumpDirection = new Vector2(m_wallRight ? -speedDir : speedDir, speedDir);
                }

            }
        }
        if (m_jumpTime >= 0 && m_jumpTime < m_maxJumpDuration)
            applyVelocity = true; // longJump

        if(applyVelocity)
        {
            Vector2 velocity = m_rigidbody.velocity;
            if (velocity.y < m_jumpDirection.y)
                velocity.y = m_jumpDirection.y;
            if(m_jumpDirection.x < 0 && velocity.x > m_jumpDirection.x)
                velocity.x = m_jumpDirection.x;
            if (m_jumpDirection.x > 0 && velocity.x < m_jumpDirection.x)
                velocity.x = m_jumpDirection.x;
            m_rigidbody.velocity = velocity;
            m_outGroundTime = m_jumpBufferTimerAfterLand + 1;
            m_outWallTime = m_jumpBufferTimerAfterLand + 1;
        }

        if (m_jumpPressedTime >= 0)
            m_jumpPressedTime += Time.deltaTime;
        if (m_jumpTime >= 0)
            m_jumpTime += Time.deltaTime;
        if (m_jumpTime > m_maxJumpDuration)
            m_jumpTime = -1;
    }

    void UpdateDash()
    {
        if (m_grounded && m_dashingTime < 0)
            m_dashCount = 0;

        if (m_dashingTime >= 0)
            m_dashingTime += Time.deltaTime;

        if(m_dashPressedTime >= 0)
        {
            m_dashPressedTime = -1;

            CanDashEvent dashEvent = new CanDashEvent(m_dashCount + 1);
            Event<CanDashEvent>.Broadcast(dashEvent, gameObject, true);

            if (!dashEvent.allowed)
                return;

            m_dashingTime = 0;
            m_dashDirection = m_direction.normalized * m_dashSpeed;
            m_dashCount++;
        }

        if(m_dashingTime <= m_dashDuration && m_dashingTime >= 0)
            m_rigidbody.velocity = m_dashDirection;

        if(m_dashingTime >= m_dashDuration)
        {
            m_dashingTime = -1;

            var velocity = m_rigidbody.velocity;

            if(velocity.x < -m_maxSpeed)
                velocity.x = -m_maxSpeed;
            if (velocity.x > m_maxSpeed)
                velocity.x = m_maxSpeed;
            if (velocity.y > 0)
                velocity.y = 0;
            if (velocity.y < -m_maxSpeed)
                velocity.y = -m_maxSpeed;

            m_rigidbody.velocity = velocity;
        }
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

    void OnDash(StartDashEvent e)
    {
        m_dashPressedTime = 0;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        const float checkSize = 0.02f;

        Vector2 pos, size;
        float rot;
        GetBoxSizeAndRot(out pos, out size, out rot);
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

    void GetBoxSizeAndRot(out Vector2 outPos, out Vector2 outSize, out float outRot)
    {
        Vector2 size = m_collider.size;
        Vector2 offset = m_collider.offset;
        Vector2 pos = m_collider.transform.position;
        float rot = m_collider.transform.rotation.eulerAngles.z;
        float radRot = Mathf.Deg2Rad * rot;

        pos.x += Mathf.Cos(radRot) * offset.x + Mathf.Sin(radRot) * offset.y;
        pos.y += Mathf.Sin(radRot) * offset.x + Mathf.Cos(radRot) * offset.y;

        outPos = pos;
        outSize = size;
        outRot = rot;
    }
}
