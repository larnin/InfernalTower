using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class StartJumpEvent { }
class EndJumpEvent { }

class GetDirectionEvent
{
    public Vector2 direction;
}

class GetJumpEvent
{
    public bool jump;
}