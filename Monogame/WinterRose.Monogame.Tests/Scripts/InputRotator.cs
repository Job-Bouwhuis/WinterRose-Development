using Microsoft.Xna.Framework.Input;

namespace WinterRose.Monogame.Tests.Scripts;

internal class InputRotator : ObjectBehavior
{
    public float Speed { get; set; } = 70;

    protected override void Update()
    {
        if(Input.GetKey(Keys.Q)) 
            transform.rotation -= Speed * Time.deltaTime;
        if (Input.GetKey(Keys.E))
            transform.rotation += Speed * Time.deltaTime;
    }
}
