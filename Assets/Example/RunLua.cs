using System;
using System.Collections;
using MoonSharp.Interpreter;
using UnityEngine;

public class RunLua : MonoBehaviour
{
    private DynValue _coroutine;
    private Script _script;
    private void Start()
    {
        string code = @"    
            main = function()
                waitAndLog1()
                waitAndLog2()
            end

            function waitAndLog1()
                log('first')
                wait(3)
                log('second')
            end

            function waitAndLog2()
                log('first')
                wait(5)
                log('second')
            end

            return main
            ";
        _script = new Script();
        _script.Globals["log"]  = (Action<string>)Log;
        _script.Globals["wait"] = (Func<float, DynValue>)Wait;
        // _script.DoString("function wait(time) coroutine.yield(time) end");
        DynValue func = _script.DoString(code);

        _coroutine = _script.CreateCoroutine(func);
        _coroutine.Coroutine.Resume();

    }

    private DynValue Wait(float time)
    {
        StartCoroutine(WaitCoroutine(time));
        return DynValue.NewYieldReq(new DynValue[]{});
    }

    private IEnumerator WaitCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        _coroutine.Coroutine.Resume();
    }

    private void Log(string log)
    {
        Debug.Log(log);
    }
}