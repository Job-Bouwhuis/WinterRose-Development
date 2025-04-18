using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using WinterRose.Monogame;
using WinterRose.Serialization;

namespace TopDownGame.ScrollUI
{
    class ScrollGridUI : ActiveRenderer
    {
        public override RectangleF Bounds => bounds;
        private RectangleF bounds = new RectangleF(200, 400, 0, 0);
        public override TimeSpan DrawTime { get; protected set; }

        public ScrollGridSettings Settings { get; set; } = new();

        private Dictionary<Sprite, Action<Sprite>> items = [];

        private Sprite pixel;

        public void SetSize(Vector2 size)
        {
            bounds.Size = size;
        }

        public void AddItem(Sprite item, Action<Sprite> onClick)
        {
            items.Add(item, onClick);
        }

        protected override void Awake()
        {
            bounds.Position = transform.position;
            pixel = new Sprite(1, 1, Color.White);
        }

        protected override void Update()
        {
            bounds.Position = transform.position;
        }

        public override void Render(SpriteBatch batch)
        {

        }
    }

    [IncludeAllProperties]
    public class ScrollGridSettings
    {
        /// <summary>
        /// The color of the entire control's background
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.DarkGray;
        /// <summary>
        /// The color of the background of the scrollbar
        /// </summary>
        public Color ScrollBarBackgroundColor { get; set; } = Color.LightGray;
        /// <summary>
        /// The color the scrollbar itself has
        /// </summary>
        public Color ScrollBarColor { get; set; } = Color.White;
        /// <summary>
        /// The color of the selected item outline
        /// </summary>
        public Color SelectedItemColor { get; set; } = Color.White;
        /// <summary>
        /// How many pixels bigger than the selected item sprite should the selected outline be
        /// </summary>
        public float SelectedItemSize { get; set; } = 5;
        /// <summary>
        /// How many pixels may the <see cref="SelectedItemSize"/> grow and shrink over time
        /// </summary>
        public float SelectedItemSizePulseAmount { get; set; } = 3;
        /// <summary>
        /// The speed at which <see cref="SelectedItemSize"/> grows and shrinks
        /// </summary>
        public float SelectedItemSizePulseSpeed { get; set; } = 20;
        /// <summary>
        /// Whether or not to pulse the <see cref="SelectedItemSize"/> at all
        /// </summary>
        public bool SelectedItemPulseEnabled { get; set; } = true;
    }
}
