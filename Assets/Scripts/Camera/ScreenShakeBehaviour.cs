using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NRand;

public class ScreenShakeBehaviour : MonoBehaviour
{
    List<ScreenShakeBase> m_screenShakes = new List<ScreenShakeBase>();
    SubscriberList m_subsctiberList = new SubscriberList();

    Camera m_camera = null;
    float m_initialCameraSize = 0;

    MT19937 m_rand = new MT19937();

    private void Awake()
    {
        m_camera = GetComponentInChildren<Camera>();

        m_subsctiberList.Add(new Event<AddScreenShakeEvent>.Subscriber(OnAddShake));
        m_subsctiberList.Add(new Event<StopScreenShakeEvent>.Subscriber(OnStopShake));
        m_subsctiberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subsctiberList.Unsubscribe();
    }

    private void Start()
    {
        m_initialCameraSize = m_camera.orthographicSize;
    }

    void Update()
    {
        Vector2 offset = Vector2.zero;
        float scale = 0;
        float rotation = 0;

        float t = Time.deltaTime;

        foreach(var shake in m_screenShakes)
        {
            shake.Update(t, m_rand);

            offset += shake.GetOffset();
            scale += shake.GetScale();
            rotation += shake.GetRotation();
        }

        transform.localPosition = offset;
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
        m_camera.orthographicSize = m_initialCameraSize + scale;
    }

    void OnAddShake(AddScreenShakeEvent e)
    {
        m_screenShakes.Add(e.screenShake);
    }

    void OnStopShake(StopScreenShakeEvent e)
    {
        m_screenShakes.Clear();
    }
}
