
namespace LunarBind.Yielding
{
    using MoonSharp.Interpreter;
    
    /// <summary>
    /// Waits until Done is set to true by an external source
    /// </summary>
    [MoonSharpUserData]
    public class WaitForDone : Yielder
    {
        public bool Done { get; set; } = false;

        public override bool CheckStatus()
        {
            return Done;
        }
    }
}
