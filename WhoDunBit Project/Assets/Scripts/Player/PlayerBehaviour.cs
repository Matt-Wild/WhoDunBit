using UnityEngine;

namespace WhoDunBit.Player
{
    public class PlayerBehaviour : MonoBehaviour, IPlayerBehaviour
    {
        private PlayerMaster _master;

        public PlayerMaster Master => _master;

        public virtual void Init(PlayerMaster master)
        {
            _master = master;
        }
    }

    public interface IPlayerBehaviour
    {
        public PlayerMaster Master { get; }

        public void Init(PlayerMaster master);
    }
}
