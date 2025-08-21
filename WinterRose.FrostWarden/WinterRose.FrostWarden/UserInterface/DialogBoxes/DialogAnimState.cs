namespace WinterRose.ForgeWarden.UserInterface.DialogBoxes
{
    public struct DialogAnimation
    {
        public float Elapsed;

        public float Alpha;
        public float ScaleWidth;
        public float ScaleHeight;

        public bool Completed { get; internal set; }
    }

}
