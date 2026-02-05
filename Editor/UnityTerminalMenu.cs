using UnityEditor;

namespace UnityTerminal.Editor
{
    public static class UnityTerminalMenu
    {
        [MenuItem("Tools/Unity Terminal/About")]
        private static void ShowAbout()
        {
            EditorUtility.DisplayDialog(
                "Unity Terminal",
                "Unity Terminal editor extension is initialized. Open Tools > Unity Terminal > Open Terminal to launch the PTY view.",
                "OK"
            );
        }
    }
}
