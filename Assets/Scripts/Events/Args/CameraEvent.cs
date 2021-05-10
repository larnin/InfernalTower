using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class MoveCameraEvent
{
    public Vector2 pos;
    public int weight;

    public MoveCameraEvent(Vector2 _pos, int _weight = 1)
    {
        pos = _pos;
        weight = _weight;
    }
}

class InstantMoveCameraEvent
{
    public Vector2 pos;
    public int weight;

    public InstantMoveCameraEvent(Vector2 _pos, int _weight = 1)
    {
        pos = _pos;
        weight = _weight;
    }
}

class SetLockAreaCameraEvent
{
    public BoxCollider2D collider;

    public SetLockAreaCameraEvent(BoxCollider2D _collider)
    {
        collider = _collider;
    }
}