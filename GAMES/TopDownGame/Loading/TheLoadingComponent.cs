using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Monogame;
using WinterRose.Monogame.Worlds;
using WinterRose.WinterForgeSerializing.Logging;

namespace TopDownGame.Loading
{
    class TheLoadingComponent(TextRenderer text) : ObjectBehavior
    {
        internal class Progresser() : WinterForgeProgressTracker(WinterForgeProgressVerbosity.Instance)
        {
            public Action<string> onMessage = delegate { };
            public Action<float> onProgress = delegate { };

            protected override void Report(string message) => onMessage(message);
            protected override void Report(float graphPercentage)
            {
                onProgress(graphPercentage);
            }
        }

        internal string? templateName;
        internal Type? templateType;

        float precentage = 0;
        float barmax = 400;

        internal Progresser progress = new Progresser();

        Task<World> worldLoadTask = null!;

        protected override void Start()
        {
            worldLoadTask = DoLoad();
            text.Text = "Please wait...";
            progress.onMessage += m => text.Text = m;
            progress.onProgress += p => precentage = p;
        }

        private async Task<World> DoLoad()
        {
            World w = null!;
            await Task.Run(() =>
            {
                if (templateName is null)
                    w = World.FromTemplate(templateType!, progress);
                else
                    w = World.FromTemplateFile(templateName, progress);
            });
            return w;
        }

        protected override void Update()
        {
            float targetScaleX = barmax * precentage;
            transform.scale = new Vector2(targetScaleX, 1);

            if(worldLoadTask.IsCompleted)
            {
                Universe.CurrentWorld = worldLoadTask.Result;
            }
        }
    }
}
