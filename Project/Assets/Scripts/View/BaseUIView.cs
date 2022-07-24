using Game.UI.FSM;
using UnityEngine;

public class BaseUIView : MonoBehaviour
{
    
    protected bool _isActive = false;

    public virtual void Show()
    {
        gameObject.SetActive(true);
        _isActive = true;
    }
		
    public virtual void Hide()
    {
        gameObject.SetActive(false);
        _isActive = false;
    }
		
    public virtual void Pause()
    {
        _isActive = false;
    }
		
    public virtual void Resume()
    {
        _isActive = true;
    }

    /*
    protected T GetState<T>() where T : FSMState
    {
        return States.State<T>();
    }*/
}