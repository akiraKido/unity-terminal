using UnityEditor;
using UnityEngine;

namespace UnityTerminal.Editor
{
    public static class UnityTerminalMenu
    {
        [MenuItem("Tools/Unity Terminal/About")]
        private static void ShowAbout()
        {
            EditorUtility.DisplayDialog(
                "Unity Terminal",
                "Unity Terminal editor extension is initialized.",
                "OK"
            );
        }
    }
}
