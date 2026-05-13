using MFPS.Internal.Structures;
using MFPSEditor;
using UnityEngine;

namespace MFPS.Core
{
    public class MFPSGameItem
    {
        public string Name;
        [SpritePreview(AutoScale = true)] public Sprite Icon;
        public MFPSItemUnlockability Unlockability;

        [HideInInspector] public int ItemID;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="icon"></param>
        public void SetIcon(Texture2D icon)
        {
            // convert the texture to a sprite
            Icon = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MFPSItemUnlockability.ItemTypeEnum GetItemType()
        {
            return Unlockability.ItemType;
        }
    }
}