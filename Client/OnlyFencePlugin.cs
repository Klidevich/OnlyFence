using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using EFT.UI;
using EFT.Trading;
using UnityEngine;

namespace OnlyFenceClient
{
    [BepInPlugin("com.onlyfence.client", "Only Fence Client", "1.0.12")]
    public sealed class OnlyFencePlugin : BaseUnityPlugin
    {
        internal static ConfigEntry<bool> HideDefaultTraders;
        internal static ConfigEntry<bool> HideModdedTraders;
        internal static ConfigEntry<bool> EnableDebugLogging;

        private Harmony _harmony;
        private bool _lastHideDefault;
        private bool _lastHideModded;

        private void Awake()
        {
            HideDefaultTraders = Config.Bind(
                "Trader Visibility",
                "Hide default traders",
                true,
                "Hide original EFT traders except Fence."
            );

            HideModdedTraders = Config.Bind(
                "Trader Visibility",
                "Hide modded traders",
                false,
                "Hide traders added by other mods."
            );

            EnableDebugLogging = Config.Bind(
                "Diagnostics",
                "Enable debug logging",
                false,
                "Log trader card hierarchy when visibility settings change."
            );

            _lastHideDefault = HideDefaultTraders.Value;
            _lastHideModded = HideModdedTraders.Value;

            _harmony = new Harmony("com.onlyfence.client");
            _harmony.PatchAll(typeof(OnlyFencePlugin).Assembly);

            Logger.LogInfo("[OnlyFence] Client 1.0.12 loaded");
        }

        private void Update()
        {
            bool hideDefault = HideDefaultTraders.Value;
            bool hideModded = HideModdedTraders.Value;

            if (hideDefault == _lastHideDefault &&
                hideModded == _lastHideModded)
                return;

            _lastHideDefault = hideDefault;
            _lastHideModded = hideModded;

            OnlyFenceTraderScreensPatch.RefreshVisibleCards();
            if (EnableDebugLogging.Value)
                OnlyFenceDiagnostics.Dump(Logger);
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    [HarmonyPatch(
        typeof(TraderScreensGroup),
        "Show",
        new Type[]
        {
            typeof(TraderScreensGroup.TraderScreenController)
        }
    )]
    public static class OnlyFenceTraderScreensPatch
    {
        private const string FenceId =
            "579dc571d53a0658a154fbec";

        private static readonly HashSet<string> DefaultTraders =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "54cb50c76803fa8b248b4571",
                "54cb57776803fa99248b456e",
                "58330581ace78e27b8b10cee",
                "5935c25fb3acc3127c3d8cd9",
                "5a7c2eca46aef81a7ca2145d",
                "5ac3b934156ae10c4430e83c",
                "5c0647fdd443bc2504c2d371",
                "6617beeaa9cfa777ca915b7c"
            };

        private static readonly FieldInfo TradersListField =
            AccessTools.Field(
                typeof(TraderScreensGroup.TraderScreenController),
                "TradersList"
            );

        private static readonly FieldInfo SelectedTraderField =
            AccessTools.Field(
                typeof(TraderScreensGroup.TraderScreenController),
                "_trader"
            );

        private static readonly FieldInfo SpawnedCardsField =
            AccessTools.Field(
                typeof(TraderScreensGroup),
                "_spawnedCards"
            );

        private static readonly FieldInfo CardTraderField =
            AccessTools.Field(
                typeof(TraderCard),
                "_trader"
            );

        private static readonly Dictionary<
            TraderScreensGroup.TraderScreenController,
            List<Trader>
        > OriginalTraders = new Dictionary<
            TraderScreensGroup.TraderScreenController,
            List<Trader>
        >();

        private static bool ShouldShow(string id)
        {
            if (string.IsNullOrEmpty(id))
                return true;

            if (string.Equals(
                id, FenceId, StringComparison.OrdinalIgnoreCase))
                return true;

            bool isDefault = DefaultTraders.Contains(id);

            if (isDefault &&
                OnlyFencePlugin.HideDefaultTraders.Value)
                return false;

            if (!isDefault &&
                OnlyFencePlugin.HideModdedTraders.Value)
                return false;

            return true;
        }

        private static void Prefix(
            TraderScreensGroup.TraderScreenController controller)
        {
            if (controller == null ||
                TradersListField == null ||
                SelectedTraderField == null)
                return;

            if (!OriginalTraders.TryGetValue(
                controller, out var original))
            {
                var traders =
                    TradersListField.GetValue(controller)
                    as IEnumerable<Trader>;

                if (traders == null)
                    return;

                original = traders.ToList();
                OriginalTraders[controller] = original;
            }

            var visible = original
                .Where(trader =>
                    trader != null &&
                    ShouldShow(trader.Id))
                .ToList();

            if (visible.Count == 0)
                return;

            TradersListField.SetValue(controller, visible);

            var selected =
                SelectedTraderField.GetValue(controller)
                as Trader;

            if (selected == null ||
                !visible.Contains(selected))
            {
                var fence = visible.FirstOrDefault(
                    trader => string.Equals(
                        trader.Id,
                        FenceId,
                        StringComparison.OrdinalIgnoreCase)
                );

                SelectedTraderField.SetValue(
                    controller,
                    fence ?? visible[0]
                );
            }
        }

        public static void RefreshVisibleCards()
        {
            if (SpawnedCardsField == null ||
                CardTraderField == null)
                return;

            var screens =
                Resources.FindObjectsOfTypeAll<TraderScreensGroup>();

            foreach (var screen in screens)
            {
                if (screen == null ||
                    !screen.gameObject.scene.IsValid() ||
                    !screen.gameObject.activeInHierarchy)
                    continue;

                var cards =
                    SpawnedCardsField.GetValue(screen)
                    as IEnumerable<TraderCard>;

                if (cards == null)
                    continue;

                foreach (var card in cards)
                {
                    if (card == null)
                        continue;

                    var info =
                        CardTraderField.GetValue(card)
                        as EFT.Profile.TraderInfo;

                    if (info == null)
                        continue;

                    bool visible = ShouldShow(info.Id);

                    if (card.gameObject.activeSelf != visible)
                        card.gameObject.SetActive(visible);
                }
            }
        }
    }

    internal static class OnlyFenceDiagnostics
    {
        private static readonly System.Reflection.FieldInfo ContainerField =
            HarmonyLib.AccessTools.Field(
                typeof(EFT.UI.TraderScreensGroup),
                "_traderCardsContainer"
            );

        private static readonly System.Reflection.FieldInfo CardsField =
            HarmonyLib.AccessTools.Field(
                typeof(EFT.UI.TraderScreensGroup),
                "_spawnedCards"
            );

        public static void Dump(BepInEx.Logging.ManualLogSource log)
        {
            try
            {
                var screens =
                    UnityEngine.Resources.FindObjectsOfTypeAll<
                        EFT.UI.TraderScreensGroup>();

                foreach (var screen in screens)
                {
                    if (screen == null ||
                        !screen.gameObject.scene.IsValid() ||
                        !screen.gameObject.activeInHierarchy)
                        continue;

                    log.LogInfo("=== ONLY FENCE UI DIAGNOSTICS ===");

                    var container =
                        ContainerField?.GetValue(screen)
                        as UnityEngine.Transform;

                    if (container == null)
                    {
                        log.LogWarning("Container not found");
                        continue;
                    }

                    DumpObject(log, container, "CONTAINER");

                    var cards =
                        CardsField?.GetValue(screen)
                        as System.Collections.Generic.IEnumerable<TraderCard>;

                    if (cards != null)
                    {
                        int index = 0;

                        foreach (var card in cards)
                        {
                            if (card != null)
                            {
                                log.LogInfo(
                                    "CARD #" + index +
                                    " name=" + card.name +
                                    " activeSelf=" + card.gameObject.activeSelf +
                                    " activeHierarchy=" +
                                    card.gameObject.activeInHierarchy
                                );

                                DumpObject(
                                    log,
                                    card.transform,
                                    "CARD"
                                );

                                var parent = card.transform.parent;

                                if (parent != null)
                                    DumpObject(log, parent, "CARD PARENT");
                            }

                            index++;
                        }
                    }

                    log.LogInfo("=== CONTAINER CHILDREN ===");

                    for (int i = 0; i < container.childCount; i++)
                    {
                        var child = container.GetChild(i);
                        DumpObject(log, child, "CHILD #" + i);
                    }

                    log.LogInfo("=== END DIAGNOSTICS ===");
                }
            }
            catch (System.Exception ex)
            {
                log.LogError("[OnlyFence] Diagnostics error: " + ex);
            }
        }

        private static void DumpObject(
            BepInEx.Logging.ManualLogSource log,
            UnityEngine.Transform transform,
            string label)
        {
            if (transform == null)
                return;

            var obj = transform.gameObject;

            log.LogInfo(
                label +
                " name=" + obj.name +
                " activeSelf=" + obj.activeSelf +
                " activeHierarchy=" + obj.activeInHierarchy +
                " parent=" +
                (transform.parent != null
                    ? transform.parent.name
                    : "NULL") +
                " childCount=" + transform.childCount
            );

            foreach (var component in obj.GetComponents<UnityEngine.Component>())
            {
                if (component == null)
                    continue;

                log.LogInfo(
                    "  COMPONENT: " +
                    component.GetType().FullName
                );
            }
        }
    }
}



