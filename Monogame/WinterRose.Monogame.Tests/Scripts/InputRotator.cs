using Microsoft.Xna.Framework.Input;

namespace WinterRose.Monogame.Tests.Scripts;

internal class InputRotator : ObjectBehavior
{
    public float Speed { get; set; } = 70;

    private void Update()
    {
        if(Input.GetKey(Keys.Q)) 
            transform.rotation -= Speed * Time.SinceLastFrame;
        if (Input.GetKey(Keys.E))
            transform.rotation += Speed * Time.SinceLastFrame;
    }
}
