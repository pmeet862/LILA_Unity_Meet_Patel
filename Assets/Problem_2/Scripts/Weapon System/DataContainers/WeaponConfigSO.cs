using UnityEngine;
using System.Collections.Generic;
using LILA_TEST_PROBLEM_2.WeaponSystem.enums;

namespace LILA_TEST_PROBLEM_2.WeaponSystem.DataContainers
{
    /// <summary>
    /// Data-only ScriptableObject that fully describes a weapon's behaviour.
    /// Shared across all instances of the same weapon type.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponConfigSO", menuName = "LILA_TEST_PROBLEM_2/Weapon System/Weapon Config")]
    public class WeaponConfigSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string m_name;
        public string Name => m_name;

        [SerializeField] WeaponType m_type;
        public WeaponType Type => m_type;

        [SerializeField] Sprite m_displaySprite;
        public Sprite DisplaySprite => m_displaySprite;

        // ── Damage ─────────────────────────────────────────────────────────────
        [Header("Damage")]
        [SerializeField] float m_weaponDamage;
        public float WeaponDamage => m_weaponDamage;

        // ── Ammo ───────────────────────────────────────────────────────────────
        [Header("Ammo")]
        [SerializeField] int m_magazineSize;
        public int MagazineSize => m_magazineSize;

        [Tooltip("Total rounds the player can carry outside the magazine. -1 = infinite.")]
        [SerializeField] int m_maxReserveAmmo = 90;
        public int MaxReserveAmmo => m_maxReserveAmmo;

        [SerializeField] float m_reloadTime = 2f;
        public float ReloadTime => m_reloadTime;

        // ── Fire Rate ──────────────────────────────────────────────────────────
        [Header("Fire Rate")]
        [Tooltip("Rounds per minute.")]
        [SerializeField] float m_fireRate = 600f;
        public float FireRate => m_fireRate;

        /// <summary>Seconds between shots derived from FireRate.</summary>
        public float FireInterval => 60f / m_fireRate;

        // ── Burst Fire ─────────────────────────────────────────────────────────
        [Header("Burst Settings")]
        [Tooltip("Number of rounds fired per burst trigger pull.")]
        [SerializeField] int m_burstCount = 3;
        public int BurstCount => m_burstCount;

        [Tooltip("Delay in seconds between shots within a burst.")]
        [SerializeField] float m_burstDelay = 0.08f;
        public float BurstDelay => m_burstDelay;

        // ── Firing Modes ───────────────────────────────────────────────────────
        [Header("Firing Modes")]
        [SerializeField] WeaponFiringMode m_firingMode;
        public WeaponFiringMode FiringMode => m_firingMode;

        [Tooltip("All modes the player may cycle through. Must include the default FiringMode.")]
        [SerializeField] WeaponFiringMode[] m_supportedFiringModes = { WeaponFiringMode.SingleShot };
        public IReadOnlyList<WeaponFiringMode> SupportedFiringModes => m_supportedFiringModes;

        // ── Projectile ─────────────────────────────────────────────────────────
        [Header("Projectile")]
        [Tooltip("Optional prefab spawned when the weapon fires. Leave null for hitscan.")]
        [SerializeField] GameObject m_projectilePrefab;
        public GameObject ProjectilePrefab => m_projectilePrefab;

        [Tooltip("Speed of the projectile in units/s. Ignored for hitscan weapons.")]
        [SerializeField] float m_projectileSpeed = 50f;
        public float ProjectileSpeed => m_projectileSpeed;

        [Tooltip("Maximum range for hitscan weapons in units.")]
        [SerializeField] float m_range = 100f;
        public float Range => m_range;

#if UNITY_EDITOR
        private void OnValidate()
        {
            m_magazineSize = Mathf.Max(1, m_magazineSize);
            m_maxReserveAmmo = Mathf.Max(-1, m_maxReserveAmmo);
            m_fireRate = Mathf.Max(1f, m_fireRate);
            m_burstCount = Mathf.Max(1, m_burstCount);

            // Ensure default mode is in supported list
            bool found = false;
            foreach (var mode in m_supportedFiringModes)
                if (mode == m_firingMode) { found = true; break; }

            if (!found)
                Debug.LogWarning($"[WeaponConfigSO] {m_name}: default FiringMode '{m_firingMode}' is not listed in SupportedFiringModes.");
        }
#endif
    }
}