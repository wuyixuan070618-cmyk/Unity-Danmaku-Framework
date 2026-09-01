using UnityEngine;
public class TimeCondition : MonoBehaviour,ITriggerCondition
{
    [SerializeField] private float delay=1f;
    private float _elapsedTime;
    public void ResetRuntimeState()
    {
        _elapsedTime=0f;
    }
    public bool Tick(float deltaTime)
    {
        _elapsedTime+=deltaTime;
        return _elapsedTime>=delay;
    }
}
