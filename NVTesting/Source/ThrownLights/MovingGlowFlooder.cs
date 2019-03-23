// Nightvision NVTesting MovingGlowFlooder.cs
// 
// 20 03 2019
// 
// 20 03 2019

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Verse;

namespace NVTesting.ThrownLights
{
    public class MovingGlowFlooder
    {
        public static List<WeakReference<MovingGlowFlooder>> ActiveGlowFlooders = new List<WeakReference<MovingGlowFlooder>>();

        public IntVec3 position;

        public float glowRadius;

        public Map map;

        public CellIndices CellIndices;

        public Building[] edifices;

        public ColorInt glowColor;

        private static FastPriorityQueue<int> queue = new FastPriorityQueue<int>(new SortByDist());
        
        private static Dictionary<int, int> activeSet = new Dictionary<int, int>(); 

        private static HashSet<int> closedSetCellIndices = new HashSet<int>();

        private static bool[] blockedCells = new bool[4];

        private int mapSizeX;

        public MovingGlowCells movingCells;

        public MovingGlowFlooder(Map map, IntVec3 position, float glowRadius, ColorInt glowColor)
        {
            this.map = map;
            this.position = position;
            this.glowRadius = glowRadius;
            this.glowColor = glowColor;
            mapSizeX = map.Size.x;
            InitialiseGlow();
            ActiveGlowFlooders.Add(new WeakReference<MovingGlowFlooder>(this));

        }

        private float _attenLinearSlope = 0;
        public float AttenLinearSlope
        {
            get
            {
                if (_attenLinearSlope == 0)
                {
                    _attenLinearSlope = -1f / glowRadius;
                }

                return _attenLinearSlope;
            }
        }

        private Stopwatch _timer = new Stopwatch();
        private int ticks;
        

        public void UpdatePosition(IntVec3 newPosition)
        {
            if (newPosition == position)
            {

                return;
            }

            ticks++;
            _timer.Start();
            int diffX = newPosition.x - position.x;
            int diffZ = newPosition.z - position.z;
            int diffInd = diffX + ((diffZ)* mapSizeX);
            
            position = newPosition;
            Color32[] glowGrid = map.glowGrid.glowGrid;
            int numRows = movingCells.NumRows;
            for (int i = 0; i < numRows; i++)
            {
                List<GlowCell> row = movingCells[i];
                for (int j = 0; j < row.Count; j++)
                {
                    if (row[j] != null)
                    {
                        GlowCell cell = row[j];
                        cell.index += diffInd;
                        if (movingCells[i + diffZ,j + diffX] is GlowCell oldCell)
                        {
                            if (cell.NewCellNeedsUpdate(oldCell))
                            {
                                UpdateColor(cell.index, cell.addedColour, glowGrid, cell.oldGlow);
                            }
                            
                        }
                        else
                        {
                            ApplyColour(cell.index, cell.addedColour, glowGrid, out Color32 orgColor);
                            cell.StoreOldColour(orgColor);
                        }
                    }
                }
            }
            map.mapDrawer.MapMeshDirty(position, MapMeshFlag.GroundGlow);

            _timer.Stop();
            
        }

        public void InitialiseGlow()
        {
            edifices = map.edificeGrid.InnerArray;

            int intRad = Mathf.RoundToInt(glowRadius * 100);

            CellIndices = map.cellIndices;

            int locIndex = CellIndices.CellToIndex(position);
            
            
            movingCells = new MovingGlowCells(glowRadius, mapSizeX);
            
            queue.Clear();
            closedSetCellIndices.Clear();
            activeSet.Clear();


            queue.Push(locIndex);
            activeSet[locIndex] = 100;
            movingCells.Add(new GlowCell(){dist = 100, index = locIndex});

            while (queue.Count != 0)
            {
                int currentCellIndex = queue.Pop();
                IntVec3 cell = CellIndices.IndexToCell(currentCellIndex);

                closedSetCellIndices.Add(currentCellIndex);
                
                
                for (int nInd = 0; nInd < 8; nInd++)
                {
                    uint safex = (uint)(cell.x + (int)directions[nInd, 0]);
                    uint safez = (uint)(cell.z + (int)directions[nInd, 1]);

                    if ((ulong) safex >= (ulong) ((long) map.Size.x) || (ulong) safez >= (ulong) ((long) map.Size.z))
                    {
                        continue;
                    }

                    int x = (int) safex;
                    int z = (int) safez;

                    int nCellIndex = CellIndices.CellToIndex(x, z);
                    if (closedSetCellIndices.Contains(nCellIndex))
                    {
                        continue;
                    }

                    bool blocked = edifices[nCellIndex]?.def.blockLight == true;

                    if (nInd < 4)
                    {
                        blockedCells[nInd] = blocked;
                    }
                    
                    int dist = activeSet[currentCellIndex] + (nInd < 4 ? 100 : 141);
                    
                    if (dist <= intRad)
                    {
                        
                            if (!activeSet.ContainsKey(nCellIndex))
                            {
                                activeSet[nCellIndex] = 99999999;
                            }

                            if (dist < activeSet[nCellIndex])
                            {
                                movingCells.AddOrUpdateCell(nCellIndex, dist);

                                if (blocked)
                                {
                                    
                                    continue;
                                }

                                switch (nInd)
                                {
                                    case 4:

                                        if (blockedCells[0] && blockedCells[1])
                                        {
                                            continue;
                                        }

                                        break;
                                    case 5:

                                        if (blockedCells[1] && blockedCells[2])
                                        {
                                            continue;
                                        }

                                        break;
                                    case 6:

                                        if (blockedCells[2] && blockedCells[3])
                                        {
                                            continue;
                                        }

                                        break;
                                    case 7:

                                        if (blockedCells[0] && blockedCells[3])
                                        {
                                            continue;
                                        }

                                        break;
                                }
                                activeSet[nCellIndex] = dist;
                                queue.Push(nCellIndex);
                            }

                        


                    }



                }


            }

            Log.Message($"movingCells.Count = {movingCells.NumRows * movingCells.NumCols}");
            
            SetUpGlowCells();
            map.mapDrawer.MapMeshDirty(position, MapMeshFlag.GroundGlow);
            Log.Message($"movingCells = {movingCells}");
            

        }

        public void SetUpGlowCells()
        {
            Color32[] glowGrid = map.glowGrid.glowGrid;
            movingCells.Sort();

            foreach (GlowCell glowCell in movingCells)
            {
                int cellindex = glowCell.index;
                int dist      = glowCell.dist;

                float    rad         = (float) dist / 100f;
                ColorInt addedColour = default(ColorInt);

                if (rad <= glowRadius)
                {
                    float b = 1f / (rad * rad);
                    float a = 1f + AttenLinearSlope * rad;
                    
                    float b2 = Mathf.Lerp(a, b, 0.4f);
                    addedColour = glowColor * b2;

                }
                else
                {
                    Log.Message($"No color in cell = {CellIndices.IndexToCell(cellindex).ToString()}");
                    
                }
                
                glowCell.SetNewColour(addedColour.ToColor32);

                if (edifices[cellindex]?.def?.blockLight == true)
                {
                    continue;
                }

                ApplyColour(cellindex, addedColour.ToColor32, glowGrid, out Color32 orgColor);
                glowCell.StoreOldColour(orgColor);
            }

            map.mapDrawer.MapMeshDirty(position, MapMeshFlag.GroundGlow);
        }

        public void RestoreOriginalGlow()
        {
            Color32[] glowGrid = map.glowGrid.glowGrid;

            foreach (GlowCell movingGlowCell in movingCells)
            {
                glowGrid[movingGlowCell.index] = movingGlowCell.oldGlow;
            }
        }

        public void ReapplyGlow()
        {
            Color32[] glowGrid = map.glowGrid.glowGrid;
            
            foreach (GlowCell movingGlowCell in movingCells)
            {
                ApplyColour(movingGlowCell.index, movingGlowCell.addedColour, glowGrid, out Color32 orgColour);
                movingGlowCell.SetNewColour(orgColour);
            }
        }
        


        public void ApplyColour(int cellIndex, Color32 addedColor, Color32[] glowGrid, out Color32 orgColor)
        {

            orgColor = glowGrid[cellIndex];

            if (edifices[cellIndex]?.def.blockLight != true)
            {
                SetGlowGrid(cellIndex, addedColor, glowGrid);
            }

        }

        public void UpdateColor(int cellIndex, Color32 addedColor, Color32[] glowGrid, Color32 orgColor)
        {
            glowGrid[cellIndex] = orgColor;
            if (edifices[cellIndex]?.def.blockLight != true)
            {
                
                SetGlowGrid(cellIndex, addedColor, glowGrid);    
            }
        }

        public void SetGlowGrid(int cellIndex, Color32 addedColor, Color32[] glowGrid)
        {
            
            if (addedColor.r != 0 || addedColor.g != 0 || addedColor.b != 0)
            {
                ColorInt intOrgCol = glowGrid[cellIndex].AsColorInt();
                intOrgCol += addedColor;
                intOrgCol.ClampToNonNegative();

                //Log.Message($"Setting Glow Grid");
                //Log.Message($"intOrgCol. = {intOrgCol.b},{intOrgCol.g},{intOrgCol.r}");
                
                
                //if (rad < this.glower.Props.overlightRadius)
                //{
                //    intOrgCol.a = 1;
                //}
                Color32 toColor = intOrgCol.ToColor32;
                glowGrid[cellIndex] = toColor;
            }
        }

        public void OnDestroy()
        {
            RestoreOriginalGlow();
            map.mapDrawer.MapMeshDirty(position, MapMeshFlag.GroundGlow);

            Log.Message(new string('-', 20));
            Log.Message($"MovingGlowFlooder");
            Log.Message($"OnDestroy");
            Log.Message($"updates = {ticks}");
            Log.Message($"ms = {_timer.Elapsed.TotalMilliseconds}");
            Log.Message($"av. ms per tick = {_timer.Elapsed.TotalMilliseconds / ticks }");

            Log.Message($"section ms = {LightingOverlayRegenerate_Patch.Timer.Elapsed.TotalMilliseconds}");
            Log.Message($"section av. ms per tick = {LightingOverlayRegenerate_Patch.Timer.Elapsed.TotalMilliseconds / ticks}");
            
            LightingOverlayRegenerate_Patch.Timer.Reset();
            _timer.Reset();
            ticks = 0;
            Log.Message(new string('-', 20));
        }

        public const int standardSectionDim = 17;
        public void SectionVertIndicesFromCellIndex(int cellIndex, int sectWidth, out int botLeft, out int botRight, out int topLeft, out int topRight, out int centre)
        {
            int sectX = (cellIndex % mapSizeX) % standardSectionDim;
            int sectZ = (cellIndex / mapSizeX) % standardSectionDim;

            botLeft = sectX + (sectZ * (sectWidth + 1));
            botRight = botLeft + 1;
            topLeft = botLeft + sectWidth + 1;
            topRight = topLeft + 1;
            centre = (sectWidth + 1) * (sectWidth + 1) + (sectZ * sectWidth) + sectX;
        }
        

        public int SectionID(int cellIndex)
        {
            return ((cellIndex % mapSizeX) / standardSectionDim) + (((cellIndex % mapSizeX) / standardSectionDim) * (mapSizeX / standardSectionDim));
        }

        public Dictionary<int, SectionLayer_LightingOverlay> sections;
        
        
        public class SortByDist : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return activeSet[x].CompareTo(activeSet[y]);
            }
        }

        
        

        public sbyte[,] directions = {{0, -1}, {1, 0}, {0, 1}, {-1, 0}, {1, -1}, {1, 1}, {-1, 1}, {-1, -1}};
    }
}