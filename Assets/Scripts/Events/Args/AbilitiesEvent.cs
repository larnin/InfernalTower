using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class CanJumpEvent
{
    public int jumpIndex; //first jump is at index 1
    public bool wallJump;
    public bool allowed;

    public CanJumpEvent(int _jumpIndex, bool _wallJump = false)
    {
        jumpIndex = _jumpIndex;
        if (jumpIndex < 1)
            jumpIndex = 1;
        allowed = jumpIndex == 1;
        wallJump = _wallJump;
    }
}

class CanWallJumpEvent
{
    public bool allowed;

    public CanWallJumpEvent()
    {
        allowed = false;
    }
}

class CanDashEvent
{
    public int dashIndex; //first dash is at index 1
    public bool allowed;

    public CanDashEvent(int _dashIndex)
    {
        dashIndex = _dashIndex;
        if (dashIndex < 1)
            dashIndex = 1;
        allowed = dashIndex == 1;
    }
}
