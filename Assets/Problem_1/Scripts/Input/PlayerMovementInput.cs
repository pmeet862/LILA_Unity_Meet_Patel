using LILA_TEST_PROBLEM_1.Input.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LILA_TEST_PROBLEM_1.Input
{
    public class PlayerMovementInput : MonoBehaviour, IMovementInput
    {
        [SerializeField] InputActionReference m_moveActionReference;

        public Vector2 MoveInput { get; private set; }

        private void OnEnable()
        {
            m_moveActionReference.action.Enable();
            m_moveActionReference.action.performed += OnMovePerformed;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        private void OnDisable()
        {
            m_moveActionReference.action.performed -= OnMovePerformed;
            m_moveActionReference.action.Disable();
        }
    }
}