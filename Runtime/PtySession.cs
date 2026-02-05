using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace UnityTerminal
{
    public sealed class PtySession : IDisposable
    {
        private readonly ConcurrentQueue<byte[]> _receivedChunks = new();
        private readonly Thread _readerThread;
        private readonly int _handle;
        private volatile bool _running;

        public bool IsRunning => _running;

        public PtySession(string shellPath, string workingDirectory, int cols, int rows)
        {
            _handle = PtyNative.Spawn(shellPath, workingDirectory, cols, rows);
            if (_handle <= 0)
            {
                throw new InvalidOperationException("Failed to spawn PTY shell.");
            }

            _running = true;
            _readerThread = new Thread(ReadLoop)
            {
                IsBackground = true,
                Name = "UnityTerminal.PtySession.ReadLoop"
            };
            _readerThread.Start();
        }

        public bool TryDequeueChunk(out byte[] chunk)
        {
            return _receivedChunks.TryDequeue(out chunk);
        }

        public void WriteUtf8(string text)
        {
            if (!_running || string.IsNullOrEmpty(text))
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(text);
            Write(bytes);
        }

        public void Write(byte[] bytes)
        {
            if (!_running || bytes == null || bytes.Length == 0)
            {
                return;
            }

            PtyNative.Write(_handle, bytes, bytes.Length);
        }

        public void Resize(int cols, int rows)
        {
            if (!_running)
            {
                return;
            }

            PtyNative.Resize(_handle, Math.Max(2, cols), Math.Max(1, rows));
        }

        public void Kill()
        {
            if (!_running)
            {
                return;
            }

            PtyNative.Kill(_handle);
        }

        public void Dispose()
        {
            _running = false;

            if (_readerThread.IsAlive)
            {
                _readerThread.Join(250);
            }

            PtyNative.Close(_handle);
        }

        private void ReadLoop()
        {
            var buffer = new byte[8192];

            while (_running)
            {
                var read = PtyNative.Read(_handle, buffer, buffer.Length);
                if (read > 0)
                {
                    var chunk = new byte[read];
                    Buffer.BlockCopy(buffer, 0, chunk, 0, read);
                    _receivedChunks.Enqueue(chunk);
                    continue;
                }

                if (read == 0)
                {
                    _running = false;
                    break;
                }

                Thread.Sleep(8);
            }
        }
    }
}
