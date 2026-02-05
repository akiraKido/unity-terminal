using System;
using System.Collections.Generic;
using System.Text;

namespace UnityTerminal.Editor
{
    public sealed class ScreenBuffer
    {
        private readonly List<StringBuilder> _lines = new();
        private readonly int _maxLines;

        public int CursorRow { get; private set; }
        public int CursorCol { get; private set; }

        public ScreenBuffer(int maxLines = 2000)
        {
            _maxLines = Math.Max(100, maxLines);
            _lines.Add(new StringBuilder());
        }

        public void PutChar(char c)
        {
            EnsureCursorRow();

            var line = _lines[CursorRow];
            while (line.Length < CursorCol)
            {
                line.Append(' ');
            }

            if (CursorCol == line.Length)
            {
                line.Append(c);
            }
            else
            {
                line[CursorCol] = c;
            }

            CursorCol++;
        }

        public void CarriageReturn() => CursorCol = 0;

        public void LineFeed()
        {
            CursorRow++;
            EnsureCursorRow();
        }

        public void Backspace()
        {
            if (CursorCol > 0)
            {
                CursorCol--;
            }
        }

        public void MoveCursor(int row, int col)
        {
            CursorRow = Math.Max(0, row);
            CursorCol = Math.Max(0, col);
            EnsureCursorRow();
        }

        public void MoveCursorRelative(int dRow, int dCol)
        {
            MoveCursor(CursorRow + dRow, CursorCol + dCol);
        }

        public void EraseInLine(int mode)
        {
            EnsureCursorRow();
            var line = _lines[CursorRow];
            switch (mode)
            {
                case 0:
                    if (CursorCol < line.Length)
                    {
                        line.Length = CursorCol;
                    }
                    break;
                case 1:
                    var keepFrom = Math.Min(CursorCol, line.Length);
                    var tail = line.ToString(keepFrom, line.Length - keepFrom);
                    line.Clear();
                    for (var i = 0; i < keepFrom; i++)
                    {
                        line.Append(' ');
                    }
                    line.Append(tail);
                    break;
                case 2:
                    line.Clear();
                    break;
            }
        }

        public void EraseInDisplay(int mode)
        {
            switch (mode)
            {
                case 2:
                    _lines.Clear();
                    _lines.Add(new StringBuilder());
                    CursorRow = 0;
                    CursorCol = 0;
                    break;
                case 0:
                    EraseInLine(0);
                    for (var i = CursorRow + 1; i < _lines.Count; i++)
                    {
                        _lines[i].Clear();
                    }
                    break;
            }
        }

        public void ScrollUp(int lines)
        {
            var count = Math.Max(1, lines);
            for (var i = 0; i < count; i++)
            {
                if (_lines.Count > 0)
                {
                    _lines.RemoveAt(0);
                }
                _lines.Add(new StringBuilder());
            }
            CursorRow = Math.Max(0, CursorRow - count);
        }

        public void ScrollDown(int lines)
        {
            var count = Math.Max(1, lines);
            for (var i = 0; i < count; i++)
            {
                _lines.Insert(0, new StringBuilder());
            }
            CursorRow += count;
            TrimToMaxLines();
        }

        public string GetText()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < _lines.Count; i++)
            {
                sb.Append(_lines[i]);
                if (i < _lines.Count - 1)
                {
                    sb.Append('\n');
                }
            }
            return sb.ToString();
        }

        private void EnsureCursorRow()
        {
            while (_lines.Count <= CursorRow)
            {
                _lines.Add(new StringBuilder());
            }
            TrimToMaxLines();
        }

        private void TrimToMaxLines()
        {
            while (_lines.Count > _maxLines)
            {
                _lines.RemoveAt(0);
                CursorRow = Math.Max(0, CursorRow - 1);
            }
        }
    }
}
