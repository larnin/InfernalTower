using UnityEngine;
using System.Collections;

public class LockCameraOnEnable : MonoBehaviour
{
    bool m_enabled = false;

    private void OnEnable()
    {
        m_enabled = true;
    }

    private void Update()
    {
        if(m_enabled)
        {
            var collider = GetComponentInChildren<BoxCollider2D>();
            if (collider == null)
                return;

            Event<SetLockAreaCameraEvent>.Broadcast(new SetLockAreaCameraEvent(collider));

            m_enabled = false;
        }
    }
}
