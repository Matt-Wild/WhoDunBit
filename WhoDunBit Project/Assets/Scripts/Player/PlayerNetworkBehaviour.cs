using Unity.Netcode;

namespace WhoDunBit.Player
{
    public class PlayerNetworkBehaviour : NetworkBehaviour, IPlayerBehaviour
    {
        private PlayerMaster _master;

        public PlayerMaster Master => _master;

        public virtual void Init(PlayerMaster master)
        {
            _master = master;
        }
    }
}
