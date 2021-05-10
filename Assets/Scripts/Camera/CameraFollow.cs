using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float m_minSpeed = 1;
    [SerializeField] float m_maxSpeed = 50;
    [SerializeField] float m_speed = 1;
    [SerializeField] float m_speedPow = 1;
    [SerializeField] float m_forgetTime = 0.2f;

    class TargetData
    {
        public Vector2 target;
        public int weight;
        public float duration;
    }

    BoxCollider2D m_lockArea = null;
    Camera m_camera = null;

    List<TargetData> m_targets = new List<TargetData>();

    Vector2 m_currentPosition;

    SubscriberList m_subscriberList = new SubscriberList();

    void Awake()
    {
        m_camera = GetComponentInChildren<Camera>();

        m_subscriberList.Add(new Event<MoveCameraEvent>.Subscriber(OnMove));
        m_subscriberList.Add(new Event<InstantMoveCameraEvent>.Subscriber(OnInstantMove));
        m_subscriberList.Add(new Event<SetLockAreaCameraEvent>.Subscriber(OnLockCamera));
        m_subscriberList.Subscribe();

        m_currentPosition = transform.position;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void FixedUpdate()
    {
        UpdateTargets();

        var target = GetTarget();

        var dir = target - m_currentPosition;

        float dist = dir.magnitude;
        float speed = m_speed * Mathf.Pow(dist, m_speedPow);
        speed = Mathf.Clamp(speed, m_minSpeed, m_maxSpeed);


        float moveDist = speed * Time.deltaTime;
        if (moveDist < dist && dist > 0.01f)
            dir *= moveDist / dist;

        m_currentPosition += dir;

        LockCurrentPosition();

        var z = transform.position.z;

        transform.position = new Vector3(m_currentPosition.x, m_currentPosition.y, z);
    }

    Vector2 GetTarget()
    {
        TargetData target = null;

        foreach(var t in m_targets)
        {
            if (target == null || target.weight < t.weight)
                target = t;
        }

        if (target == null)
            return m_currentPosition;

        return target.target;
    }

    void UpdateTargets()
    {
        List<TargetData> newTargets = new List<TargetData>();

        foreach(var t in m_targets)
        {
            t.duration += Time.deltaTime;

            if (t.duration < m_forgetTime)
                newTargets.Add(t);
        }

        m_targets = newTargets;
    }

    void LockCurrentPosition()
    {
        if (m_lockArea == null || m_camera == null)
            return;

        //the area need to be aabb
        var bounds = m_lockArea.bounds;

        float height = m_camera.orthographicSize;
        float width = height * m_camera.aspect;

        Vector3 size = bounds.extents;
        size.x -= width;
        size.y -= height;

        if (size.x < 0)
            size.x = 0;
        if (size.y < 0)
            size.y = 0;

        Vector2 topLeft = bounds.center - size;
        Vector2 downRight = bounds.center + size;

        if (m_currentPosition.x < topLeft.x)
            m_currentPosition.x = topLeft.x;
        if (m_currentPosition.y < topLeft.y)
            m_currentPosition.y = topLeft.y;
        if (m_currentPosition.x > downRight.x)
            m_currentPosition.x = downRight.x;
        if (m_currentPosition.y > downRight.y)
            m_currentPosition.y = downRight.y;
    }

    void UpdateTarget(Vector2 pos, int weight)
    {
        foreach (var t in m_targets)
        {
            if (t.weight != weight)
                continue;

            t.target = pos;
            t.duration = 0;

            return;
        }

        TargetData target = new TargetData();
        target.duration = 0;
        target.target = pos;
        target.weight = weight;
        m_targets.Add(target);
    }

    void OnMove(MoveCameraEvent e)
    {
        UpdateTarget(e.pos, e.weight);
    }

    void OnInstantMove(InstantMoveCameraEvent e)
    {
        UpdateTarget(e.pos, e.weight);

        int bestWeight = int.MinValue;

        foreach (var t in m_targets)
        {
            if (t.weight > bestWeight)
                bestWeight = t.weight;
        }

        if(bestWeight <= e.weight)
            m_currentPosition = e.pos;
    }

    void OnLockCamera(SetLockAreaCameraEvent e)
    {
        m_lockArea = e.collider;
    }
}
