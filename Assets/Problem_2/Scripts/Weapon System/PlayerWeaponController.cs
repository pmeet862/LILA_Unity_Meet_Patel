using System;
using UnityEngine;
using LILA_TEST_PROBLEM_2.WeaponSystem.enums;
using LILA_TEST_PROBLEM_2.WeaponSystem.Interfaces;

namespace LILA_TEST_PROBLEM_2.WeaponSystem
{
    /// <summary>
    /// Sits on the Player GameObject.
    ///
    /// Slot layout
    ///   PrimarySlots[0]  – Primary weapon A  (key 1 / gamepad LB)
    ///   PrimarySlots[1]  – Primary weapon B  (key 2)
    ///   SecondarySlot    – Sidearm            (key 3 / gamepad RB)
    ///
    /// The controller owns all weapon state transitions (equip, drop, switch)
    /// and relays fire/reload/cycle commands to the active weapon.
    /// It also broadcasts events that the HUD and other systems subscribe to.
    /// </summary>
    public class PlayerWeaponController : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────────
        [Header("Slot References (assign in prefab)")]
        [SerializeField] WeaponSlot[] m_primarySlots = new WeaponSlot[2];
        [SerializeField] WeaponSlot m_secondarySlot;

        [Header("Fire Point")]
        [SerializeField] Transform m_firePoint;

        // ── Events ─────────────────────────────────────────────────────────────
        /// <summary>Raised whenever the active weapon changes. Arg: new active weapon (may be null).</summary>
        public event Action<IWeapon> OnActiveWeaponChanged;

        /// <summary>Raised when a weapon is equipped into any slot.</summary>
        public event Action<IWeapon, WeaponType, int> OnWeaponEquipped;   // weapon, type, slot index

        /// <summary>Raised when a weapon is removed from any slot.</summary>
        public event Action<WeaponType, int> OnWeaponRemoved;

        // ── State ──────────────────────────────────────────────────────────────
        private IWeapon m_activeWeapon;
        private WeaponSlot m_activeSlot;
        private int m_activePrimaryIndex = 0;
        private bool m_isSecondaryActive = false;

        public IWeapon ActiveWeapon => m_activeWeapon;
        public bool IsSecondaryActive => m_isSecondaryActive;
        public int ActivePrimaryIndex => m_activePrimaryIndex;

        // ── Unity lifecycle ────────────────────────────────────────────────────
        private void Start()
        {
            // Activate first available primary on start
            if (m_primarySlots[0] != null && m_primarySlots[0].HasWeapon)
                ActivateSlot(m_primarySlots[0], 0, false);
            else if (m_primarySlots[1] != null && m_primarySlots[1].HasWeapon)
                ActivateSlot(m_primarySlots[1], 1, false);
            else if (m_secondarySlot != null && m_secondarySlot.HasWeapon)
                ActivateSlot(m_secondarySlot, 0, true);
        }

        // ── Public API: Weapon Management ──────────────────────────────────────
        /// <summary>
        /// Equips a weapon into the appropriate slot.
        /// For primary weapons you can specify a preferred slot index (0 or 1).
        /// Returns true on success.
        /// </summary>
        public bool EquipWeapon(IWeapon weapon, int preferredPrimaryIndex = 0)
        {
            if (weapon == null) return false;

            if (weapon.Config.Type == WeaponType.Primary)
            {
                int index = GetAvailablePrimaryIndex(preferredPrimaryIndex);
                if (index < 0)
                {
                    Debug.LogWarning("[PlayerWeaponController] Both primary slots are occupied.");
                    return false;
                }
                return EquipIntoPrimarySlot(weapon, index);
            }
            else
            {
                return EquipIntoSecondarySlot(weapon);
            }
        }

        /// <summary>Removes the weapon from the given primary slot without destroying it.</summary>
        public IWeapon DropPrimary(int index)
        {
            if (!IsValidPrimaryIndex(index)) return null;
            var slot = m_primarySlots[index];
            if (!slot.HasWeapon) return null;

            var dropped = slot.RemoveWeapon();
            OnWeaponRemoved?.Invoke(WeaponType.Primary, index);

            if (m_activeSlot == slot)
                TryActivateFallback();

            return dropped;
        }

        /// <summary>Removes the secondary weapon without destroying it.</summary>
        public IWeapon DropSecondary()
        {
            if (!m_secondarySlot.HasWeapon) return null;

            var dropped = m_secondarySlot.RemoveWeapon();
            OnWeaponRemoved?.Invoke(WeaponType.Secondary, 0);

            if (m_isSecondaryActive)
                TryActivateFallback();

            return dropped;
        }

        // ── Public API: Weapon Commands ────────────────────────────────────────
        /// <summary>Fires the active weapon. Call this every frame while the trigger is held.</summary>
        public void FireActiveWeapon()
        {
            if (m_activeWeapon == null || m_firePoint == null) return;
            m_activeWeapon.Fire(m_firePoint);
        }

        public void ReloadActiveWeapon()
        {
            m_activeWeapon?.Reload();
        }

        public void CycleActiveFiringMode()
        {
            m_activeWeapon?.CycleFiringMode();
        }

        /// <summary>Switch to primary slot 0 or 1.</summary>
        public void SwitchToPrimary(int index)
        {
            if (!IsValidPrimaryIndex(index)) return;
            if (!m_primarySlots[index].HasWeapon) return;
            if (!m_isSecondaryActive && m_activePrimaryIndex == index) return;

            ActivateSlot(m_primarySlots[index], index, false);
        }

        /// <summary>Cycle between primary weapon A and B.</summary>
        public void SwitchPrimary()
        {
            int next = m_isSecondaryActive ? m_activePrimaryIndex : (m_activePrimaryIndex + 1) % 2;
            SwitchToPrimary(next);
        }

        public void SwitchToSecondary()
        {
            if (m_secondarySlot == null || !m_secondarySlot.HasWeapon) return;
            if (m_isSecondaryActive) return;
            ActivateSlot(m_secondarySlot, 0, true);
        }

        // ── Internals ──────────────────────────────────────────────────────────
        private bool EquipIntoPrimarySlot(IWeapon weapon, int index)
        {
            var slot = m_primarySlots[index];
            if (!slot.SetWeapon(weapon)) return false;

            OnWeaponEquipped?.Invoke(weapon, WeaponType.Primary, index);

            // Auto-activate if no primary is currently active
            if (m_activeWeapon == null || (m_isSecondaryActive == false && m_activePrimaryIndex == index))
                ActivateSlot(slot, index, false);

            return true;
        }

        private bool EquipIntoSecondarySlot(IWeapon weapon)
        {
            if (!m_secondarySlot.SetWeapon(weapon)) return false;

            OnWeaponEquipped?.Invoke(weapon, WeaponType.Secondary, 0);

            if (m_activeWeapon == null)
                ActivateSlot(m_secondarySlot, 0, true);

            return true;
        }

        private void ActivateSlot(WeaponSlot slot, int primaryIndex, bool isSecondary)
        {
            // Hide current weapon
            SetWeaponVisible(m_activeWeapon, false);

            m_activeSlot = slot;
            m_activeWeapon = slot.Weapon;
            m_isSecondaryActive = isSecondary;
            if (!isSecondary) m_activePrimaryIndex = primaryIndex;

            // Show new weapon
            SetWeaponVisible(m_activeWeapon, true);

            OnActiveWeaponChanged?.Invoke(m_activeWeapon);
        }

        private void TryActivateFallback()
        {
            // Try other primary → then secondary → then null
            for (int i = 0; i < m_primarySlots.Length; i++)
            {
                if (m_primarySlots[i] != null && m_primarySlots[i].HasWeapon)
                {
                    ActivateSlot(m_primarySlots[i], i, false);
                    return;
                }
            }

            if (m_secondarySlot != null && m_secondarySlot.HasWeapon)
            {
                ActivateSlot(m_secondarySlot, 0, true);
                return;
            }

            // No weapon left
            m_activeWeapon = null;
            m_activeSlot = null;
            OnActiveWeaponChanged?.Invoke(null);
        }

        private int GetAvailablePrimaryIndex(int preferred)
        {
            if (IsValidPrimaryIndex(preferred) && !m_primarySlots[preferred].HasWeapon)
                return preferred;

            for (int i = 0; i < m_primarySlots.Length; i++)
                if (!m_primarySlots[i].HasWeapon) return i;

            return -1;
        }

        private static bool IsValidPrimaryIndex(int i) => i >= 0 && i <= 1;

        private static void SetWeaponVisible(IWeapon weapon, bool visible)
        {
            if (weapon is MonoBehaviour mb)
                mb.gameObject.SetActive(visible);
        }

        // ── Slot accessors (read-only, for HUD/other systems) ──────────────────
        public IWeapon GetPrimaryWeapon(int index) =>
            IsValidPrimaryIndex(index) && m_primarySlots[index].HasWeapon
                ? m_primarySlots[index].Weapon : null;

        public IWeapon GetSecondaryWeapon() =>
            m_secondarySlot != null && m_secondarySlot.HasWeapon
                ? m_secondarySlot.Weapon : null;
    }
}