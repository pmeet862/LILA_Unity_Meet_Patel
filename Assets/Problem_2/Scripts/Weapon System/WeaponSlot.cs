using UnityEngine;
using LILA_TEST_PROBLEM_2.WeaponSystem.enums;
using LILA_TEST_PROBLEM_2.WeaponSystem.Interfaces;

namespace LILA_TEST_PROBLEM_2.WeaponSystem
{
    /// <summary>
    /// A named attachment point that accepts weapons of a specific WeaponType.
    /// The Transform of this GameObject acts as the weapon's parent in the scene hierarchy.
    /// </summary>
    public class WeaponSlot : MonoBehaviour
    {
        [SerializeField] WeaponType m_weaponType;

        private IWeapon m_weapon;

        public WeaponType WeaponType => m_weaponType;
        public bool HasWeapon => m_weapon != null;
        public IWeapon Weapon => m_weapon;

        /// <summary>
        /// Assigns a weapon to this slot. Pass null to clear the slot.
        /// Returns false if the weapon's type does not match this slot's type.
        /// </summary>
        public bool SetWeapon(IWeapon weapon)
        {
            if (weapon != null && weapon.Config.Type != m_weaponType)
            {
                Debug.LogWarning($"[WeaponSlot] Cannot place a {weapon.Config.Type} weapon in a {m_weaponType} slot.");
                return false;
            }

            // Re-parent the physical weapon object to this slot transform
            if (weapon is MonoBehaviour mb)
            {
                mb.transform.SetParent(transform, false);
                mb.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }

            m_weapon = weapon;
            return true;
        }

        /// <summary>Removes and returns the current weapon without destroying it.</summary>
        public IWeapon RemoveWeapon()
        {
            var w = m_weapon;
            if (w is MonoBehaviour mb)
                mb.transform.SetParent(null);
            m_weapon = null;
            return w;
        }
    }
}