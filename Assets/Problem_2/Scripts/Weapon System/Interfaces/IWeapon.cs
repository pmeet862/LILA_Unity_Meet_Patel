using System;
using UnityEngine;
using LILA_TEST_PROBLEM_2.WeaponSystem.DataContainers;
using LILA_TEST_PROBLEM_2.WeaponSystem.enums;

namespace LILA_TEST_PROBLEM_2.WeaponSystem.Interfaces
{
    /// <summary>
    /// Core weapon contract. All weapons must implement this interface.
    /// Events are used for decoupled communication with the HUD and other systems.
    /// </summary>
    public interface IWeapon
    {
        // ── State ──────────────────────────────────────────────────────────────
        WeaponConfigSO Config { get; }
        int CurrentAmmo { get; }        // Rounds left in the current magazine
        int ReserveAmmo { get; }        // Rounds left in reserve (outside magazine)
        bool IsReloading { get; }
        bool IsFiring { get; }          // True while a burst is in progress
        WeaponFiringMode CurrentFiringMode { get; }

        // ── Events ─────────────────────────────────────────────────────────────
        /// <summary>Fires whenever magazine or reserve ammo changes. Args: (currentAmmo, reserveAmmo)</summary>
        event Action<int, int> OnAmmoChanged;

        /// <summary>Fires when a reload coroutine begins.</summary>
        event Action OnReloadStarted;

        /// <summary>Fires when a reload coroutine completes successfully.</summary>
        event Action OnReloadCompleted;

        /// <summary>Fires when the active firing mode changes.</summary>
        event Action<WeaponFiringMode> OnFiringModeChanged;

        // ── Commands ───────────────────────────────────────────────────────────
        void Fire(Transform firePoint);
        void Reload();
        void CycleFiringMode();

        // ── Queries ────────────────────────────────────────────────────────────
        bool CanFire();
    }
}