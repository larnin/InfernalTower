using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class StartJumpEvent { }
class EndJumpEvent { }

class StartDashEvent { }

class StartAttackEvent { }
class EndAttackEvent { }

class GetDirectionEvent
{
    public Vector2 direction;
}

class GetJumpEvent
{
    public bool jump;
}

class GetDashEvent
{
    public bool dash;
}

class GetAttackEvent
{
    public bool Attack;
}

class GetAimEvent
{
    public Vector2 controlerDirection;
    public Vector2 mousePosition;
}
