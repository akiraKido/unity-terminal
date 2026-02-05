using System;
using System.Runtime.InteropServices;

namespace UnityTerminal
{
    internal static class PtyNative
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string LibraryName = "pty_bridge";
#else
        private const string LibraryName = "__Internal";
#endif

        [DllImport(LibraryName, EntryPoint = "pty_spawn")]
        internal static extern int Spawn(
            string shellPath,
            string cwd,
            int cols,
            int rows);

        [DllImport(LibraryName, EntryPoint = "pty_write")]
        internal static extern int Write(
            int handle,
            byte[] bytes,
            int len);

        [DllImport(LibraryName, EntryPoint = "pty_read")]
        internal static extern int Read(
            int handle,
            byte[] outBuffer,
            int cap);

        [DllImport(LibraryName, EntryPoint = "pty_resize")]
        internal static extern int Resize(
            int handle,
            int cols,
            int rows);

        [DllImport(LibraryName, EntryPoint = "pty_kill")]
        internal static extern int Kill(int handle);

        [DllImport(LibraryName, EntryPoint = "pty_close")]
        internal static extern int Close(int handle);
    }
}
