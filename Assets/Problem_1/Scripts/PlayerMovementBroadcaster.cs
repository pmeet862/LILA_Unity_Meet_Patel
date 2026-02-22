using UnityEngine;

namespace LILA_TEST_PROBLEM_1
{
    public class PlayerMovementBroadcaster : MonoBehaviour
    {
        public delegate void PlayerMovedEventHandler(Vector3 newPosition);
        public event PlayerMovedEventHandler OnPlayerMoved;
        private Vector3 m_lastPosition;

        private void Start()
        {
            m_lastPosition = transform.position;
        }

        private void Update()
        {
            if (HasMoved())
            {
                //If we were to use world position, remote player would be placed at the position of the player,
                //which is not what we want. We want the remote player to be placed at the position relative to the player,
                //which is why we use local position.
                OnPlayerMoved?.Invoke(transform.localPosition);
                m_lastPosition = transform.position;
            }
        }

        private bool HasMoved()
        {
            // Check if the player has moved significantly since the last position
            return (transform.position - m_lastPosition).sqrMagnitude >= 0.0001;
        }
    }
}