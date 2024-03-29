﻿using UnityEngine;
using Unity.Netcode;
using System;

namespace FunkySheep.NetWind
{
    [AddComponentMenu("FunkySheep/NetWind/Rewindable Transform State")]
    public class RewindableTransformState : RewindableStateBehaviour<RewindableTransformState.State>
    {
        [Serializable]
        public struct State : INetworkSerializable
        {
            public Vector3 localPosition;
            public Vector3 localScale;
            public Quaternion localRotation;

            // INetworkSerializable
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref localPosition);
                serializer.SerializeValue(ref localScale);
                serializer.SerializeValue(ref localRotation);
            }
            // ~INetworkSerializable
        }

        [Header("Configuration")]
        [SerializeField] private bool isInterpolated = true;

        public override bool IsInterpolated => isInterpolated;

        protected override State CaptureState()
        {
            return new State()
            {
                localPosition = transform.localPosition,
                localScale = transform.localScale,
                localRotation = transform.localRotation
            };
        }

        protected override void ApplyState(State state)
        {
            transform.localPosition = state.localPosition;
            transform.localScale = state.localScale;
            transform.localRotation = state.localRotation;
        }

        public override void Simulate(int tick, float deltaTime)
        {
        }

        protected override void CommitState(State state, int tick)
        {
            CommitStateClientRpc(state, tick);
        }

        [ClientRpc]
        private void CommitStateClientRpc(State state, int tick)
        {
            HandleStateCommit(state, tick);
        }

        public override void InterpolateState(int tickFrom, int tickTo, float f)
        {
            var fromState = stateBuffer.Get(tickFrom);
            var toState = stateBuffer.Get(tickTo);

            var interpolatedState = new State()
            {
                localPosition = Vector3.Lerp(fromState.localPosition, toState.localPosition, f),
                localScale = Vector3.Lerp(fromState.localScale, toState.localScale, f),
                localRotation = Quaternion.Lerp(fromState.localRotation, toState.localRotation, f)
            };

            ApplyState(interpolatedState);
        }
    }
}
