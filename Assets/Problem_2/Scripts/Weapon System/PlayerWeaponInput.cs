using LILA_TEST_PROBLEM_2.WeaponSystem.enums;
using UnityEngine;

namespace LILA_TEST_PROBLEM_2.WeaponSystem
{
    /// <summary>
    /// Reads player input each frame and delegates to PlayerWeaponController.
    /// Keeping input separate from weapon logic makes it easy to swap in a
    /// new Input System or rebind keys without touching weapon code.
    ///
    /// Default bindings (old Input Manager):
    ///   Left Mouse    – fire
    ///   R             – reload
    ///   V             – cycle firing mode
    ///   1             – primary A
    ///   2             – primary B
    ///   3 / Q         – secondary
    ///   Mouse scroll  – cycle primaries
    /// </summary>
    [RequireComponent(typeof(PlayerWeaponController))]
    public class PlayerWeaponInput : MonoBehaviour
    {
        private PlayerWeaponController m_controller;

        private void Awake()
        {
            m_controller = GetComponent<PlayerWeaponController>();
        }

        private void Update()
        {
            HandleFire();
            HandleReload();
            HandleCycleFiringMode();
            HandleWeaponSwitch();
        }

        private void HandleFire()
        {
            // Automatic: hold mouse; Single/Burst: GetButtonDown prevents auto-spamming
            if (m_controller.ActiveWeapon == null) return;

            bool firingModeIsAuto = m_controller.ActiveWeapon.CurrentFiringMode ==
                                    WeaponFiringMode.Automatic;

            bool triggerHeld = firingModeIsAuto
                ? Input.GetMouseButton(0)
                : Input.GetMouseButtonDown(0);

            if (triggerHeld)
                m_controller.FireActiveWeapon();
        }

        private void HandleReload()
        {
            if (Input.GetKeyDown(KeyCode.R))
                m_controller.ReloadActiveWeapon();
        }

        private void HandleCycleFiringMode()
        {
            if (Input.GetKeyDown(KeyCode.V))
                m_controller.CycleActiveFiringMode();
        }

        private void HandleWeaponSwitch()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) m_controller.SwitchToPrimary(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) m_controller.SwitchToPrimary(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Q))
                m_controller.SwitchToSecondary();

            // Mouse scroll cycles primary weapons
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f) m_controller.SwitchPrimary();
        }
    }
}