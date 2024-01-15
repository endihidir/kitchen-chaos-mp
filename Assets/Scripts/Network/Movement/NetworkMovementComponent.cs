using Unity.Netcode;
using UnityEngine;

namespace Unity.Multiplayer.Samples.Utilities.ClientAuthority.Movement
{
    public class NetworkMovementComponent : NetworkBehaviour
    {
        [SerializeField] private Player _player;

        [SerializeField] private float _speed;

        private int _tick = 0;
        private float _tickRate = 1f / 60f;
        private float _tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private InputState[] _inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        public TransformState _previousTransformState;
        
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            ServerTransformState.OnValueChanged -= OnServerStateChanged;
        }

        private void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            _previousTransformState = previousvalue;
        }

        public void ProcessLocalPlayerController(Vector2 movementInput)
        {
            _tickDeltaTime += Time.deltaTime;

            if (_tickDeltaTime > _tickRate)
            {
                var bufferIndex = _tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MovePlayerServerRPC(_tick, movementInput);
                    MovePlayer(movementInput);
                }
                else
                {
                    MovePlayer(movementInput);
                    SetTransformState(_tick);
                }

                var inputState = new InputState()
                {
                    Tick = _tick,
                    movementInput = movementInput
                };
                
                var transformState = new TransformState()
                {
                    Tick = _tick,
                    Position = _player.transform.position,
                    Rotation = _player.transform.rotation,
                    HasStartedMoving = true
                };

                _inputStates[bufferIndex] = inputState;
                _transformStates[bufferIndex] = transformState;

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }

        public void ProcessSimulatedPlayerMovement()
        {
            if(ServerTransformState.Value is null) return;
            
            _tickDeltaTime += Time.deltaTime;
            
            if (_tickDeltaTime > _tickRate)
            {
                if (ServerTransformState.Value.HasStartedMoving)
                {
                    _player.transform.position = ServerTransformState.Value.Position;
                    _player.transform.rotation = ServerTransformState.Value.Rotation;
                }
                
                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }

        [ServerRpc]
        private void MovePlayerServerRPC(int tick, Vector2 movementInput)
        {
            MovePlayer(movementInput);

            SetTransformState(tick);
        }

        private void MovePlayer(Vector2 movementInput)
        {
            /*var lastMovementDir = _player.HandleMovement(movementInput, _tickRate, _speed);
        
            _player.HandleInteractions(lastMovementDir);*/
        }

        private void SetTransformState(int tick)
        {
            var state = new TransformState()
            {
                Tick = tick,
                Position = _player.transform.position,
                Rotation = _player.transform.rotation,
                HasStartedMoving = true
            };

            _previousTransformState = ServerTransformState.Value;

            ServerTransformState.Value = state;
        }
    }
}