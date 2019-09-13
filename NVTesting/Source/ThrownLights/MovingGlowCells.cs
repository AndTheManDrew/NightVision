// Nightvision NVTesting MovingGlowCells.cs
// 
// 22 03 2019
// 
// 22 03 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace NVTesting.ThrownLights
{
    public class MovingGlowCells
    {
        private const char space = '\u2007';
        private const int colWidth = 6;
        public List<List<GlowCell>> matrix;
        private List<RowInfo> rowInfo;
        public int mapWidth;
        public bool sorted;
        private readonly int expectedDim;


        public int NumRows => matrix.Count;

        public int NumCols
        {
            get
            {
                Sort();

                return matrix[index: 0].Count;
            }
        }

        public List<GlowCell> this[int i]
        {
            get
            {
                if (i >= NumRows)
                {
                    return null;
                }

                return matrix[index: i];
            }
        }

        public GlowCell this[int i, int j]
        {
            get
            {
                if (i >= NumRows || i < 0 || j >= NumCols || j < 0)
                {
                    return null;
                }

                try
                {
                    return matrix[index: i][index: j];
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log.Message(text: $"ActualValue = {e.ActualValue}");
                    Log.Message(text: $"Invalid [i,j] = [{i},{j}]");
                    Log.Message(text: $"[NumCols,NumRows] = [{NumCols},{NumRows}]");
                    throw;
                }
            }
        }

        public MovingGlowCells(float radius, int mapWidth)
        {
            expectedDim   = (int) (radius * 2) - 1;
            matrix        = new List<List<GlowCell>>(capacity: expectedDim);
            this.mapWidth = mapWidth;
        }


        public void AddOrUpdateCell(int cellIndex, int dist)
        {
            GlowCell cell = matrix.Select(selector: gcl => gcl.Find(match: gc => gc.index == cellIndex)).First();

            if (cell == null)
            {
                cell = new GlowCell {index = cellIndex, dist = dist};
                Add(newCell: cell);
            }
            else
            {
                cell.dist = dist;
            }
        }

        public void AddNew(int cellIndex, int dist)
        {
            Add(newCell: new GlowCell {dist = dist, index = cellIndex});
        }

        public void Add(GlowCell newCell)
        {
            foreach (List<GlowCell> glowCells in matrix)
            {
                if (glowCells.Count == 0)
                {
                    continue;
                }

                if (newCell.index / mapWidth == glowCells[index: 0].index / mapWidth)
                {
                    glowCells.Add(item: newCell);

                    return;
                }
            }

            matrix.Add(item: new List<GlowCell>(capacity: expectedDim) {newCell});
        }

        public void Sort()
        {
            if (sorted)
            {
                return;
            }

            var maxLength = 0;

            for (int i = matrix.Count - 1; i >= 0; i--)
            {
                if (matrix[index: i] == null || matrix[index: i].Count == 0)
                {
                    matrix.RemoveAt(index: i);
                }
                else
                {
                    matrix[index: i].Sort(comparer: new SortByCellIndex());

                    if (matrix[index: i].Count > maxLength)
                    {
                        maxLength = matrix[index: i].Count;
                    }
                }
            }

            matrix.Sort(comparer: new SortByRowIndex());
            rowInfo = new List<RowInfo>(matrix.Count);
            for (var row = 0; row < matrix.Count; row++)
            {
                List<GlowCell> glowCells = matrix[row];
                int numValid = glowCells.Count;
                
                int            val       = maxLength - glowCells.Count;

                if (val > 0)
                {
                    val /= 2;

                    if (val == 0)
                    {
                        Log.Message(text: $"Sort: val = {val}");
                    }

                    for (var i = 0; i < val; i++)
                    {
                        glowCells.Insert(index: 0, item: null);
                        glowCells.Add(item: null);
                    }
                }

                rowInfo.Add(new RowInfo(){numValid = glowCells.Count, firstIndex = val});
            }

            sorted = true;
        }


        public override string ToString()
        {
            var sb = new StringBuilder(value: $"Moving glow cell matrix: h ={matrix.Count}\n");

            foreach (List<GlowCell> glowCells in matrix)
            {
                foreach (GlowCell glowCell in glowCells)
                {
                    if (glowCell != null)
                    {
                        sb.Append(value: $"{glowCell.dist.ToString().PadLeft(totalWidth: 5, paddingChar: space)}{space}");
                    }
                    else
                    {
                        sb.Append(value: new string(c: space, count: colWidth));
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ToStringIndices()
        {
            var sb = new StringBuilder(value: $"Moving glow cell matrix: h ={matrix.Count}\n");

            foreach (List<GlowCell> glowCells in matrix)
            {
                foreach (GlowCell glowCell in glowCells)
                {
                    if (glowCell != null)
                    {
                        sb.Append(value: $"{glowCell.index.ToString().PadLeft(totalWidth: 5, paddingChar: space)}{space}");
                    }
                    else
                    {
                        sb.Append(value: new string(c: space, count: colWidth));
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ToStringColour()
        {
            var sb = new StringBuilder(value: $"Moving glow cell matrix: h ={matrix.Count}\n");

            foreach (List<GlowCell> glowCells in matrix)
            {
                foreach (GlowCell glowCell in glowCells)
                {
                    if (glowCell != null)
                    {
                        sb.Append(value: $"{glowCell.addedColour.ToString().PadLeft(totalWidth: 26, paddingChar: space)}{space}");
                    }
                    else
                    {
                        sb.Append(value: new string(c: space, count: 26));
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public GlowCellsSafeEnumerator GetEnumerator()
        {
            Sort();

            return new GlowCellsSafeEnumerator(parent: this);
        }

        public struct RowInfo
        {
            public int numValid;
            public int firstIndex;

            public void SetFirstIndex(int newVal)
            {
                firstIndex = newVal;
            }
        }

        public class SortByCellIndex : IComparer<GlowCell>
        {
            public int Compare(GlowCell x, GlowCell y)
            {
                return x.index.CompareTo(value: y.index);
            }
        }

        public class SortByRowIndex : IComparer<List<GlowCell>>
        {
            public int Compare(List<GlowCell> x, List<GlowCell> y)
            {
                return x[index: 0].index.CompareTo(value: y[index: 0].index);
            }
        }

        public class GlowCellsSafeEnumerator
        {
            private int rowIndex;
            private int colIndex;

            public GlowCellsSafeEnumerator(MovingGlowCells parent)
            {
                glowCells = parent;
            }

            private readonly MovingGlowCells glowCells;

            public GlowCell Current => glowCells[i: rowIndex][index: colIndex];

            public bool MoveNext()
            {
                colIndex++;

                if (colIndex >= glowCells[i: rowIndex].Count)
                {
                    rowIndex++;
                    colIndex = 0;

                    if (rowIndex >= glowCells.NumRows)
                    {
                        return false;
                    }
                }

                if (Current == null)
                {
                    return MoveNext();
                }


                return true;
            }
        }
    }
}