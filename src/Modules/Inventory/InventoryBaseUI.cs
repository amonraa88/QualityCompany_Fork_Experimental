using GameNetcodeStuff;
using QualityCompany.Service;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace QualityCompany.Modules.Inventory
{
    internal abstract class InventoryBaseUI : MonoBehaviour
    {
        protected ModLogger Logger;
        protected readonly List<TextMeshProUGUI> Texts = new();
        private GameObject _baseTextToCopyGameObject;

        protected void Awake()
        {
            _baseTextToCopyGameObject = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube/Canvas (1)/MainContainer/HeaderText");
            if (_baseTextToCopyGameObject == null)
            {
                Logger?.LogError("Base text GameObject not found!");
                return;
            }
            Logger = new ModLogger("InventoryModule");
            
            transform.SetParent(HUDManager.Instance.HUDContainer.transform);
            transform.position = Vector3.zero;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        #region UI Updates
        protected void OnUpdate(PlayerControllerB instance)
        {
            if (!instance.IsOwner) return;

            if (Plugin.Instance.PluginConfig.InventoryForceUpdateAllSlotsOnDiscard)
            {
                ForceUpdateAllSlots(instance);
                return;
            }

            if (instance.currentlyHeldObjectServer is null)
            {
                Hide(instance.currentItemSlot);
                return;
            }

            OnUpdate(instance.currentlyHeldObjectServer, instance.currentItemSlot);
        }

        protected abstract void OnUpdate(GrabbableObject go, int index);
        #endregion

        #region UI
        protected TextMeshProUGUI CreateInventoryGameObject(string gameObjectName, int fontSize, Transform parent, Vector3? localPositionDelta = null)
        {
            var textObject = Instantiate(_baseTextToCopyGameObject, parent);
            textObject.name = gameObjectName;
            textObject.transform.localPosition = localPositionDelta ?? Vector3.zero;
            textObject.transform.localScale = Vector3.one;
            textObject.transform.rotation = Quaternion.identity;
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.enabled = false;

            return text;
        }

        protected void ForceUpdateAllSlots(PlayerControllerB instance)
        {
            var itemSlots = GameNetworkManager.Instance.localPlayerController.ItemSlots;
            for (var i = 0; i < itemSlots.Length; i++)
            {
                if (instance.ItemSlots[i] == null)
                {
                    Hide(i);
                    continue;
                }
                OnUpdate(instance.ItemSlots[i], i);
            }
        }

        protected virtual void UpdateItemSlotText(int index, string text, Color color)
        {
            if (index < 0 || index >= Texts.Count) return;
            var textComponent = Texts[index];
            textComponent.enabled = true;
            textComponent.text = text;
            textComponent.color = color;
        }

        protected virtual void Hide(int currentItemSlotIndex)
        {
            if (currentItemSlotIndex < 0 || currentItemSlotIndex >= Texts.Count) return;
            Texts[currentItemSlotIndex].text = string.Empty;
            Texts[currentItemSlotIndex].enabled = false;
        }

        protected void HideAll(PlayerControllerB instance)
        {
            if (!instance.IsOwner) return;

            var itemSlots = GameNetworkManager.Instance.localPlayerController.ItemSlots;
            for (var itemIndex = 0; itemIndex < itemSlots.Length; itemIndex++)
            {
                Hide(itemIndex);
            }
        }
        #endregion
    }
}
