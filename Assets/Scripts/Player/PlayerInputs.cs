using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    const string InputsName = "Player";
    const string MoveName = "Move";
    const string JumpName = "Jump";

    PlayerInput m_inputs;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector2 m_direction = Vector2.zero;
    bool m_jump = false;
    
    void Start()
    {
        m_subscriberList.Add(new Event<GetDirectionEvent>.LocalSubscriber(GetDirection, gameObject));
        m_subscriberList.Add(new Event<GetJumpEvent>.LocalSubscriber(GetJump, gameObject));
        m_subscriberList.Subscribe();

        m_inputs = GetComponent<PlayerInput>();
        if (m_inputs)
            m_inputs.onActionTriggered += OnInput;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void OnInput(InputAction.CallbackContext e)
    {
        if (e.action == null)
            return;
        if (e.action.actionMap == null)
            return;
        if (e.action.actionMap.name != InputsName)
            return;

        if(e.action.name == MoveName)
        {
            if(e.phase == InputActionPhase.Started || e.phase == InputActionPhase.Performed)
            {
                m_direction = e.ReadValue<Vector2>();
            }
            else if(e.phase == InputActionPhase.Disabled)
            {
                m_direction = Vector2.zero;
            }
        }
        else if(e.action.name == JumpName)
        {
            if(e.phase == InputActionPhase.Started)
            {
                m_jump = true;
                Event<StartJumpEvent>.Broadcast(new StartJumpEvent(), gameObject);
            }
            else if(e.phase == InputActionPhase.Canceled)
            {
                m_jump = false;
                Event<EndJumpEvent>.Broadcast(new EndJumpEvent(), gameObject);
            }
        }
    }

    void GetDirection(GetDirectionEvent e)
    {
        e.direction = m_direction;
    }

    void GetJump(GetJumpEvent e)
    {
        e.jump = m_jump;
    }
}
