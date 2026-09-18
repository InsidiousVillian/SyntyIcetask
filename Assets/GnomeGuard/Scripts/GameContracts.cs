using UnityEngine;

namespace GnomeGuard
{
    public interface IHittable
    {
        bool TryHit(Vector3 point, Vector3 incoming, int damage);
    }

    public interface IPickup
    {
        void Collect();
    }

    public interface IGameModifiers
    {
        void AddFreeze(float seconds);
        void AddSlow(float seconds);
        void AddRapidFire(float seconds);
        void AddHighlight(float seconds);
        void AddShield(float seconds);
        void RepairTree(int amount);
        void AddScore(int amount);
        void Announce(string banner);
        void CelebrateTree();
    }

    public interface IPowerUpEffect
    {
        void Apply(IGameModifiers game);
    }
}
