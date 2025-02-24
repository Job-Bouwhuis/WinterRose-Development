using Microsoft.Xna.Framework.Input;
namespace WinterRose.Monogame.Tests;

internal class PositionResetter : ObjectBehavior
{
    protected override void Update()
    {
        if (Input.GetKeyDown(Keys.Space))
            transform.position = new(0, 0);
    }
}
