using System;
using System.Collections;
using UnityEngine;
using LILA_TEST_PROBLEM_2.WeaponSystem.DataContainers;
using LILA_TEST_PROBLEM_2.WeaponSystem.enums;
using LILA_TEST_PROBLEM_2.WeaponSystem.Interfaces;

namespace LILA_TEST_PROBLEM_2.Weapons
{
    /// <summary>
    /// Concrete base implementation of IWeapon.
    /// Handles single-shot, burst, and automatic fire; reload coroutine; firing-mode cycling.
    ///
    /// Override <see cref="SpawnProjectile"/> to customise what happens each time a bullet
    /// is actually fired (hitscan raycast, physics projectile, VFX, audio, etc.).
    /// All state management stays here so sub-classes stay lean.
    /// </summary>
    public class BaseWeapon : MonoBehaviour, IWeapon
    {
        // ── Inspector ──────────────────────────────────────────────────────────
        [SerializeField] WeaponConfigSO m_config;

        // ── IWeapon properties ─────────────────────────────────────────────────
        public WeaponConfigSO Config => m_config;
        public int CurrentAmmo => m_currentAmmo;
        public int ReserveAmmo => m_reserveAmmo;
        public bool IsReloading => m_isReloading;
        public bool IsFiring => m_isFiring;
        public WeaponFiringMode CurrentFiringMode => m_currentFiringMode;

        // ── IWeapon events ─────────────────────────────────────────────────────
        public event Action<int, int> OnAmmoChanged;
        public event Action OnReloadStarted;
        public event Action OnReloadCompleted;
        public event Action<WeaponFiringMode> OnFiringModeChanged;

        // ── Private state ──────────────────────────────────────────────────────
        private int m_currentAmmo;
        private int m_reserveAmmo;
        private bool m_isReloading;
        private bool m_isFiring;
        private float m_nextFireTime;
        private WeaponFiringMode m_currentFiringMode;
        private int m_firingModeIndex;
        private Coroutine m_reloadCoroutine;
        private Coroutine m_burstCoroutine;

        // ── Unity lifecycle ────────────────────────────────────────────────────
        protected virtual void Awake()
        {
            if (m_config == null)
            {
                Debug.LogError($"[BaseWeapon] {name} is missing a WeaponConfigSO. Disabling.");
                gameObject.SetActive(false);
                return;
            }

            m_currentAmmo = m_config.MagazineSize;
            m_reserveAmmo = m_config.MaxReserveAmmo;
            m_currentFiringMode = m_config.FiringMode;

            // Sync the index so CycleFiringMode starts from the correct position
            var modes = m_config.SupportedFiringModes;
            m_firingModeIndex = 0;
            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i] == m_currentFiringMode) { m_firingModeIndex = i; break; }
            }
        }

        // ── IWeapon: Fire ──────────────────────────────────────────────────────
        public virtual void Fire(Transform firePoint)
        {
            if (!CanFire()) return;

            switch (m_currentFiringMode)
            {
                case WeaponFiringMode.SingleShot:
                    ExecuteSingleShot(firePoint);
                    break;

                case WeaponFiringMode.Automatic:
                    ExecuteSingleShot(firePoint);   // Caller is responsible for holding trigger
                    break;

                case WeaponFiringMode.BurstFire:
                    if (!m_isFiring)
                        m_burstCoroutine = StartCoroutine(ExecuteBurst(firePoint));
                    break;
            }
        }

        // ── IWeapon: Reload ────────────────────────────────────────────────────
        public virtual void Reload()
        {
            if (m_isReloading) return;
            if (m_currentAmmo == m_config.MagazineSize) return;
            if (m_reserveAmmo == 0 && m_config.MaxReserveAmmo != -1) return;

            m_reloadCoroutine = StartCoroutine(ReloadRoutine());
        }

        // ── IWeapon: CycleFiringMode ───────────────────────────────────────────
        public virtual void CycleFiringMode()
        {
            if (m_isReloading || m_isFiring) return;

            var modes = m_config.SupportedFiringModes;
            if (modes.Count <= 1) return;

            m_firingModeIndex = (m_firingModeIndex + 1) % modes.Count;
            m_currentFiringMode = modes[m_firingModeIndex];
            OnFiringModeChanged?.Invoke(m_currentFiringMode);
        }

        // ── IWeapon: CanFire ───────────────────────────────────────────────────
        public virtual bool CanFire()
        {
            return !m_isReloading
                && !m_isFiring
                && m_currentAmmo > 0
                && Time.time >= m_nextFireTime;
        }

        // ── Internal fire helpers ──────────────────────────────────────────────
        private void ExecuteSingleShot(Transform firePoint)
        {
            ConsumeAmmo(1);
            m_nextFireTime = Time.time + m_config.FireInterval;
            SpawnProjectile(firePoint);

            if (m_currentAmmo == 0)
                Reload();
        }

        private IEnumerator ExecuteBurst(Transform firePoint)
        {
            m_isFiring = true;
            int shots = Mathf.Min(m_config.BurstCount, m_currentAmmo);

            for (int i = 0; i < shots; i++)
            {
                ConsumeAmmo(1);
                SpawnProjectile(firePoint);

                if (i < shots - 1)
                    yield return new WaitForSeconds(m_config.BurstDelay);
            }

            m_nextFireTime = Time.time + m_config.FireInterval;
            m_isFiring = false;

            if (m_currentAmmo == 0)
                Reload();
        }

        private IEnumerator ReloadRoutine()
        {
            m_isReloading = true;
            OnReloadStarted?.Invoke();

            yield return new WaitForSeconds(m_config.ReloadTime);

            int needed = m_config.MagazineSize - m_currentAmmo;

            if (m_config.MaxReserveAmmo == -1) // Infinite reserve
            {
                m_currentAmmo = m_config.MagazineSize;
            }
            else
            {
                int toLoad = Mathf.Min(needed, m_reserveAmmo);
                m_currentAmmo += toLoad;
                m_reserveAmmo -= toLoad;
            }

            m_isReloading = false;
            OnAmmoChanged?.Invoke(m_currentAmmo, m_reserveAmmo);
            OnReloadCompleted?.Invoke();
        }

        private void ConsumeAmmo(int amount)
        {
            m_currentAmmo = Mathf.Max(0, m_currentAmmo - amount);
            OnAmmoChanged?.Invoke(m_currentAmmo, m_reserveAmmo);
        }

        /// <summary>
        /// Override in a sub-class to add hitscan/projectile logic, audio, VFX etc.
        /// Called once per bullet that leaves the barrel.
        /// </summary>
        protected virtual void SpawnProjectile(Transform firePoint)
        {
            if (m_config.ProjectilePrefab != null)
            {
                var go = Instantiate(m_config.ProjectilePrefab, firePoint.position, firePoint.rotation);
                if (go.TryGetComponent<Rigidbody>(out var rb))
                    rb.linearVelocity = firePoint.forward * m_config.ProjectileSpeed;
            }
            else
            {
                // Default hitscan
                if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, m_config.Range))
                    OnHitscanHit(hit);
            }
        }

        /// <summary>
        /// Override to apply damage, spawn decals, play impact audio, etc.
        /// Only called when no ProjectilePrefab is assigned (hitscan mode).
        /// </summary>
        protected virtual void OnHitscanHit(RaycastHit hit)
        {
            Debug.Log($"[BaseWeapon] Hitscan hit: {hit.collider.name} for {m_config.WeaponDamage} damage.");
        }

        // ── Cleanup ────────────────────────────────────────────────────────────
        private void OnDisable()
        {
            if (m_reloadCoroutine != null) StopCoroutine(m_reloadCoroutine);
            if (m_burstCoroutine != null) StopCoroutine(m_burstCoroutine);
            m_isReloading = false;
            m_isFiring = false;
        }
    }
}