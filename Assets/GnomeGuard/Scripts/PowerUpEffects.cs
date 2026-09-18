namespace GnomeGuard
{
    public sealed class FreezeEffect : IPowerUpEffect
    {
        public void Apply(IGameModifiers game)
        {
            game.AddFreeze(4.5f);
            game.Announce("FREEZE!");
        }
    }

    public sealed class RapidFireEffect : IPowerUpEffect
    {
        public void Apply(IGameModifiers game)
        {
            game.AddRapidFire(8f);
            game.Announce("RAPID FIRE!");
        }
    }

    public sealed class RepairEffect : IPowerUpEffect
    {
        public void Apply(IGameModifiers game)
        {
            game.RepairTree(2);
            game.AddScore(50);
            game.Announce("SANTA REPAIRS THE TREE");
            game.CelebrateTree();
        }
    }

    public sealed class ScoreBurstEffect : IPowerUpEffect
    {
        public void Apply(IGameModifiers game)
        {
            game.AddScore(120);
            game.Announce("GOLDEN NUGGET +120");
        }
    }

    public sealed class SlowMoEffect : IPowerUpEffect
    {
        public void Apply(IGameModifiers game)
        {
            game.AddSlow(6.5f);
            game.Announce("VACATION SLOW-MO");
        }
    }

    public sealed class HighlightEffect : IPowerUpEffect
    {
        public void Apply(IGameModifiers game)
        {
            game.AddHighlight(8f);
            game.Announce("THEORY: BONUS HITS");
        }
    }

    public sealed class ShieldEffect : IPowerUpEffect
    {
        public void Apply(IGameModifiers game)
        {
            game.AddShield(6.5f);
            game.Announce("BASALT SHIELD");
        }
    }

    public static class PowerUpCatalog
    {
        public static IPowerUpEffect For(GnomeKind kind)
        {
            switch (kind)
            {
                case GnomeKind.Wizard: return new FreezeEffect();
                case GnomeKind.Soldier: return new RapidFireEffect();
                case GnomeKind.Santa: return new RepairEffect();
                case GnomeKind.Dwarf: return new ScoreBurstEffect();
                case GnomeKind.Beach: return new SlowMoEffect();
                case GnomeKind.Theorist: return new HighlightEffect();
                case GnomeKind.Basalt: return new ShieldEffect();
                default: return new ScoreBurstEffect();
            }
        }
    }
}
