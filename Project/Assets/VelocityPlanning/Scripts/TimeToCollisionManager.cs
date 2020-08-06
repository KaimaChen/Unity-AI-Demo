using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 预测出什么时候会发生碰撞，然后进行规避
/// </summary>
public class TimeToCollisionManager : MonoBehaviour
{
    public static TimeToCollisionManager Instance;

    private readonly List<Agent> m_agents = new List<Agent>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateAllAgents();
    }

    public void Register(Agent agent)
    {
        m_agents.Add(agent);
    }

    public void Unregister(Agent agent)
    {
        m_agents.Remove(agent);
    }

    /// <summary>
    /// 什么时候会发生碰撞
    /// 参考：Game AI Pro 2 Chapter19
    /// </summary>
    private static float TimeToCollision(Agent i, Agent j)
    {
        //满足 ||(Pb + Vb * t) - (Pa + Va * t)|| = Ra + Rb 时，a和b发生碰撞
        //进行一些分解后就能得到一个二元二次方程，然后使用求解公式即可

        float r = i.Radius + j.Radius;
        Vector2 w = i.transform.position - j.transform.position;
        float c = Vector2.Dot(w, w) - r * r;
        if (c < 0) //agents are colliding
            return 0;

        Vector2 v = i.Velocity - j.Velocity;
        float a = Vector2.Dot(v, v);
        float b = Vector2.Dot(w, v);
        float discr = b * b - a * c;
        if (discr <= 0)
            return float.MaxValue;

        float tau = (b - Mathf.Sqrt(discr)) / a;
        if (tau < 0)
            return float.MaxValue;

        return tau;
    }

    private void UpdateAllAgents()
    {
        foreach(Agent agent in m_agents)
        {
            agent.Force = 2 * (agent.GoalVelocity - agent.Velocity);

            List<Agent> neighbors = FindNeighbors(agent);
            foreach(var neighbor in neighbors)
            {
                float t = TimeToCollision(agent, neighbor);

                //计算力方向（就是邻居指向自己的向量）
                Vector2 avoidForce = agent.Pos2 + agent.Velocity * t - neighbor.Pos2 - neighbor.Velocity * t;
                if (avoidForce != Vector2.zero)
                    avoidForce.Normalize();

                //计算力大小（越接近0越大，超过TimeHorizon则为0）
                float mag = 0;
                if (t >= 0 && t <= agent.TimeHorizon)
                    mag = (agent.TimeHorizon - t) / (t + 0.001f);
                if (mag > agent.MaxForce)
                    mag = agent.MaxForce;
                avoidForce *= mag;

                agent.Force += avoidForce;
            }
        }

        foreach(Agent agent in m_agents)
        {
            agent.Velocity += agent.Force * Time.deltaTime;
            agent.Pos2 += agent.Velocity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 寻找某个单位附近的邻居
    /// 可以考虑使用四叉树等优化性能
    /// </summary>
    private List<Agent> FindNeighbors(Agent curt)
    {
        List<Agent> result = new List<Agent>();

        float sqrSensingDist = curt.SensingRadius * curt.SensingRadius;
        for(int i = 0; i < m_agents.Count; i++)
        {
            Agent neighbor = m_agents[i];
            if (neighbor == curt)
                continue;

            float sqrDistance = (neighbor.Pos2 - curt.Pos2).sqrMagnitude;
            if (sqrDistance <= sqrSensingDist)
                result.Add(neighbor);
        }

        return result;
    }
}