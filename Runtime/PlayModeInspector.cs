//
// PlayMode Inspector for Unity. Copyright (c) 2015-2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityPlayModeInspector
//
using System;

namespace Oddworm.Framework
{
    /// <summary>
    /// Use this attribute to expose a method to the PlayMode Inspector window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    sealed public class PlayModeInspectorMethodAttribute : Attribute
    {
    }
}
