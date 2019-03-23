// Nightvision NVTesting MovingGlowCells.cs
// 
// 22 03 2019
// 
// 22 03 2019

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace NVTesting.ThrownLights
{
    public class MovingGlowCells
    {
        public List<List<GlowCell>> matrix;

        public int mapWidth;

        public MovingGlowCells(float radius, int mapWidth)
        {
            expectedDim = (int) (radius * 2) - 1;
            matrix = new List<List<GlowCell>>(expectedDim);
            this.mapWidth = mapWidth;
        }

        private int expectedDim;

        public void AddOrUpdateCell(int cellIndex, int dist)
        {
            GlowCell cell = matrix.Select<List<GlowCell>, GlowCell>(gcl => gcl.Find(gc => gc.index == cellIndex)).First();

            if (cell == null)
            {
                cell = new GlowCell(){index = cellIndex, dist = dist};
                Add(cell);
            }
            else
            {
                cell.dist = dist;
            }
        }



        public void Add(GlowCell newCell)
        {
            foreach (List<GlowCell> glowCells in matrix)
            {
                if (glowCells.Count == 0)
                {
                    continue;
                }

                if (newCell.index / mapWidth == glowCells[0].index / mapWidth)
                {
                    glowCells.Add(newCell);
                    return;
                }
            }
            matrix.Add(new List<GlowCell>(expectedDim){newCell});
        }

        public void Sort()
        {
            if (sorted)
            {
                return;
            }
            int maxLength = 0;
            for (int i = matrix.Count - 1; i >= 0; i--)
            {
                if (matrix[i] == null || matrix[i].Count == 0)
                {
                    matrix.RemoveAt(i);
                }
                else
                {
                    matrix[i].Sort(new SortByCellIndex());

                    if (matrix[i].Count > maxLength)
                    {
                        maxLength = matrix[i].Count;
                    }
                }
            }
            matrix.Sort(new SortByRowIndex());

            foreach (List<GlowCell> glowCells in matrix)
            {
                int val = maxLength - glowCells.Count;

                if (val > 0)
                {
                    val /= 2;

                    if (val == 0)
                    {
                        Log.Message($"Sort: val = {val}");
                        
                    }

                    for (int i = 0; i < val; i++)
                    {
                        glowCells.Insert(0, null);
                        glowCells.Add(null);
                    }
                }
            }

            sorted = true;
        }



        public class SortByCellIndex : IComparer<GlowCell>
        {
            public int Compare(GlowCell x, GlowCell y)
            {
                return x.index.CompareTo(y.index);
            }
        }

        public class SortByRowIndex : IComparer<List<GlowCell>>
        {
            public int Compare(List<GlowCell> x, List<GlowCell> y)
            {
                return x[0].index.CompareTo(y[0].index);
            }
        }

        public bool sorted;
        private const char space = '\u2007';
        private const int colWidth = 6;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"Moving glow cell matrix: h ={matrix.Count}\n");

            foreach (List<GlowCell> glowCells in matrix)
            {
                foreach (GlowCell glowCell in glowCells)
                {
                    if (glowCell != null)
                    {
                        sb.Append(($"{(glowCell.dist.ToString().PadLeft(5, space))}{space}"));
                    }
                    else
                    {
                        sb.Append(new string(space, colWidth));
                    }
                }

                sb.AppendLine();
            }
            return sb.ToString();
        }

        public int NumRows => matrix.Count;

        public int NumCols
        {
            get
            {
                Sort();
                return matrix[0].Count;
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
                return matrix[i];
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
                    return matrix[i][j];
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Log.Message($"ActualValue = {e.ActualValue.ToString()}");
                    
                    Log.Message($"Invalid [i,j] = [{i},{j}]");
                    Log.Message($"[NumCols,NumRows] = [{NumCols},{NumRows}]");
                    
                    
                    

                    throw;
                }
                
                
            }
        }

        public GlowCellsEnumerator GetEnumerator()
        {
            Sort();
            return new GlowCellsEnumerator(this);
        }

        public class GlowCellsEnumerator
        {
            private int rowIndex;
            private int colIndex;

            public GlowCellsEnumerator(MovingGlowCells parent)
            {
                glowCells = parent;
            }

            private MovingGlowCells glowCells;

            public GlowCell Current => glowCells[rowIndex][colIndex];

            public bool MoveNext()
            {
                colIndex++;

                if (colIndex >= glowCells[rowIndex].Count)
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