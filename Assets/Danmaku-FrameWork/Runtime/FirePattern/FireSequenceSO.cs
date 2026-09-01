using UnityEngine;

[CreateAssetMenu(menuName ="STG/Fire Sequence")]
public class FireSequenceSO : ScriptableObject
{
    public FirePhaseSO[] phases;//按顺序排列的FirePhase
    public bool loop=true;//是否循环
    public int loopCount=1;//循环次数,-1为无限
}