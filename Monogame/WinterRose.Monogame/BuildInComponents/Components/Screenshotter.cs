using Microsoft.Xna.Framework.Input;
using System;

namespace WinterRose.Monogame;

public class Screenshotter : ObjectBehavior
{
    private Camera cam;
    Keys screenshotKey;

    public Screenshotter(Keys key)
    {
        screenshotKey = key;
    }
    private Screenshotter() { }

    protected override void Awake()
    {
        cam = FetchComponent<Camera>();
        if (cam is null)
            throw new Exception("Component of type Screenshotter requires a camera to be on the same object.");
    }

    protected override void Update()
    {
        if (!Input.GetKeyDown(screenshotKey))
            return;

        var screenshot = cam.Screenshot;

        string name = 
            $"{DateTime.Now.Day:D2}-" +
            $"{DateTime.Now.Month:D2}-" +
            $"{DateTime.Now.Year:D4}--" +
            $"{DateTime.Now.Hour:D2}-" +
            $"{DateTime.Now.Minute:D2}-" +
            $"{DateTime.Now.Second:D2}";

        screenshot.SaveAsPng($"Content/Screenshots/{name}.png");
    }
}
