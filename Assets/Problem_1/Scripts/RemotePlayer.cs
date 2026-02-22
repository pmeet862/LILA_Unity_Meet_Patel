using LILA_TEST_PROBLEM_1.Utils;
using UnityEngine;

namespace LILA_TEST_PROBLEM_1
{
    public class RemotePlayer : MonoBehaviour
    {

        [SerializeField] private float m_interpolationSpeed = 15f;

        //If we were to use world position, remote player would be placed at the position of the player,
        //which is not what we want. We want the remote player to be placed at the position relative to the player,
        //which is why we use local position.
        private Vector3 m_targetLocalPosition;

        private void Start()
        {
            m_targetLocalPosition = transform.localPosition;
        }

        public void ReceiveData(byte[] data)
        {
            if (data == null || (data.Length != 6 && data.Length != 12)) return;

            // If the data length is 6, we assume it's a compressed position, otherwise we assume it's a full position
            if (data.Length == 6)
                m_targetLocalPosition = DataConversionUtils.ConvertCompressedByteArrayToVector3(data);
            else
                m_targetLocalPosition = DataConversionUtils.ConvertByteArrayToVector3(data);


            Debug.Log($"Received: {m_targetLocalPosition}");
        }

        private void Update()
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                m_targetLocalPosition,
                Time.deltaTime * m_interpolationSpeed
            );
        }
    }
}