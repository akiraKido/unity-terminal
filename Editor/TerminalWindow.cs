using UnityEditor;
using UnityEngine;

namespace UnityTerminal.Editor
{
    public sealed class TerminalWindow : EditorWindow
    {
        private TerminalView _terminalView;

        [MenuItem("Tools/Unity Terminal/Open Terminal")]
        public static void ShowWindow()
        {
            var window = GetWindow<TerminalWindow>();
            window.titleContent = new GUIContent("Unity Terminal");
            window.Show();
        }

        private void CreateGUI()
        {
            _terminalView = new TerminalView();
            rootVisualElement.Add(_terminalView);

#if UNITY_EDITOR_OSX
            var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            _terminalView.StartSession("/bin/zsh", home, 120, 40);
#else
            rootVisualElement.Add(new UnityEngine.UIElements.HelpBox("PTY bridge is currently implemented for macOS Editor.", UnityEngine.UIElements.HelpBoxMessageType.Info));
#endif
        }

        private void OnDisable()
        {
            _terminalView?.StopSession();
        }
    }
}
