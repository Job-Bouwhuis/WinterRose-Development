using WinterRose.Monogame;
using WinterRose.Monogame.DamageSystem;

namespace TopDownGame.UIStuff;

class PlayerHealthBar : ObjectComponent
{
    private const float min = 0;
    private float max = 0;
    protected override void Awake()
    {
        max = transform.scale.X;
        world.FindObjectWithFlag("Player").FetchComponent<Vitality>().Health.OnHealthChanged += PlayerDamaged;
    }

    private void PlayerDamaged(Health health, int previousHP, int newHP)
    {
        float percentage = newHP / (float)health.MaxHealth;
        transform.scale = new(float.Lerp(min, max, percentage), transform.scale.Y);
    }
}
