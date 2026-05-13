using MFPS.Runtime.AI;
using UnityEngine;
using UnityEngine.AI;

public abstract class bl_AIShooter : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public AIAgentState AgentState
    {
        get;
        set;
    } = AIAgentState.Idle;

    /// <summary>
    /// 
    /// </summary>
    public bl_AITarget Target
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual bl_AITarget AimTarget
    {
        get;
        protected set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract float CachedTargetDistance
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public AILookAt LookingAt
    {
        get;
        set;
    } = AILookAt.Path;

    /// <summary>
    /// 
    /// </summary>
    public bool IsDeath
    {
        get;
        set;
    } = false;

    /// <summary>
    /// 
    /// </summary>
    public Team AITeam
    {
        get;
        set;
    } = Team.None;

    /// <summary>
    /// 
    /// </summary>
    public bool FocusOnSingleTarget
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsTeamMate
    {
        get
        {
            return (AITeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() && !isOneTeamMode);
        }
    }

    public virtual NavMeshAgent Agent
    {
        get;
        set;
    }

    private string _AIName = string.Empty;
    public string AIName
    {
        get
        {
            return _AIName;
        }
        set
        {
            _AIName = value;
            gameObject.name = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// 
    /// </summary>
    public virtual void OnDeath() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void CheckTargets() { }

    /// <summary>
    /// 
    /// </summary>
    public abstract void Respawn();

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdateTargetList();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromPosition"></param>
    public abstract void OnGetHit(Vector3 fromPosition);

    /// <summary>
    /// Should be called when this bot is attacked (hit or almost hit)
    /// </summary>
    public virtual void OnAttacked(bl_PlayerReferencesCommon attacker) { }

    /// <summary>
    /// 
    /// </summary>
    public virtual Vector3 TargetPosition
    {
        get => Target.position;
    }

    /// <summary>
    /// 
    /// </summary>
    private Vector3 m_lookAtDirection = Vector3.forward;
    public virtual Vector3 LookAtDirection
    {
        get => m_lookAtDirection;
        set => m_lookAtDirection = value;
    }

    /// <summary>
    /// 
    /// </summary>
    private Vector3 m_lookAtPosition = Vector3.forward;
    public virtual Vector3 LookAtPosition
    {
        get => m_lookAtPosition;
        set => m_lookAtPosition = value;
    }

    public Vector3 Position
    {
        get => transform.localPosition;
    }

    /// <summary>
    /// Get a random position around the given position
    /// </summary>
    /// <param name="positionReference"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public Vector3 GetPositionAround(Vector3 positionReference, float radius)
    {
        var v = Random.insideUnitSphere * radius;
        v.y = 0;
        v += positionReference;
        return v;
    }

    /// <summary>
    /// Get a random point on the NavMesh behind the bot
    /// </summary>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public Vector3 GetRandomPointBehind(float maxDistance)
    {
        Vector3 backwardDirection = -CachedTransform.forward; // The direction behind the bot
        Vector3 randomPoint;

        // Try finding a valid point on the NavMesh
        for (int i = 0; i < 10; i++) // Try up to 10 times to find a valid point
        {
            // Generate a random point within a circle behind the bot
            Vector3 randomDirection = backwardDirection + (Random.insideUnitSphere * Random.Range(1, maxDistance));
            randomDirection.y = 0; // Keep the point on the horizontal plane
            randomPoint = CachedTransform.position + randomDirection;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // If no valid point is found, return the bot's current position
        return CachedTransform.position;
    }

    /// <summary>
    /// Get a random point at a given distance in a given direction from the bot (left, right, or behind)
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="distance"></param>
    /// <param name="direction">-1 = left, 0 = behind, 1 = right</param>
    /// <returns></returns>
    public Vector3 GetRandomPointAtDirection(float distance, int direction = -2)
    {
        Vector3 directionVector = Vector3.zero;
        if (direction == -2) direction = Random.Range(-1, 2);

        switch (direction)
        {
            case -1:
                directionVector = -CachedTransform.right;
                break;
            case 1:
                directionVector = CachedTransform.right;
                break;
            case 0:
                directionVector = -CachedTransform.forward;
                break;
        }

        // Try finding a valid point on the NavMesh
        for (int i = 0; i < 5; i++) // Try up to 5 times to find a valid point
        {
            Vector3 randomOffset = 0.5f * distance * Random.insideUnitSphere; // Half the distance to ensure randomness
            randomOffset.y = 0; // Keep the point on the horizontal plane

            Vector3 targetPoint = CachedTransform.position + (directionVector * distance) + randomOffset;

            if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, distance, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return CachedTransform.position;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float RandomRange(float min, float max)
    {
        return Random.Range(min, max);
    }

    private bl_AIShooterReferences _references;
    public bl_AIShooterReferences References
    {
        get
        {
            if (_references == null) _references = GetComponent<bl_AIShooterReferences>();
            return _references;
        }
    }

}