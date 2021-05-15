using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    const string InputsName = "Player";
    const string MoveName = "Move";
    const string JumpName = "Jump";
    const string DashName = "Dash";
    const string AttackName = "Attack";
    const string TargetMouseName = "TargetMouse";
    const string TargetGamepadName = "TargetGamepad";

    PlayerInput m_inputs;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector2 m_direction = Vector2.zero;
    bool m_jump = false;
    bool m_dash = false;
    
    void Start()
    {
        m_subscriberList.Add(new Event<GetDirectionEvent>.LocalSubscriber(GetDirection, gameObject));
        m_subscriberList.Add(new Event<GetJumpEvent>.LocalSubscriber(GetJump, gameObject));
        m_subscriberList.Add(new Event<GetDashEvent>.LocalSubscriber(GetDash, gameObject));
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

        string deviceClass = e.control.device.description.deviceClass;
        InputType type = Settings.instance.inputType;

        if (deviceClass == "Mouse" || deviceClass == "Keyboard")
        {
            if (type != InputType.Keyboard)
                Settings.instance.inputType = InputType.Keyboard;
        }
        else if (type != InputType.Gamepad)
            Settings.instance.inputType = InputType.Gamepad;

        if(e.action.name == MoveName)
        {
            if(e.phase == InputActionPhase.Started || e.phase == InputActionPhase.Performed)
            {
                m_direction = e.ReadValue<Vector2>();
            }
            else if(e.phase == InputActionPhase.Disabled || e.phase == InputActionPhase.Canceled)
            {
                m_direction = Vector2.zero;
            }
        }
        else if(e.action.name == JumpName)
        {
            if(e.phase == InputActionPhase.Started)
            {
                m_jump = true;
                Event<StartJumpEvent>.Broadcast(new StartJumpEvent(), gameObject, true);
            }
            else if(e.phase == InputActionPhase.Canceled)
            {
                m_jump = false;
                Event<EndJumpEvent>.Broadcast(new EndJumpEvent(), gameObject, true);
            }
        }
        else if(e.action.name == DashName)
        {
            if (e.phase == InputActionPhase.Started)
            {
                m_dash = true;
                Event<StartDashEvent>.Broadcast(new StartDashEvent(), gameObject, true);
            }
            else if (e.phase == InputActionPhase.Canceled)
            {
                m_dash = false;
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

    void GetDash(GetDashEvent e)
    {
        e.dash = m_dash;
    }
}
