using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace SnowLibraryTesting
{
    internal class CoolComponent1 : IUpdatableComponnet
    {
        public string Name => "yeet";

        public string achternaam => "kobe";

        public void GetPopcorn()
        {
            Console.WriteLine("splat");
        }

        public void Update()
        {
            Console.WriteLine("om nom nom");
        }
    }
    internal class Coolcomponent2 : IUpdatableComponnet
    {
        public string Name => "yeet2";

        public void Update()
        {
            Console.WriteLine("om nom nom 2");
        }

        public void Draw()
        {

        }
    }

    internal class interfaceDemotests
    {
        List<Component> components = new List<Component>();

        public void UpdateGameObject()
        {
            foreach(var component in components)
            {
                if (component is IUpdatableComponnet updatable)
                {
                    updatable.Update();
                }
                if (component is IDrawableComponent drawable)
                {
                    drawable.Draw();
                }
            }

        }
    }



    public interface IUpdatableComponnet
    {
        string Name { get; }

        void Update();
    }

    public interface IDrawableComponent
    {
        string achternaam { get; }

        void Draw();
    }
}
