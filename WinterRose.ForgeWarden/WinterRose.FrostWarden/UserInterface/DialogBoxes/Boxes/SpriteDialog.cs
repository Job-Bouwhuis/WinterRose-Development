using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes.Boxes;

//    class SpriteDialog : Dialog
//    {
//        public SpriteDialog(
//            string title, 
//            string spriteSource,
//            DialogPlacement placement, 
//            DialogPriority priority) 
//            : base(title, spriteSource, placement, priority, ["Ok"], null, null)
//        {
//        }

//        public override void DrawContent(Rectangle bounds, float contentAlpha, ref int padding, ref float innerWidth, ref int y)
//        {
//            Sprite sprite = SpriteCache.Get(Message.ToString());

//            float maxWidth = bounds.Width - padding * 2;

//            // Calculate how much vertical space is left below y, minus padding and button area
//            float reservedForButtons = 30 + 25 + padding;
//            float maxHeight = bounds.Height - (y - bounds.Y) - reservedForButtons;

//            float aspect = (float)sprite.Width / sprite.Height;

//            float targetWidth = maxWidth;
//            float targetHeight = targetWidth / aspect;

//            if (targetHeight > maxHeight)
//            {
//                targetHeight = maxHeight;
//                targetWidth = targetHeight * aspect;
//            }

//            float drawX = bounds.X + (bounds.Width - targetWidth) / 2;
//            float drawY = y;

//            ray.DrawTexturePro(
//                sprite.Texture,
//                new Rectangle(0, 0, sprite.Width, sprite.Height),
//                new Rectangle(drawX, drawY, targetWidth, targetHeight),
//                new Vector2(0, 0),
//                0f,
//                Style.ContentTint
//            );

//            y += (targetHeight + padding).CeilingToInt();
//        }

//        public override void Update()
//        {

//        }
//    }

