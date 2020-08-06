using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private float m_radius;
    [SerializeField] private Transform m_target;
    [SerializeField] private float m_speed;
    [SerializeField] private float m_maxForce = 10f;

    /// <summary>
    /// Agent能检测到多大范围内的邻居
    /// </summary>
    [SerializeField] private float m_sensingRadius = 5;

    /// <summary>
    /// 只考虑该时间范围内会发生的碰撞
    /// 进攻性强的可以设小一点，内向的可以设大一点
    /// </summary>
    [SerializeField] private float m_timeHorizon = 4f;

    public float Radius { get { return m_radius; } }
    public Vector2 Velocity { get; set; }
    public Vector2 Force { get; set; }
    public float TimeHorizon { get { return m_timeHorizon; } }
    public float SensingRadius { get { return m_sensingRadius; } }
    public float MaxForce { get { return m_maxForce; } }

    public Vector2 GoalVelocity
    {
        get
        {
            if (m_target != null)
            {
                Vector2 p = new Vector2(m_target.position.x, m_target.position.z);
                Vector2 dir = (p - Pos2).normalized;
                return dir * m_speed;
            }
            else
            {
                return Vector2.zero;
            }
        }
    }

    public Vector2 Pos2
    {
        get
        {
            var pos3 = transform.position;
            return new Vector2(pos3.x, pos3.z);
        }

        set
        {
            transform.position = new Vector3(value.x, 0, value.y);
        }
    }

    private void Start()
    {
        TimeToCollisionManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        TimeToCollisionManager.Instance.Unregister(this);
    }

    private void OnDrawGizmos()
    {
        if (m_target != null)
            Gizmos.DrawLine(transform.position, m_target.position);
    }
}
