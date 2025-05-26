using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using WinterRose.Monogame.Worlds;

namespace TopDownGame.Loading
{
    class LoadingLevel : WorldTemplate
    {
        private string templateName;
        private Type templateType;

        public LoadingLevel(string templateName)
        {
            this.templateName = templateName;
        }

        public LoadingLevel(Type templateType)
        {
            this.templateType = templateType;
        }

        public override void Build(in World world)
        {
            // Create a new object named "LoadingBar"
            WorldObject loadingBarObject = world.CreateObject("LoadingBar");

            // Create a sprite for the loading bar background (gray bar)
            Sprite backgroundSprite = new Sprite(400, 20, new Color(0.2f, 0.2f, 0.2f)); // dark gray
            var backgroundRenderer = loadingBarObject.AttachComponent<SpriteRenderer>(backgroundSprite);
            backgroundRenderer.Origin = new Vector2(0, 0.5f); // left-center origin

            // Create a sprite for the actual progress fill (green bar)
            Sprite progressSprite = new Sprite(1, 20, new Color(0.1f, 0.8f, 0.1f)); // bright green, start width 0
            WorldObject progressObject = world.CreateObject("LoadingProgress");
            var progressRenderer = progressObject.AttachComponent<SpriteRenderer>(progressSprite);
            progressRenderer.LayerDepth = 0.6f;
            progressRenderer.Origin = new Vector2(0, 0.5f);

            // Attach progressObject as child to loadingBarObject so it moves with the background
            progressObject.transform.parent = loadingBarObject.transform;

            // Position the loading bar somewhere centered horizontally and near bottom
            loadingBarObject.transform.position = new Vector2(MonoUtils.WindowBounds.Width / 2f - 200, 50);

            TextRenderer tr = world.CreateObject<TextRenderer>("MessageText");
            tr.transform.parent = loadingBarObject.transform;
            tr.transform.localPosition = new Vector2(0, 50);

            TextRenderer tr2 = world.CreateObject<TextRenderer>("progressText");
            tr2.transform.parent = loadingBarObject.transform;
            tr2.transform.localPosition = new Vector2(0, 100);

            var loader = progressObject.AttachComponent<TheLoadingComponent>(tr);
            loader.progress.onProgress += p => tr2.Text = (p * 100).ToString() + '%';
            if (templateName is null)
                loader.templateType = templateType;
            else
                loader.templateName = templateName;
            // You’d also need a component or system that updates progressSprite.Width dynamically based on loading progress
            // For example, you could add a LoadingBarComponent that modifies progressSprite.Width on each frame
        }

    }
}
