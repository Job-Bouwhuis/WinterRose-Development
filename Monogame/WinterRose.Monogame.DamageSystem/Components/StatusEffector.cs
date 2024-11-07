using System;
using System.Collections.Generic;
using System.Linq;
using WinterRose.Monogame.DamageSystem;

namespace WinterRose.Monogame.StatusSystem
{
    public class StatusEffector : ObjectBehavior
    {
        [IgnoreInTemplateCreation]
        public List<StatusEffect> effects = [];

        /// <summary>
        /// The <see cref="Vitality"/> component on the owner of this status effector. May be null in edge cases.
        /// </summary>
        [Hidden]
        public Vitality Vitals => vitals ??= FetchComponent<Vitality>();
        [Hidden, IgnoreInTemplateCreation]
        private Vitality vitals;

        private void Update()
        {
            for (int i = 0; i < effects.Count; i++)
            {
                StatusEffect effect = effects[i];
                if (effect.SecondsPerStack != -1)
                    HandleEffectTimeout(effect);

                if (effect.UpdateType == StatusEffectUpdateType.Always)
                    effect.Update(this);

                if(effect.updateStacks)
                {
                    effect.updateStacks = false;
                    effect.StacksUpdated(this, effect.previousStacks, effect.Stacks);
                }

                if(effect.Stacks == 0)
                    effects.RemoveAt(i);
            }
        }
        private void HandleEffectTimeout(StatusEffect effect)
        {
            effect.currentSeconds += Time.SinceLastFrame;
            if (effect.currentSeconds > effect.SecondsPerStack)
            {

                if (effect.RemoveAllStacksOntimeout)
                    effect.Stacks = 0;
                else
                {
                    effect.Stacks -= 1;
                    effect.currentSeconds = 0;
                }

                if (effect.UpdateType == StatusEffectUpdateType.StackRemoval)
                    effect.Update(this);
            }
        }

        /// <summary>
        /// Applies the status effect of this type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initalStacks">The initial stacks that this effect starts off at when applied</param>
        /// <param name="secondsPerStack">The amount of seconds per stack. -1 to have infinite stacks.</param>
        public void Apply<T>(int initalStacks = 1, float secondsPerStack = -1) where T : StatusEffect
        {
            if(HasEffect(out T e))
            {
                e.Stacks += initalStacks;
                e.SecondsPerStack = secondsPerStack;
                return;
            }

            StatusEffect effect = ActivatorExtra.CreateInstance<T>();
            effect.Stacks = initalStacks;
            effect.SecondsPerStack = secondsPerStack;

            effects.Add(effect);

            if (effect.UpdateType == StatusEffectUpdateType.Static)
                effect.Update(this);
        }

        public void Apply(StatusEffect effect)
        {
            if (HasEffect(effect.GetType(), out StatusEffect e))
            {
                e.Stacks += effect.Stacks;
                e.SecondsPerStack = effect.SecondsPerStack;
                return;
            }

            effects.Add(effect);
        }
        /// <summary>
        /// Checks if there exists a effect of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns><see langword="true"/> if the effect of type <typeparamref name="T"/> is present, <see langword="false"/> if not</returns>
        public bool HasEffect<T>() => effects.Any(x => x is T);
        /// <summary>
        /// Checks if there exists a effect of type <paramref name="effectType"/>
        /// </summary>
        /// <returns><see langword="true"/> if the effect of type  is present, <see langword="false"/> if not</returns>
        public bool HasEffect(Type effectType) => effects.Any(x => x.GetType() == effectType);
        /// <summary>
        /// Checks if there exists a effect of type <typeparamref name="T"/>. And if so, populates <paramref name="effect"/> with it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="effect"></param>
        /// <returns><see langword="true"/> if the effect of type <typeparamref name="T"/> is present, <see langword="false"/> if not</returns>
        public bool HasEffect<T>(out T effect) where T : StatusEffect => (effect = GetEffect<T>()) is not null;
        /// <summary>
        /// Checks if there exists a effect of type <typeparamref name="T"/>. And if so, populates <paramref name="effect"/> with it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="effect"></param>
        /// <returns><see langword="true"/> if the effect of type <typeparamref name="T"/> is present, <see langword="false"/> if not</returns>
        public bool HasEffect(Type effectType, out StatusEffect effect) => (effect = GetEffect(effectType)) is not null;
        /// <summary>
        /// Gets the effect of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The effect of type <typeparamref name="T"/>, or <see langword="null"/> if there is no effect found</returns>
        public T? GetEffect<T>() where T : StatusEffect => effects.FirstOrDefault(x => x is T) as T;
        /// <summary>
        /// Gets the effect of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The effect of type <typeparamref name="T"/>, or <see langword="null"/> if there is no effect found</returns>
        public StatusEffect GetEffect(Type effectType) => effects.FirstOrDefault(x => x.GetType() == effectType);
        /// <summary>
        /// Removes the effect of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Remove<T>() where T : StatusEffect
        {
            T? t = GetEffect<T>();
            if (t is null)
                return;
            t.Stacks = 0;
        }
    }
}
