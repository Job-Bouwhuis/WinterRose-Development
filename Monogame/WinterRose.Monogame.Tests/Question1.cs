using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WinterRose.Monogame.UI;
using WinterRose.Monogame.Worlds;

namespace WinterRose.Monogame.Tests;

[WorldTemplate]
internal class Question1 : WorldTemplate
{
    public Question1()
    {
        Name = "Question1";
    }
    public override void Build(in World world)
    {
        WorldObject obj = world.CreateObject("Button1");
        obj.AttachComponent<SpriteRenderer>((MonoUtils.WindowResolution.X / 2) - 50, MonoUtils.WindowResolution.Y - 200, Color.DarkGreen).Origin = new();
        obj.transform.position = new(45, 100);
        var btn = obj.AttachComponent<Button>();
        btn.text.text = "Die";
        btn.OnClick += () =>
        {
            Debug.Log("You chose to die.", true);
        };

        obj = world.CreateObject("Button2");
        obj.AttachComponent<SpriteRenderer>((MonoUtils.WindowResolution.X / 2) - 50, MonoUtils.WindowResolution.Y - 200, Color.DarkRed).Origin = new();
        obj.transform.position = new((MonoUtils.WindowResolution.X / 2) + 5, 100);
        btn = obj.AttachComponent<Button>();
        btn.text.text = "Be Killed";
        btn.OnClick += () =>
        {
            Debug.Log("You chose to be killed", true);
        };

        obj = world.CreateObject("Questionair");
        obj.AddDrawBehavior((obj, batch) =>
        {
            string text = "Would you rather die or be killed?";
            // calculate the size of the text
            var size = MonoUtils.DefaultFont.MeasureString(text);
            // calculate the position so its in the center
            var pos = new Vector2(MonoUtils.WindowResolution.X / 2, 40);
            // draw the text
            batch.DrawString(MonoUtils.DefaultFont, text, pos, Color.White, 0, new(size.X / 2, size.Y / 2), 1, SpriteEffects.None, 1);
        });
        obj.AddUpdateBehavior(x =>
        {
            if (Input.GetKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                WorldEditor.Show = true;
            }
        });
    }
}
