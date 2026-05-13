using Photon.Pun;

public class bl_AIPlayerTarget : bl_AITarget
{
    public bl_PlayerReferencesCommon playerReferences = null;
    private string targetName;

    public override void Awake()
    {
        base.Awake();
        targetName = playerReferences != null ? playerReferences.PlayerName : transform.root.name;
        if (playerReferences != null) { playerReferences.onDie += MarkAsDeath; }
    }

    public override void OnAttacked(bl_PlayerReferencesCommon attacker)
    {
        if (playerReferences != null)
        {
            playerReferences.OnAttacked(attacker);
        }
    }

    public override string Name
    {
        get => targetName;
        set => targetName = value;
    }

    public override Team GetTeam()
    {
        if (playerReferences != null)
        {
            return playerReferences.PlayerTeam;
        }

        return Team.None;
    }

    public override PhotonView GetNetView()
    {
        if (playerReferences != null)
        {
            return playerReferences.NetworkView;
        }

        return null;
    }
}
