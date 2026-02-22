using LILA_TEST_PROBLEM_2.WeaponSystem.enums;
using LILA_TEST_PROBLEM_2.WeaponSystem.Interfaces;
using UnityEngine;

namespace LILA_TEST_PROBLEM_2.WeaponSystem.UI
{
    /// <summary>
    /// Root HUD component.  Assign a WeaponSlotUI for each weapon slot
    /// (Primary A, Primary B, Secondary) in the inspector, then hook
    /// this up to a PlayerWeaponController to start receiving updates.
    ///
    /// Hierarchy example:
    ///   Canvas
    ///     └─ WeaponHUD             [WeaponHUD]
    ///          ├─ PrimarySlotA_UI  [WeaponSlotUI]
    ///          ├─ PrimarySlotB_UI  [WeaponSlotUI]
    ///          └─ SecondarySlotUI  [WeaponSlotUI]
    /// </summary>
    public class WeaponHUD : MonoBehaviour
    {
        [Header("Slot UI References")]
        [SerializeField] WeaponSlotUI m_primarySlotA;
        [SerializeField] WeaponSlotUI m_primarySlotB;
        [SerializeField] WeaponSlotUI m_secondarySlotUI;

        private PlayerWeaponController m_controller;

        // ── Initialisation (called by GameManager / Player setup code) ─────────
        public void Bind(PlayerWeaponController controller)
        {
            if (m_controller != null) Unbind();

            m_controller = controller;
            m_controller.OnActiveWeaponChanged += HandleActiveWeaponChanged;
            m_controller.OnWeaponEquipped += HandleWeaponEquipped;
            m_controller.OnWeaponRemoved += HandleWeaponRemoved;

            RefreshAll();
        }

        public void Unbind()
        {
            if (m_controller == null) return;
            m_controller.OnActiveWeaponChanged -= HandleActiveWeaponChanged;
            m_controller.OnWeaponEquipped -= HandleWeaponEquipped;
            m_controller.OnWeaponRemoved -= HandleWeaponRemoved;
            m_controller = null;
        }

        private void OnDestroy() => Unbind();

        // ── Event handlers ─────────────────────────────────────────────────────
        private void HandleActiveWeaponChanged(IWeapon _)
        {
            // Just update active-highlight states
            m_primarySlotA?.SetActive(!m_controller.IsSecondaryActive && m_controller.ActivePrimaryIndex == 0);
            m_primarySlotB?.SetActive(!m_controller.IsSecondaryActive && m_controller.ActivePrimaryIndex == 1);
            m_secondarySlotUI?.SetActive(m_controller.IsSecondaryActive);
        }

        private void HandleWeaponEquipped(IWeapon weapon, WeaponType type, int index)
        {
            var slotUI = GetSlotUI(type, index);
            slotUI?.BindWeapon(weapon);
        }

        private void HandleWeaponRemoved(WeaponType type, int index)
        {
            var slotUI = GetSlotUI(type, index);
            slotUI?.BindWeapon(null);
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private void RefreshAll()
        {
            m_primarySlotA?.BindWeapon(m_controller.GetPrimaryWeapon(0));
            m_primarySlotB?.BindWeapon(m_controller.GetPrimaryWeapon(1));
            m_secondarySlotUI?.BindWeapon(m_controller.GetSecondaryWeapon());
            HandleActiveWeaponChanged(null);
        }

        private WeaponSlotUI GetSlotUI(WeaponType type, int index)
        {
            if (type == WeaponType.Secondary) return m_secondarySlotUI;
            return index == 0 ? m_primarySlotA : m_primarySlotB;
        }
    }
}