namespace WinterRose.Monogame.DamageSystem;

public class NeutralDamage : DamageType
{
    public override void DealDamage(Vitality target)
    {
        target.DealDamage(Damage);
    }

    public NeutralDamage() { }
    public NeutralDamage(int BaseDamage) : base(BaseDamage) { }
}
