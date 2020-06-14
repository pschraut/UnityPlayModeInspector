//
// PlayMode Inspector for Unity. Copyright (c) 2015-2020 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityPlayModeInspector
//
#pragma warning disable IDE1006, IDE0017
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
using Oddworm.Framework;

namespace Oddworm.EditorFramework
{
    class PlayModeInspector : EditorWindow
    {
        List<AbstractEntry> m_Entries; // A list of all methods to inspect.
        Vector2 m_ScrollPosition; // The scroll position in the playmode inspector.
        bool m_ExceptionOccurred; // Whether an exception occurred while drawing the playmode inspector.
        bool m_Locked; // Whether the object selected is locked.

        [MenuItem("Window/Analysis/PlayMode Inspector", priority = 500)]
        static void CreateMenuItem()
        {
            var wnd = EditorWindow.GetWindow<PlayModeInspector>();
            if (wnd != null)
                wnd.Show();
        }

        void OnEnable()
        {
            m_Locked = false;
            m_ExceptionOccurred = false;
            titleContent = new GUIContent("PlayMode Inspector");

            var icon = EditorGUIUtility.IconContent("UnityEditor.InspectorWindow");
            if (icon != null)
                titleContent.image = icon.image;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange playMode)
        {
            m_Locked = false;
            m_ExceptionOccurred = false;

            OnSelectionChange();
        }

        void OnSelectionChange()
        {
            if (!Application.isPlaying || m_Locked)
                return;

            m_Entries = new List<AbstractEntry>();
            m_ExceptionOccurred = false;

            // If a GameObject is selected, we want to check each of its Components
            // if one or multiple of them have a method with the [PlayModeInspectorMethod] attribute
            if (Selection.activeGameObject != null)
            {
                foreach (var c in Selection.activeGameObject.GetComponents<Component>())
                    UnityEngineObjectEntry.TryCreate(c, m_Entries);
            }

            // If a ScriptableObject is selected, there really is just that one object where
            // a method with the [PlayModeInspectorMethod] attribute can be found.
            if (Selection.activeObject is ScriptableObject)
                UnityEngineObjectEntry.TryCreate(Selection.activeObject, m_Entries);

            Repaint();
        }

        void OnInspectorUpdate()
        {
            // OnInspectorUpdate is called more often than OnGUI if no repaint events are triggered.
            // In order for the play mode inspector to update its UI without user interaction, we use
            // OnInspectorUpdate to trigger repaint events.

            if (!Application.isPlaying)
                return;

            Repaint();
        }

        void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            DrawToolbar();
            EditorGUI.EndDisabledGroup();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("PlayMode Inspector is interactive during play mode only.", MessageType.Info);
                return;
            }

            if (m_Entries == null || m_Entries.Count == 0)
            {
                EditorGUILayout.HelpBox($"No object found with a [PlayModeInspectorMethod] attribute.", MessageType.Info);
                return;
            }

            // If an exception occurred, display a helpbox and early out.
            // This is to avoid causing an exception every frame.
            if (m_ExceptionOccurred)
            {
                EditorGUILayout.HelpBox("An error occurred, please see Console for details.", MessageType.Error);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Ignore"))
                    m_ExceptionOccurred = false;
                EditorGUILayout.EndHorizontal();
                return;
            }

            using (var scrollView = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
            {
                m_ScrollPosition = scrollView.scrollPosition;

                for (var n = 0; n < m_Entries.Count; ++n)
                {
                    var entry = m_Entries[n];

                    EditorGUIUtility.labelWidth = Mathf.Min(250, position.width * 0.3f);
                    EditorGUIUtility.fieldWidth = 0;
                    GUI.enabled = true;
                    GUI.matrix = Matrix4x4.identity;

                    using (new EditorGUILayout.VerticalScope())
                    {
                        var isExpanded = DrawTitlebar(entry);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(18);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                GUILayout.Space(6);

                                try
                                {
                                    if (isExpanded)
                                        entry.Invoke(); // This will actually call the method to draw the GUI
                                }
                                catch (Exception e)
                                {
                                    m_ExceptionOccurred = true;
                                    Debug.LogException(e);
                                }

                                GUILayout.Space(8);
                            }
                        }
                    }
                }

                GUILayout.Space(16);
            }
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var newLock = GUILayout.Toggle(m_Locked, "Lock", EditorStyles.toolbarButton, GUILayout.Width(50));
                if (newLock != m_Locked)
                {
                    m_Locked = newLock;
                    OnSelectionChange();
                    return;
                }

                if (GUILayout.Button("Static...", EditorStyles.toolbarDropDown, GUILayout.Width(60)))
                {
                    ShowStaticMethodPopup();
                    return;
                }

                GUILayout.Space(1);
                GUILayout.FlexibleSpace();

                // Draw button to add a new play mode inspector window
                if (GUILayout.Button(new GUIContent("+", "Add PlayMode Inspector"), EditorStyles.toolbarButton, GUILayout.Width(24)))
                {
                    var wnd = EditorWindow.CreateInstance<PlayModeInspector>();
                    if (wnd != null)
                        wnd.Show();
                }
                GUILayout.Space(1);

                GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            }

            // Draws a separator line as found in all Unity toolbars.
            // TODO: There must be an existing style for it, right?
            var r = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            r.y -= 1;
            var c = GUI.color;
            GUI.color = new Color(0, 0, 0, EditorGUIUtility.isProSkin ? 0.3f : 0.2f);
            GUI.DrawTexture(r, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = c;
        }

        void ShowStaticMethodPopup()
        {
            var menu = new GenericMenu();
            var methods = new List<AbstractEntry>();

            // Find all static methods that are decorated with the PlayModeInspectorMethod attribute.
            foreach (var method in TypeCache.GetMethodsWithAttribute<PlayModeInspectorMethodAttribute>())
            {
                if (!method.IsStatic)
                    continue;

                if (method.DeclaringType.IsGenericType)
                    continue;

                StaticMethodEntry.TryCreate(method, methods);
            }

            // Sort methods, so they appear in a stable order in the menu.
            methods.Sort(delegate (AbstractEntry x, AbstractEntry y)
            {
                return x.title.text.CompareTo(y.title.text);
            });

            // Add each method to the context-menu.
            foreach (var method in methods)
            {
                var title = method.title;

                menu.AddItem(title, false,
                    delegate (object userData)
                    {
                        // If the item is selected, assign it to be inspected.
                        m_Entries = new List<AbstractEntry>();
                        StaticMethodEntry.TryCreate(userData as MethodInfo, m_Entries);
                    },
                    method);
            }

            menu.ShowAsContext();
        }

        // Draws a titlebar with a particular method entry.
        // I tried to mimic the look&feel of the regular Unity Inspector titlebar.
        bool DrawTitlebar(AbstractEntry entry)
        {
            GUIStyle inspectorTitlebar = "IN Title";

            var editorPrefsKey = string.Format("PlayModeInspector.{0}.expanded", entry.editorPrefKey);
            var isExpanded = EditorPrefs.GetBool(editorPrefsKey, true);

            var r = GUILayoutUtility.GetRect(0, 22, GUILayout.ExpandWidth(true));
            r.y -= 1;

            var rtitlebar = r;
            GUI.Box(rtitlebar, "", inspectorTitlebar);

            var e = Event.current;
            if (e != null)
            {
                if (e.type == EventType.MouseDown && e.button == 0 && rtitlebar.Contains(e.mousePosition))
                {
                    EditorPrefs.SetBool(editorPrefsKey, !isExpanded);
                    e.Use();
                }
            }

            var rfoldout = r;
            rfoldout.x += 4; rfoldout.width = 16;
            GUI.Toggle(rfoldout, isExpanded, "", EditorStyles.foldout);

            var title = entry.title;

            var ricon = r;
            ricon.x += 20; ricon.y += 3; ricon.width = 16; ricon.height = 16;
            if (title.image != null)
                GUI.DrawTexture(ricon, AssetPreview.GetMiniThumbnail(title.image));

            var rlabel = r;
            rlabel.x += 38;
            GUI.Button(rlabel, title.text, EditorStyles.boldLabel);

            return isExpanded;
        }

        abstract class AbstractEntry
        {
            public GUIContent title
            {
                get
                {
                    string title;

                    var attribute = m_Method.GetCustomAttribute<PlayModeInspectorMethodAttribute>(true);
                    if (attribute != null && !string.IsNullOrEmpty(attribute.displayName))
                        title = attribute.displayName;
                    else
                        title = string.Format("{0}.{1}", m_Method.DeclaringType.Name, m_Method.Name);

                    if (m_Object != null)
                        title += string.Format(" ({0})", m_Object.name);

                    GUIContent content = new GUIContent(title);

                    if (m_Object != null)
                        content.image = AssetPreview.GetMiniThumbnail(m_Object);
                    else
                        content.image = AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript));

                    return content;
                }
            }

            public string editorPrefKey
            {
                get
                {
                    return string.Format("{0}.{1}", m_Method.DeclaringType.Name, m_Method.Name);
                }
            }

            protected UnityEngine.Object m_Object;
            protected MethodInfo m_Method;

            abstract public void Invoke();

            static protected bool IsMethodValid(MethodInfo method)
            {
                if (method == null)
                    return false;

                // Accept parameterless method only.
                var parameters = method.GetParameters();
                if (parameters == null || parameters.Length != 0)
                    return false;

                // Accept void return type only.
                if (method.ReturnType != typeof(void))
                    return false;

                return true;
            }
        }


        class UnityEngineObjectEntry : AbstractEntry
        {
            public static bool TryCreate(UnityEngine.Object o, List<AbstractEntry> target)
            {
                if (o == null)
                    return false;

                var type = o.GetType();
                foreach(var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<PlayModeInspectorMethodAttribute>();
                    if (attr != null && IsMethodValid(method))
                    {
                        target.Add(new UnityEngineObjectEntry(o, method));
                    }
                }

                return true;
            }

            UnityEngineObjectEntry(UnityEngine.Object o, MethodInfo method)
            {
                m_Object = o;
                m_Method = method;
            }

            public override void Invoke()
            {
                if (m_Method != null)
                    m_Method.Invoke(m_Object, null);
            }
        }

        class StaticMethodEntry : AbstractEntry
        {
            public static bool TryCreate(MethodInfo method, List<AbstractEntry> target)
            {
                if (method == null)
                    return false;

                if (!method.IsStatic)
                    return false;

                if (!IsMethodValid(method))
                    return false;

                target.Add(new StaticMethodEntry(method));
                return true;
            }

            public StaticMethodEntry(MethodInfo o)
            {
                m_Method = o;
            }

            public override void Invoke()
            {
                m_Method.Invoke(null, null);
            }
        }
    }
}
