using QualityCompany.Service;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static QualityCompany.Events.GameEvents;

namespace QualityCompany.Modules.Core
{
    internal class ModuleLoader : MonoBehaviour
    {
        private readonly ModLogger Logger = new(nameof(ModuleLoader));

        private void Start()
        {
            transform.position = Vector3.zero;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            StartCoroutine(LoadModulesCoroutine());

            // Subscribe to Disconnected event
            Disconnected += DetachAllModules;
        }

        private void OnDestroy()
        {
            // Unsubscribe from Disconnected event to avoid memory leaks
            Disconnected -= DetachAllModules;
        }

        private IEnumerator LoadModulesCoroutine()
        {
            // Check if Plugin instance and PluginConfig are initialized
            if (Plugin.Instance?.PluginConfig == null)
            {
                Logger.TryLogDebug("Plugin configuration is missing; skipping module load.");
                yield break;
            }

            float delay = Mathf.Max(3.0f, Plugin.Instance.PluginConfig.InventoryStartupDelay);
            Logger.TryLogDebug($"Loading up modules with a {delay} seconds delay...");

            // Load non-delayed modules
            foreach (var internalModule in ModuleRegistry.Modules?.Where(x => !x.DelayedStart) ?? Enumerable.Empty<InternalModule>())
            {
                Logger.TryLogDebug($"Starting up {internalModule.Name}");
                var instance = internalModule.OnLoad?.Invoke(null, null);
                if (instance == null) continue;

                internalModule.Instance = instance;
                internalModule.OnAttach?.Invoke(instance, null);
            }

            yield return new WaitForSeconds(delay);

            // Load delayed modules
            foreach (var internalModule in ModuleRegistry.Modules?.Where(x => x.DelayedStart) ?? Enumerable.Empty<InternalModule>())
            {
                Logger.TryLogDebug($"Starting up {internalModule.Name}");
                var instance = internalModule.OnLoad?.Invoke(null, null);
                if (instance == null) continue;

                internalModule.Instance = instance;
                internalModule.OnAttach?.Invoke(instance, null);
            }

            Logger.TryLogDebug("Internal modules loaded!");
        }

        private void DetachAllModules(GameNetworkManager _)
        {
            Logger.TryLogDebug("Detaching all modules...");

            foreach (var internalModule in ModuleRegistry.Modules ?? Enumerable.Empty<InternalModule>())
            {
                if (internalModule.Instance == null) continue;

                Logger.TryLogDebug($"Detaching {internalModule.Name}");
                internalModule.OnDetach?.Invoke(internalModule.Instance, null);
                internalModule.Instance = null;
            }
        }
    }
}
