using QualityCompany.Modules.Core;
using UnityEngine;
using static QualityCompany.Events.GameEvents;

namespace QualityCompany.Modules.Inventory
{
    [Module(Delayed = true)]
    internal class ShotgunAmmoModule : InventoryBaseUI
    {
        private static readonly Color TextColorFull = new(0f, 1f, 0f, 0.75f);
        private static readonly Color TextColorHalf = new(0.953f, 0.953f, 0.141f, 0.75f);  // Converted to 0-1 range.
        private static readonly Color TextColorEmpty = new(1f, 0f, 0f, 0.75f);

        public ShotgunAmmoModule() : base(nameof(ShotgunAmmoModule))
        { }

        [ModuleOnLoad]
        private static ShotgunAmmoModule SpawnShotgunAmmoModule()
        {
            if (!Plugin.Instance.PluginConfig.InventoryShowShotgunAmmoCounterUI)
            {
                Debug.Log("Shotgun Ammo Module UI not enabled in config.");
                return null;
            }

            var go = new GameObject(nameof(ShotgunAmmoModule));
            return go.AddComponent<ShotgunAmmoModule>();
        }

        private void Awake()
        {
            base.Awake();
            
            // Initialize ammo UI slots based on the player’s inventory slots
            for (var i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
            {
                Texts.Add(CreateInventoryGameObject($"qc_HUDShotgunAmmoUI{i}", 16, 
                    HUDManager.Instance.itemSlotIconFrames[i].gameObject.transform));
            }
        }

        [ModuleOnAttach]
        private void AttachEvents()
        {
            PlayerGrabObjectClientRpc += OnUpdate;
            PlayerThrowObjectClientRpc += OnUpdate;
            PlayerDiscardHeldObject += OnUpdate;
            PlayerDropAllHeldItems += HideAll;
            PlayerDeath += HideAll;
            PlayerShotgunShoot += OnUpdate;
            PlayerShotgunReload += OnUpdate;
        }

        protected override void OnUpdate(GrabbableObject item, int index)
        {
            if (item == null) return;  // Added null check for item
            
            var shotgunItem = item.GetComponent<ShotgunItem>();
            if (shotgunItem == null) return;

            var shellsLoaded = shotgunItem.shellsLoaded;
            var color = shellsLoaded switch
            {
                2 => TextColorFull,
                1 => TextColorHalf,
                _ => TextColorEmpty
            };

            UpdateItemSlotText(index, shellsLoaded.ToString(), color);
        }
    }
}
