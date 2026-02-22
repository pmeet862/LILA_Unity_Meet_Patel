using System;
using LILA_TEST_PROBLEM_1.Utils;
using UnityEngine;

namespace LILA_TEST_PROBLEM_1
{
    public class DataManager : MonoBehaviour
    {
        [SerializeField] PlayerMovementBroadcaster m_playerMovementBroadcaster;
        [SerializeField] RemotePlayer m_remotePlayer;
        [SerializeField] bool m_useCompression = true;

        private byte[] m_compressedPositionData = new byte[6];
        private byte[] m_fullPositionData = new byte[12];

        private void OnEnable()
        {
            m_playerMovementBroadcaster.OnPlayerMoved += HandlePlayerMoved;
        }

        private void OnDisable()
        {
            m_playerMovementBroadcaster.OnPlayerMoved -= HandlePlayerMoved;
        }

        private void HandlePlayerMoved(Vector3 newPosition)
        {
            if (m_useCompression)
            {
                DataConversionUtils.ConvertVector3ToCompressedByteArrayNonAlloc(newPosition, m_compressedPositionData);
                Debug.Log($"Sent: {newPosition} | Size: {m_compressedPositionData.Length * 8} bits");
                m_remotePlayer.ReceiveData(m_compressedPositionData);
            }
            else
            {
                DataConversionUtils.ConvertVector3ToByteArrayNonAlloc(newPosition, m_fullPositionData);
                Debug.Log($"Sent: {newPosition} | Size: {m_fullPositionData.Length * 8} bits");
                m_remotePlayer.ReceiveData(m_fullPositionData);
            }
        }
    }
}