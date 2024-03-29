﻿using UnityEngine;
using Unity.Netcode;
using System;

namespace FunkySheep.NetWind
{
    public abstract class RewindableInputBehaviour<T> : NetworkBehaviour, IRewindableInput
    {
        [Header("NetWind")]
        [SerializeField] private TickHistoryBuffer<T> inputBuffer;
        [SerializeField] private int earliestReceivedInput;
        [SerializeField] private bool hasNewInput = false;

        public ulong NetId => NetworkObjectId;
        public bool IsOwn => IsOwner || (IsOwnedByServer && IsServer);

        public int EarliestReceivedInput => earliestReceivedInput;

        public bool HasNewInput => hasNewInput;

        public int LatestKnownInput => inputBuffer.GetLatestKnownFrame();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            inputBuffer = new TickHistoryBuffer<T>(NetworkRewindManager.Instance.HistorySize, NetworkManager.LocalTime.Tick);
        }

        protected abstract T CaptureInput();
        protected abstract void ApplyInput(T input);

        public void RestoreInput(int tick)
        {
            try
            {
              ApplyInput(inputBuffer.Get(tick));
            } catch (IndexOutOfRangeException)
            {
              Debug.LogWarning($"[Input] Couldn't restore input for tick {tick}, ignoring");
            }
        }

        public void SaveInput(int tick)
        {
            if (IsOwn)
            {
                hasNewInput = true;
                earliestReceivedInput = Math.Min(earliestReceivedInput, tick);
            }

            T input = CaptureInput();
            inputBuffer.Set(input, tick);
        }

        public void CommitInput(int tick)
        {
            CommitInput(inputBuffer.Get(tick), tick);
        }

        protected abstract void CommitInput(T state, int tick);

        protected void HandleInputCommit(T state, int tick)
        {
            inputBuffer.Set(state, tick);
            earliestReceivedInput = Math.Min(tick, earliestReceivedInput);
            hasNewInput = true;
        }

        public void AcknowledgeInputs()
        {
            earliestReceivedInput = NetworkManager.LocalTime.Tick;
            hasNewInput = false;
        }
    }
}