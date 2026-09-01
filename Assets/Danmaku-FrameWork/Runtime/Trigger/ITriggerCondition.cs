public interface ITriggerCondition
{
    void ResetRuntimeState();
    bool Tick(float deltaTime);
}