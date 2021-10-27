namespace LunarBind.Yielding
{
    using MoonSharp.Interpreter;
    /// <summary>
    /// Waits X amount of calls
    /// </summary>
    [MoonSharpUserData]
    public class WaitFrames : Yielder
    {
        long framesLeft;

        public WaitFrames(long frames)
        {
            framesLeft = frames;
        }
        public override bool CheckStatus()
        {
            return (framesLeft-- <= 0);
        }
    }
}
