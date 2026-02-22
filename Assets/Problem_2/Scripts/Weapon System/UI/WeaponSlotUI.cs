using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LILA_TEST_PROBLEM_2.WeaponSystem.Interfaces;
using LILA_TEST_PROBLEM_2.WeaponSystem.enums;

namespace LILA_TEST_PROBLEM_2.WeaponSystem.UI
{
    /// <summary>
    /// Displays a single weapon slot on the HUD.
    ///
    /// Expected UI hierarchy (create in the Canvas under WeaponHUD):
    ///   WeaponSlotUI (this component lives here)
    ///     ├─ WeaponIcon      [Image]          – weapon sprite
    ///     ├─ AmmoText        [TMP_Text]        – "28 / 90"
    ///     ├─ FiringModeText  [TMP_Text]        – "AUTO" / "BURST" / "SINGLE"
    ///     ├─ ReloadOverlay   [GameObject]      – shown during reload
    ///     └─ ActiveBorder    [Image/Outline]   – highlighted when this slot is active
    /// </summary>
    public class WeaponSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] Image m_weaponIcon;
        [SerializeField] TMP_Text m_ammoText;
        [SerializeField] TMP_Text m_firingModeText;
        [SerializeField] GameObject m_reloadOverlay;
        [SerializeField] GameObject m_activeBorder;
        [SerializeField] GameObject m_emptyOverlay;     // shown when no weapon is equipped

        private IWeapon m_boundWeapon;

        // ── Public API ─────────────────────────────────────────────────────────
        /// <summary>
        /// Bind a new weapon to this slot panel.
        /// Pass null to show the empty state.
        /// </summary>
        public void BindWeapon(IWeapon weapon)
        {
            // Unsubscribe from old weapon
            if (m_boundWeapon != null)
            {
                m_boundWeapon.OnAmmoChanged -= HandleAmmoChanged;
                m_boundWeapon.OnReloadStarted -= HandleReloadStarted;
                m_boundWeapon.OnReloadCompleted -= HandleReloadCompleted;
                m_boundWeapon.OnFiringModeChanged -= HandleFiringModeChanged;
            }

            m_boundWeapon = weapon;

            if (weapon == null)
            {
                ShowEmptyState();
                return;
            }

            // Subscribe to new weapon
            weapon.OnAmmoChanged += HandleAmmoChanged;
            weapon.OnReloadStarted += HandleReloadStarted;
            weapon.OnReloadCompleted += HandleReloadCompleted;
            weapon.OnFiringModeChanged += HandleFiringModeChanged;

            // Initial render
            if (m_weaponIcon != null)
            {
                m_weaponIcon.sprite = weapon.Config.DisplaySprite;
                m_weaponIcon.enabled = weapon.Config.DisplaySprite != null;
            }

            SetEmptyVisible(false);
            UpdateAmmoDisplay(weapon.CurrentAmmo, weapon.ReserveAmmo);
            UpdateFiringModeDisplay(weapon.CurrentFiringMode);
            SetReloadOverlayVisible(weapon.IsReloading);
        }

        /// <summary>Toggle the active highlight border.</summary>
        public void SetActive(bool isActive)
        {
            if (m_activeBorder != null)
                m_activeBorder.SetActive(isActive);
        }

        // ── Event handlers ─────────────────────────────────────────────────────
        private void HandleAmmoChanged(int current, int reserve)
            => UpdateAmmoDisplay(current, reserve);

        private void HandleReloadStarted()
            => SetReloadOverlayVisible(true);

        private void HandleReloadCompleted()
            => SetReloadOverlayVisible(false);

        private void HandleFiringModeChanged(WeaponFiringMode mode)
            => UpdateFiringModeDisplay(mode);

        // ── Display helpers ────────────────────────────────────────────────────
        private void UpdateAmmoDisplay(int current, int reserve)
        {
            if (m_ammoText == null) return;

            if (reserve == -1)                          // Infinite reserve
                m_ammoText.text = $"{current} / ∞";
            else
                m_ammoText.text = $"{current} / {reserve}";

            // Colour cue: red when magazine is empty
            m_ammoText.color = current == 0 ? Color.red : Color.white;
        }

        private void UpdateFiringModeDisplay(WeaponFiringMode mode)
        {
            if (m_firingModeText == null) return;
            m_firingModeText.text = mode switch
            {
                WeaponFiringMode.SingleShot => "SINGLE",
                WeaponFiringMode.BurstFire => "BURST",
                WeaponFiringMode.Automatic => "AUTO",
                _ => mode.ToString().ToUpper()
            };
        }

        private void SetReloadOverlayVisible(bool visible)
        {
            if (m_reloadOverlay != null)
                m_reloadOverlay.SetActive(visible);
        }

        private void SetEmptyVisible(bool visible)
        {
            if (m_emptyOverlay != null)
                m_emptyOverlay.SetActive(visible);
        }

        private void ShowEmptyState()
        {
            if (m_weaponIcon != null) m_weaponIcon.enabled = false;
            if (m_ammoText != null) m_ammoText.text = "-- / --";
            if (m_firingModeText != null) m_firingModeText.text = "";
            SetReloadOverlayVisible(false);
            SetEmptyVisible(true);
            SetActive(false);
        }

        private void OnDestroy()
        {
            // Safety: unsubscribe to prevent leaked delegates after scene unload
            BindWeapon(null);
        }
    }
}