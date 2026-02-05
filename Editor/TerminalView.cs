using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityTerminal.Editor
{
    public sealed class TerminalView : VisualElement
    {
        private readonly Label _outputLabel;
        private readonly ScreenBuffer _buffer;
        private readonly TerminalParser _parser;
        private PtySession _session;
        private int _lastCols = -1;
        private int _lastRows = -1;

        public new class UxmlFactory : UxmlFactory<TerminalView, UxmlTraits> { }

        public TerminalView()
        {
            style.flexGrow = 1;
            focusable = true;

            _buffer = new ScreenBuffer();
            _parser = new TerminalParser();

            var scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    backgroundColor = new Color(0.08f, 0.08f, 0.08f)
                }
            };
            _outputLabel = new Label
            {
                style =
                {
                    unityFont = Font.CreateDynamicFontFromOSFont("Menlo", 14),
                    whiteSpace = WhiteSpace.PreWrap,
                    color = new Color(0.85f, 0.85f, 0.85f),
                    unityTextAlign = TextAnchor.UpperLeft
                }
            };

            scroll.Add(_outputLabel);
            Add(scroll);

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                schedule.Execute(Tick).Every(16);
            });
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void StartSession(string shellPath, string workingDirectory, int cols, int rows)
        {
            StopSession();
            _session = new PtySession(shellPath, workingDirectory, cols, rows);
            _lastCols = cols;
            _lastRows = rows;
            Focus();
        }

        public void StopSession()
        {
            if (_session == null)
            {
                return;
            }

            _session.Dispose();
            _session = null;
        }

        private void Tick()
        {
            if (_session == null)
            {
                return;
            }

            var didUpdate = false;
            while (_session.TryDequeueChunk(out var chunk))
            {
                _parser.ProcessBytes(chunk, _buffer);
                didUpdate = true;
            }

            if (didUpdate)
            {
                _outputLabel.text = _buffer.GetText();
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (_session == null)
            {
                return;
            }

            const float cellWidth = 8.4f;
            const float cellHeight = 18.0f;
            var cols = Mathf.Max(2, Mathf.FloorToInt(contentRect.width / cellWidth));
            var rows = Mathf.Max(1, Mathf.FloorToInt(contentRect.height / cellHeight));
            if (cols == _lastCols && rows == _lastRows)
            {
                return;
            }

            _lastCols = cols;
            _lastRows = rows;
            _session.Resize(cols, rows);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (_session == null)
            {
                return;
            }

            if (evt.ctrlKey)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.C:
                        _session.Write(new byte[] { 0x03 });
                        evt.StopPropagation();
                        return;
                    case KeyCode.D:
                        _session.Write(new byte[] { 0x04 });
                        evt.StopPropagation();
                        return;
                    case KeyCode.Z:
                        _session.Write(new byte[] { 0x1A });
                        evt.StopPropagation();
                        return;
                }
            }

            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:
                    _session.WriteUtf8("\u001b[A");
                    evt.StopPropagation();
                    return;
                case KeyCode.DownArrow:
                    _session.WriteUtf8("\u001b[B");
                    evt.StopPropagation();
                    return;
                case KeyCode.RightArrow:
                    _session.WriteUtf8("\u001b[C");
                    evt.StopPropagation();
                    return;
                case KeyCode.LeftArrow:
                    _session.WriteUtf8("\u001b[D");
                    evt.StopPropagation();
                    return;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    _session.WriteUtf8("\r");
                    evt.StopPropagation();
                    return;
                case KeyCode.Backspace:
                    _session.Write(new byte[] { 0x7f });
                    evt.StopPropagation();
                    return;
                case KeyCode.Space:
                    _session.WriteUtf8(" ");
                    evt.StopPropagation();
                    return;
            }

            if (!string.IsNullOrEmpty(evt.character.ToString()) && !char.IsControl(evt.character))
            {
                _session.WriteUtf8(evt.character.ToString());
                evt.StopPropagation();
            }
        }
    }
}
