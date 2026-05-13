using UnityEngine;

/// <summary>
/// This script is not used by default in the MFPS core, but is used in some mods.
/// 
/// The purpose of this script is to be able to have the references of the player model and it's local postion inside the player prefab
/// So we are able to separate the player model from the player prefab, in order to have a single player prefab and multiple player models.
/// That way changes made to the player prefab just have to be done once.
/// 
/// This script should be attached where the <see cref="bl_PlayerAnimationsBase"/> script is attached in the player model.
/// Then fetch the information of the player model and setup > unattach the player model from the player prefab and save as a separate prefab.
/// </summary>
public class bl_PlayerModel : bl_PlayerModelBase
{

    private void OnDrawGizmosSelected()
    {
        // Draw a wireframe cylinder at the player's position
        bl_GizmosUtility.DrawWireCylinderFromBase(transform.root.position, 0.5f, 2.0f, 4, new Color(0.7187521f, 1f, 0.2688679f, 0.3f));
    }
}