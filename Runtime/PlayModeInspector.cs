//
// PlayMode Inspector for Unity. Copyright (c) 2015-2022 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityPlayModeInspector
//
using System;
using UnityEngine;

namespace Oddworm.Framework
{
    /// <summary>
    /// Use this attribute to expose a method to the PlayMode Inspector window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PlayModeInspectorMethodAttribute : Attribute
    {
        /// <summary>
        /// The display name in the PlayMode Inspector titlebar.
        /// It displays "TypeName.MethodName" by default, the displayName property allows you to override it.
        /// </summary>
        public string displayName
        {
            get;
            set;
        }
    }
}
