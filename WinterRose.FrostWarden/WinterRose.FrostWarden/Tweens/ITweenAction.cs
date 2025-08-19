namespace WinterRose.ForgeWarden.Tweens;

public interface ITweenAction
{
    void Update();
    bool Completed { get; }
}

