using UnityEngine;
public struct BulletSpawnContext
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector2 direction;
    public Transform owner;
    public Transform target;
    public BulletSpawnContext(
        Vector3 pos,
        Vector2 dir,
        Transform owner = null, 
        Transform target = null
    )
    {
        this.position=pos;
        if (dir.magnitude < 1e-6f)
        {
            Debug.LogError("方向向量过小");
            this.direction=Vector2.zero;
        }
        else{
            this.direction=dir.normalized;
        }
        this.rotation=Quaternion.FromToRotation(Vector2.up,dir);
        this.owner=owner;
        this.target=target;
    }
}