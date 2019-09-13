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

        public static Dictionary<int, Color32[]> originalGlowGrids = new Dictionary<int, Color32[]>();

        public MovingGlowCells movingCells;

        public Mesh mesh;

        public Material mat;

        public MovingGlowFlooder(Map map, IntVec3 position, float glowRadius, ColorInt glowColor)
        {
            this.map = map;
            this.position = position;
            this.glowRadius = glowRadius;
            this.glowColor = glowColor;
            mapSizeX = map.Size.x;
            mat = new Material(MatBases.LightOverlay);
            mat.color = Color.white;

            if (!originalGlowGrids.ContainsKey(map.uniqueID))
            {
                Color32[] originalGlowgrid = new Color32[map.glowGrid.glowGrid.Length];
                map.glowGrid.glowGrid.CopyTo(originalGlowgrid, 0);
                originalGlowGrids[map.uniqueID] = originalGlowgrid;

            }

            InitialiseGlow();
            ActiveGlowFlooders.Add(new WeakReference<MovingGlowFlooder>(this));

        }

        public static float altitude = AltitudeLayer.LightingOverlay.AltitudeFor() - 0.02f;
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
        private bool loggedDraw;
        public void Draw(Vector3 exactPos)
        {
            if (!loggedDraw)
            {
                Log.Message($"Drawing");
                loggedDraw = true;
            }

            exactPos.y = altitude;
            Graphics.DrawMesh(mesh, exactPos + offset, Quaternion.identity, mat, 0);
        }

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
            //map.mapDrawer.MapMeshDirty(position, MapMeshFlag.GroundGlow);

            _timer.Stop();
            
        }

        private const int baseDist = 100;
        private const int strDist = 100;
        private const int diagDist = 141;
        private static sbyte[,] rotT = new sbyte[4,2]
                                                     {
                                                         {1, 1},
                                                         {-1,1},
                                                         {1,-1},
                                                         {-1,-1}
                                                     };
        public MovingGlowCells GenerateBaseCells(float radius, int mapXSize, int baseIndex)
        {
            var result = new MovingGlowCells(radius, mapXSize);
            int intRad = Mathf.RoundToInt(radius * 100);
            int maxCount = intRad / 100;
            for (int x = 0; x <= maxCount; x++)
            {
                int distAlongDiag = baseDist + (x * diagDist);
                for (int z = x; z <= maxCount; z++)
                {
                    int sumDist = distAlongDiag + (strDist * (z - x));

                    if (sumDist <= intRad)
                    {
                        if (x == 0 && z == 0)
                        {
                            result.AddNew(baseIndex, sumDist);
                        }
                        else if (x == 0)
                        {
                            result.AddNew((z * mapXSize) + baseIndex, sumDist );
                            result.AddNew(z + baseIndex, sumDist );
                            result.AddNew((-z * mapXSize) + baseIndex, sumDist );
                            result.AddNew(-z + baseIndex , sumDist );
                        }
                        else if (x==z)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                result.AddNew((rotT[i,0] * x) + (rotT[i,1] * z * mapXSize) + baseIndex, sumDist );
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                result.AddNew((rotT[i,0] * x) + (rotT[i,1] * z * mapXSize) + baseIndex, sumDist );
                                result.AddNew((rotT[i,0] * z) + (rotT[i,1] * x * mapXSize) + baseIndex, sumDist );
                            }
                        }
                    }
                }
            }

            result.Sort();

            return result;
        }


        public void InitialiseGlow()
        {
            edifices = map.edificeGrid.InnerArray;

            int intRad = Mathf.RoundToInt(glowRadius);

            CellIndices = map.cellIndices;

            int locIndex = CellIndices.CellToIndex(position);
            
            movingCells = GenerateBaseCells(glowRadius, mapSizeX, locIndex);
            
            movingCells.Sort();
            SetUpGlowCells();
            Log.Message($"movingCells = {movingCells.ToStringColour()}");
            //map.mapDrawer.MapMeshDirty(position, MapMeshFlag.GroundGlow);
            mesh = DynamicLightingOverlay.MakeNewMesh(movingCells, locIndex, intRad,glowColor, offset:out offset);
            
            //mesh = DynamicLightingOverlay.MakeTestCircle(out offset, mat);
        }

        public Vector3 offset;



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