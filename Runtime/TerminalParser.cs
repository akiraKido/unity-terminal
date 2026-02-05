using System;
using System.Collections.Generic;
using System.Text;

namespace UnityTerminal
{
    public sealed class TerminalParser
    {
        private enum ParseState
        {
            Text,
            Escape,
            Csi,
            Osc
        }

        private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
        private readonly StringBuilder _pendingText = new();
        private readonly StringBuilder _controlBuffer = new();
        private ParseState _state = ParseState.Text;

        public void ProcessBytes(byte[] bytes, ScreenBuffer buffer)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return;
            }

            var charBuffer = new char[Encoding.UTF8.GetMaxCharCount(bytes.Length)];
            var charsDecoded = _decoder.GetChars(bytes, 0, bytes.Length, charBuffer, 0, flush: false);
            for (var i = 0; i < charsDecoded; i++)
            {
                ProcessChar(charBuffer[i], buffer);
            }
        }

        private void ProcessChar(char c, ScreenBuffer buffer)
        {
            switch (_state)
            {
                case ParseState.Text:
                    if (c == 0x1b)
                    {
                        FlushText(buffer);
                        _state = ParseState.Escape;
                    }
                    else
                    {
                        _pendingText.Append(c);
                    }
                    break;

                case ParseState.Escape:
                    if (c == '[')
                    {
                        _controlBuffer.Clear();
                        _state = ParseState.Csi;
                    }
                    else if (c == ']')
                    {
                        _controlBuffer.Clear();
                        _state = ParseState.Osc;
                    }
                    else
                    {
                        _state = ParseState.Text;
                    }
                    break;

                case ParseState.Csi:
                    if ((c >= '@' && c <= '~') || c == 'm')
                    {
                        _controlBuffer.Append(c);
                        HandleCsi(_controlBuffer.ToString(), buffer);
                        _controlBuffer.Clear();
                        _state = ParseState.Text;
                    }
                    else
                    {
                        _controlBuffer.Append(c);
                    }
                    break;

                case ParseState.Osc:
                    if (c == '\a')
                    {
                        _state = ParseState.Text;
                    }
                    else if (c == 0x1b)
                    {
                        _state = ParseState.Escape;
                    }
                    break;
            }

            if (_state == ParseState.Text)
            {
                FlushText(buffer);
            }
        }

        private void FlushText(ScreenBuffer buffer)
        {
            if (_pendingText.Length == 0)
            {
                return;
            }

            for (var i = 0; i < _pendingText.Length; i++)
            {
                var c = _pendingText[i];
                switch (c)
                {
                    case '\r':
                        buffer.CarriageReturn();
                        break;
                    case '\n':
                        buffer.LineFeed();
                        break;
                    case '\b':
                        buffer.Backspace();
                        break;
                    default:
                        buffer.PutChar(c);
                        break;
                }
            }

            _pendingText.Clear();
        }

        private static void HandleCsi(string sequence, ScreenBuffer buffer)
        {
            if (string.IsNullOrEmpty(sequence))
            {
                return;
            }

            var cmd = sequence[^1];
            var paramText = sequence.Substring(0, sequence.Length - 1);
            var parameters = ParseParams(paramText);

            switch (cmd)
            {
                case 'A':
                    buffer.MoveCursorRelative(-GetParam(parameters, 0, 1), 0);
                    break;
                case 'B':
                    buffer.MoveCursorRelative(GetParam(parameters, 0, 1), 0);
                    break;
                case 'C':
                    buffer.MoveCursorRelative(0, GetParam(parameters, 0, 1));
                    break;
                case 'D':
                    buffer.MoveCursorRelative(0, -GetParam(parameters, 0, 1));
                    break;
                case 'G':
                    buffer.MoveCursor(buffer.CursorRow, Math.Max(0, GetParam(parameters, 0, 1) - 1));
                    break;
                case 'H':
                case 'f':
                    buffer.MoveCursor(
                        Math.Max(0, GetParam(parameters, 0, 1) - 1),
                        Math.Max(0, GetParam(parameters, 1, 1) - 1));
                    break;
                case 'J':
                    buffer.EraseInDisplay(GetParam(parameters, 0, 0));
                    break;
                case 'K':
                    buffer.EraseInLine(GetParam(parameters, 0, 0));
                    break;
                case 'S':
                    buffer.ScrollUp(GetParam(parameters, 0, 1));
                    break;
                case 'T':
                    buffer.ScrollDown(GetParam(parameters, 0, 1));
                    break;
                case 'm':
                    // MVP: SGR attributes are ignored for now.
                    break;
            }
        }

        private static int GetParam(List<int> parameters, int index, int fallback)
        {
            if (index >= parameters.Count)
            {
                return fallback;
            }

            return parameters[index] <= 0 ? fallback : parameters[index];
        }

        private static List<int> ParseParams(string text)
        {
            var values = new List<int>();
            if (string.IsNullOrEmpty(text))
            {
                return values;
            }

            var split = text.Split(';');
            for (var i = 0; i < split.Length; i++)
            {
                if (int.TryParse(split[i], out var value))
                {
                    values.Add(value);
                }
                else
                {
                    values.Add(0);
                }
            }

            return values;
        }
    }
}
