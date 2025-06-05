using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Components;
using WinterRose.FrostWarden.Entities;

namespace WinterRose.FrostWarden.Worlds
{
    public class World
    {
        private readonly List<Entity> entities = new();

        public void AddEntity(Entity entity)
            => entities.Add(entity);

        public void RemoveEntity(Entity entity)
            => entities.Remove(entity);

        public void Update()
        {
            foreach (var entity in entities)
            {
                foreach (var updatable in entity.GetAll<IUpdatable>())
                    updatable.Update();
            }
        }

        public void Draw(Matrix4x4 viewMatrix)
        {
            foreach (var entity in entities)
            {
                foreach (var renderable in entity.GetAll<IRenderable>())
                    renderable.Draw(viewMatrix);
            }
        }

        public IEnumerable<T> GetAll<T>() where T : class, IComponent
        {
            foreach(var entity in entities)
                foreach (T c in entity.GetAll<T>())
                    yield return c;
        }
    }
}
