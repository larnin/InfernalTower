using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CanJumpEvent
{
    public int jumpIndex; //first jump is at index 1
    public bool allowed;

    public CanJumpEvent(int _jumpIndex)
    {
        jumpIndex = _jumpIndex;
        if (jumpIndex < 1)
            jumpIndex = 1;
        allowed = jumpIndex == 1;
    }
}

class ModifySpeedEvent
{
    float m_initialSpeed;
    public float initialSpeed { get { return m_initialSpeed; } }
    public float modifiedSpeed;

    public ModifySpeedEvent(float speed)
    {
        m_initialSpeed = speed;
        modifiedSpeed = speed;
    }
}

class ModifyAccelerationEvent
{
    float m_initialAcceleration;
    public float initialAcceleration { get { return m_initialAcceleration; } }
    public float modifiedAcceleration;

    public ModifyAccelerationEvent(float acceleration)
    {
        m_initialAcceleration = acceleration;
        modifiedAcceleration = acceleration;
    }
}