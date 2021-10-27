namespace LunarBind.Yielding
{
    using MoonSharp.Interpreter;
    using System;

    /// <summary>
    /// Waits until the passed in function returns true
    /// </summary>
    [MoonSharpUserData]
    public class WaitUntil : Yielder
    {
        Func<bool> waiter;

        public WaitUntil(Func<bool> waiter)
        {
            this.waiter = waiter;
        }
        public override bool CheckStatus()
        {
            return waiter();
        }
    }
}
