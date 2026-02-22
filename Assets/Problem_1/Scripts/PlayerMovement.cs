using LILA_TEST_PROBLEM_1.Input.Interfaces;
using UnityEngine;

namespace LILA_TEST_PROBLEM_1
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float m_moveSpeed = 5f;
        [SerializeField] float m_rotationSpeed = 10f;

        private Rigidbody m_selfRB;
        private IMovementInput m_movementInput;
        private Vector3 m_moveDirection;

        private void Awake()
        {
            m_selfRB = GetComponent<Rigidbody>();
            m_movementInput = GetComponent<IMovementInput>();
        }

        private void FixedUpdate()
        {
            if (m_movementInput == null) return;
            SetMovementDirection();
            HandleMovement();
            HandleRotation();
        }

        private void SetMovementDirection()
        {
            m_moveDirection = new Vector3(m_movementInput.MoveInput.x, 0f, m_movementInput.MoveInput.y);
            if (m_moveDirection.sqrMagnitude > 1f) m_moveDirection.Normalize();
        }

        private void HandleMovement()
        {

            Vector3 currentVelocity = m_selfRB.linearVelocity;

            float verticalSpeed = Vector3.Dot(currentVelocity, transform.up);
            Vector3 gravityVelocity = transform.up * verticalSpeed;

            Vector3 targetMoveVelocity = m_moveDirection * m_moveSpeed;

            m_selfRB.linearVelocity = targetMoveVelocity + gravityVelocity;
        }

        private void HandleRotation()
        {
            if (m_moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(m_moveDirection, transform.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * m_rotationSpeed);
            }
        }
    }
}