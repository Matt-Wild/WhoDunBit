using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WhoDunBit.Player
{
    internal sealed class MovementSnapshotBuffer
    {
        private readonly List<MovementState> _receivedMovementStates = new();
        private float _currentInterpolationDelay;
        private float _targetInterpolationDelay;
        private float _healthyBufferTime;

        public void Reset(float defaultDelay)
        {
            _receivedMovementStates.Clear();
            _currentInterpolationDelay = defaultDelay;
            _targetInterpolationDelay = defaultDelay;
            _healthyBufferTime = 0f;
        }

        public void Add(MovementState current)
        {
            // Ignore state that hasn't had a pose captured yet
            if (current.SampleTime < 0d)
            {
                return;
            }

            // Ignore duplicate or older samples
            if (_receivedMovementStates.Count > 0 && current.SampleTime <= _receivedMovementStates[^1].SampleTime)
            {
                return;
            }

            _receivedMovementStates.Add(current);

            // Keep a short history for interpolation
            if (_receivedMovementStates.Count > 32)
            {
                _receivedMovementStates.RemoveAt(0);
            }
        }

        public bool TrySample(NetworkTime serverTime, float deltaTime, float minDelay, float maxDelay, out MovementState movementState)
        {
            movementState = default;

            if (_receivedMovementStates.Count == 0)
            {
                return false;
            }

            // Build up the buffer faster than we give it back
            float adjustmentRate = _targetInterpolationDelay > _currentInterpolationDelay ? 0.10f : 0.05f;
            _currentInterpolationDelay = Mathf.MoveTowards(_currentInterpolationDelay, _targetInterpolationDelay, adjustmentRate * deltaTime);

            double renderTime = serverTime.Time - _currentInterpolationDelay;
            movementState = _receivedMovementStates[0];

            if (_receivedMovementStates.Count >= 2 && renderTime >= movementState.SampleTime)
            {
                float bufferedSeconds = (float)(_receivedMovementStates[^1].SampleTime - renderTime);
                UpdateTargetInterpolationDelay(bufferedSeconds, serverTime.FixedDeltaTime, deltaTime, minDelay, maxDelay);
            }

            // Hold the first pose until render time reaches it
            if (renderTime <= movementState.SampleTime)
            {
                return true;
            }

            for (int i = 1; i < _receivedMovementStates.Count; i++)
            {
                MovementState next = _receivedMovementStates[i];

                if (next.SampleTime <= renderTime)
                {
                    movementState = next;
                    continue;
                }

                // Positions can only be blended within the same coordinate space
                bool sameSpace = movementState.HasNetworkedSupport == next.HasNetworkedSupport && (!movementState.HasNetworkedSupport || movementState.SupportReference.Equals(next.SupportReference));

                if (sameSpace)
                {
                    float blend = (float)((renderTime - movementState.SampleTime) / (next.SampleTime - movementState.SampleTime));

                    // Start the jump animation on the first frame that blends into takeoff
                    if (blend > 0f && movementState.Grounded && !next.Grounded && movementState.JumpSequence != next.JumpSequence)
                    {
                        movementState.JumpSequence = next.JumpSequence;
                        movementState.Grounded = false;
                    }

                    movementState.Position = Vector3.Lerp(movementState.Position, next.Position, blend);
                    movementState.Rotation = Quaternion.Slerp(movementState.Rotation, next.Rotation, blend);

                    movementState.LocalVelocity = Vector3.Lerp(movementState.LocalVelocity, next.LocalVelocity, blend);

                    movementState.SampleTime = renderTime;
                }

                return true;
            }

            // Hold the latest pose if no newer sample is available
            return true;
        }

        private void UpdateTargetInterpolationDelay(float bufferedSeconds, float tickDuration, float deltaTime, float minDelay, float maxDelay)
        {
            // Estimate how much would be buffered at the target delay
            float targetBuffer = bufferedSeconds + (_targetInterpolationDelay - _currentInterpolationDelay);

            if (targetBuffer < 0f)
            {
                // Recover the deficit and leave one tick of breathing room
                _targetInterpolationDelay = Mathf.Clamp(_targetInterpolationDelay - targetBuffer + tickDuration, minDelay, maxDelay);
                _healthyBufferTime = 0f;

                return;
            }

            if (targetBuffer < tickDuration * 2f)
            {
                _healthyBufferTime = 0f;
                return;
            }

            // Only reduce delay after several seconds of spare buffer
            _healthyBufferTime += deltaTime;
            if (_healthyBufferTime >= 3f)
            {
                _targetInterpolationDelay = Mathf.Max(minDelay, _targetInterpolationDelay - tickDuration);
                _healthyBufferTime = 0f;
            }
        }
    }
}
