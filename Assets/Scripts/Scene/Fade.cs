using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    static Fade instance = null;

    [SerializeField] float m_transitionDuration = 0.5f;

    SubscriberList m_subscriberList = new SubscriberList();

    Image m_plane;
    Color m_baseColor;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        m_subscriberList.Add(new Event<ShowLoadingScreenEvent>.Subscriber(OnFade));
        m_subscriberList.Subscribe();

        m_plane = GetComponentInChildren<Image>();
        if (m_plane != null)
            m_baseColor = m_plane.color;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnFade(ShowLoadingScreenEvent e)
    {
        if (e.start)
        {
            Color c = m_baseColor;
            c.a = 1;
            m_plane.gameObject.SetActive(true);
            m_plane.DOColor(c, m_transitionDuration);
        }
        else
        {
            Color c = m_baseColor;
            c.a = 0;
            m_plane.DOColor(c, m_transitionDuration).OnComplete(()=>m_plane.gameObject.SetActive(false));
        }
    }
}
