namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using LunarBind.Standards;
    internal static class StandardHelpers
    {
        internal static LuaFuncType GetLuaFuncType(bool coroutine, bool autoreset)
        {
            if (!coroutine) return LuaFuncType.Function;
            else if (autoreset) return LuaFuncType.AutoCoroutine;
            else return LuaFuncType.SingleUseCoroutine;
        }

    }
}
