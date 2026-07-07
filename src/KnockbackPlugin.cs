using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnockbackMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class KnockbackPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.uchknockback.mod";
        public const string PluginName = "UCH Knockback Mod";
        public const string PluginVersion = "1.1.0";

        internal static new ManualLogSource Logger;

        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<float> KnockbackForce { get; private set; }
        public static ConfigEntry<float> KnockbackRadius { get; private set; }
        public static ConfigEntry<float> UpwardBoost { get; private set; }
        public static ConfigEntry<float> Cooldown { get; private set; }
        public static ConfigEntry<KeyCode> KnockbackKey { get; private set; }

        private float cooldownRemaining;

        private void Awake()
        {
            Logger = base.Logger;

            EnableMod = Config.Bind("General", "EnableMod", true,
                "Enable/Disable the knockback mod");
            KnockbackForce = Config.Bind("Knockback", "Force", 15f,
                new ConfigDescription("Strength of the knockback impulse (the game's springs use ~10-20)",
                    new AcceptableValueRange<float>(0f, 100f)));
            KnockbackRadius = Config.Bind("Knockback", "Radius", 3f,
                new ConfigDescription("Radius (world units) around your character that is affected",
                    new AcceptableValueRange<float>(0.5f, 20f)));
            UpwardBoost = Config.Bind("Knockback", "UpwardBoost", 0.35f,
                new ConfigDescription("Extra upward push mixed into the knockback direction so grounded characters lift off",
                    new AcceptableValueRange<float>(0f, 1f)));
            Cooldown = Config.Bind("Knockback", "Cooldown", 0.5f,
                new ConfigDescription("Seconds between knockback triggers",
                    new AcceptableValueRange<float>(0f, 10f)));
            KnockbackKey = Config.Bind("Controls", "KnockbackKey", KeyCode.Mouse0,
                "Key to trigger knockback (Mouse0 = Left Click)");

            Logger.LogInfo($"{PluginName} v{PluginVersion} loaded");
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= Time.unscaledDeltaTime;
            }

            if (!EnableMod.Value || cooldownRemaining > 0f)
            {
                return;
            }

            if (Input.GetKeyDown(KnockbackKey.Value))
            {
                cooldownRemaining = Cooldown.Value;
                TriggerKnockback();
            }
        }

        private void TriggerKnockback()
        {
            try
            {
                List<Character> characters = Character.AllCharacters;
                if (characters == null || characters.Count == 0)
                {
                    return;
                }

                Character source = FindLocalCharacter(characters);
                if (source == null)
                {
                    Logger.LogDebug("Knockback pressed but no local character is alive (not in a round?)");
                    return;
                }

                Vector2 origin = source.transform.position;
                int affected = 0;

                foreach (Character target in characters)
                {
                    if (target == null || target == source || target.Dead || target.Dying)
                    {
                        continue;
                    }

                    Vector2 targetPos = target.transform.position;
                    float distance = Vector2.Distance(origin, targetPos);
                    if (distance > KnockbackRadius.Value)
                    {
                        continue;
                    }

                    // Push away from the source; straight up if the two overlap.
                    Vector2 direction = distance > 0.05f
                        ? (targetPos - origin) / distance
                        : Vector2.up;
                    direction = (direction + Vector2.up * UpwardBoost.Value).normalized;

                    if (target.AddImpulse(direction * KnockbackForce.Value, 0.1f))
                    {
                        affected++;
                    }
                }

                if (affected > 0)
                {
                    Logger.LogInfo($"Knocked back {affected} character(s) around player {source.PlayerNumber}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error while applying knockback: {ex}");
            }
        }

        private static Character FindLocalCharacter(List<Character> characters)
        {
            Character best = null;
            foreach (Character character in characters)
            {
                if (character == null || character.LocalPlayer == null || character.Dead || character.Dying)
                {
                    continue;
                }
                if (best == null || character.PlayerNumber < best.PlayerNumber)
                {
                    best = character;
                }
            }
            return best;
        }
    }
}
