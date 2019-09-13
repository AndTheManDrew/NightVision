// Nightvision NVTesting DynamicLightingOverlay.cs
// 
// 25 03 2019
// 
// 25 03 2019

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NVTesting.ThrownLights
{
    public class DynamicLightingOverlay
    {
        private CellRect sectRect;
        private int firstCentreInd;

        public ColorShort[] movingGlowGrid;

        //public void MakeBaseGeometry(LayerSubMesh subMesh)
        //{
        //    sectRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
        //    sectRect.ClipInsideMap(Map);
        //    int capacity = (sectRect.Width + 1) * (sectRect.Height + 1) + sectRect.Area;
        //    float y = AltitudeLayer.LightingOverlay.AltitudeFor();
        //    subMesh.verts.Capacity = capacity;

        //    for (int worldZ = sectRect.minZ; worldZ < sectRect.maxZ; worldZ++)
        //    {
        //        for (int worldX = sectRect.minX; worldX < sectRect.maxX; worldX++)
        //        {
        //            subMesh.verts.Add(new Vector3(worldX, y, worldZ));
        //        }
        //    }
        //    firstCentreInd = subMesh.verts.Count;
        //    for (int worldZ = this.sectRect.minZ; worldZ <= this.sectRect.maxZ; worldZ++)
        //    {
        //        for (int worldX = this.sectRect.minX; worldX <= this.sectRect.maxX; worldX++)
        //        {
        //            subMesh.verts.Add(new Vector3((float)worldX + 0.5f, y, (float)worldZ + 0.5f));
        //        }
        //    }
        //    subMesh.tris.Capacity = this.sectRect.Area * 4 * 3;
        //    for (int worldZ = this.sectRect.minZ; worldZ <= this.sectRect.maxZ; worldZ++)
        //    {
        //        for (int worldX = this.sectRect.minX; worldX <= this.sectRect.maxX; worldX++)
        //        {
        //            this.CalculateVertexIndices(worldX, worldZ, out int botLeft, out int topLeft, out int topRight, out int botRight, out int centre);
        //            subMesh.tris.Add(botLeft);
        //            subMesh.tris.Add(centre);
        //            subMesh.tris.Add(botRight);
        //            subMesh.tris.Add(botLeft);
        //            subMesh.tris.Add(topLeft);
        //            subMesh.tris.Add(centre);
        //            subMesh.tris.Add(topLeft);
        //            subMesh.tris.Add(topRight);
        //            subMesh.tris.Add(centre);
        //            subMesh.tris.Add(topRight);
        //            subMesh.tris.Add(botRight);
        //            subMesh.tris.Add(centre);
        //        }
        //    }

        //}
         public static Mesh MakeTestCircle(out Vector3 offset, Material mat)
        {
            int rowL = testColors.Length - 1;
            var mesh = new Mesh();
            float y = 0;
            List<Vector3> verts = new List<Vector3>();
            List<Color32> colours = new List<Color32>();
            List<int> tris = new List<int>();
            offset = Vector3.zero;
            var baseColor = mat.color;
            var glowColor = Color.green;
            float tBase = rowL / 2.0f;
            for (int z = 0; z < rowL; z++)
            {
                for (int x = 0; x < rowL; x++)
                {
                    int initial = verts.Count;
                    Color32 centreColor = Color32.Lerp(baseColor, glowColor, (tBase - (float)Math.Sqrt((x-tBase)*(x-tBase) + (z-tBase)*(z-tBase)))/tBase);
                    verts.Add(new Vector3(x * 4, y, z * 4));
                    verts.Add(new Vector3((x+1f) * 4, y, z * 4));
                    verts.Add(new Vector3(x * 4, y, (z + 1f) * 4));
                    verts.Add(new Vector3((x+1f) * 4, y, (z+1f) * 4));
                    verts.Add(new Vector3((x +0.5f) * 4, y, (z+0.5f) * 4));
                    var tempColor = centreColor;
                    tempColor.a = 0;
                    colours.Add(tempColor);
                    colours.Add(tempColor);
                    colours.Add(tempColor);
                    colours.Add(tempColor);
                    colours.Add(tempColor);

                    tris.Add(initial);
                    tris.Add(initial + 4);
                    tris.Add(initial+1);

                    tris.Add(initial);
                    tris.Add(initial+2);
                    tris.Add(initial+4);

                    tris.Add(initial+2);
                    tris.Add(initial+3);
                    tris.Add(initial+4);

                    tris.Add(initial+3);
                    tris.Add(initial+1);
                    tris.Add(initial+4);

                }
            }
            

            mesh.SetVertices(verts);
            mesh.colors32 = colours.ToArray();
            mesh.SetTriangles(tris, 0);
            //mesh.SetColors(colours);

            return mesh;

        }

        public static Color32[] testColors = new Color32[] {Color.clear, Color.white,Color.magenta, Color.red, Color.yellow, Color.cyan, Color.green, Color.blue};
        public static Mesh MakeTestPalette(out Vector3 offset, Material mat)
        {
            int rowL = testColors.Length - 1;
            var mesh = new Mesh();
            float y = 0;
            List<Vector3> verts = new List<Vector3>();
            List<Color32> colours = new List<Color32>();
            List<int> tris = new List<int>();
            offset = Vector3.zero;
            var testColor = mat.color;
            //float t = rowL / 2.0f;
            for (int z = 0; z < rowL; z++)
            {
                float zCoord = z + 0.1f;
                Color32 testColor2 = testColors[z];
                testColor2.r = (byte) (testColor2.r * 200 / 255);
                testColor2.g = (byte) (testColor2.g * 200 / 255);
                testColor2.b = (byte) (testColor2.b * 200 / 255);
                testColor2.a = 100;
                for (int x = 0; x < rowL; x++)
                {
                    int initial = verts.Count;
                    float xCoord = x + 0.1f;

                    Color32 centreColor = Color32.Lerp(testColor, testColor2, (float)x / rowL);
                    verts.Add(new Vector3(xCoord * 4, y, zCoord * 4));
                    verts.Add(new Vector3((xCoord+0.8f) * 4, y, zCoord * 4));
                    verts.Add(new Vector3(xCoord * 4, y, (zCoord + 0.8f) * 4));
                    verts.Add(new Vector3((xCoord+0.8f) * 4, y, (zCoord+0.8f) * 4));
                    verts.Add(new Vector3((xCoord +0.4f) * 4, y, (zCoord+0.4f) * 4));
                    var tempColor = centreColor;
                    tempColor.a = 0;
                    colours.Add(tempColor);
                    colours.Add(tempColor);
                    colours.Add(tempColor);
                    colours.Add(tempColor);
                    tempColor   = centreColor;
                    tempColor.a = 100;
                    colours.Add(tempColor);

                    tris.Add(initial);
                    tris.Add(initial + 4);
                    tris.Add(initial+1);

                    tris.Add(initial);
                    tris.Add(initial+2);
                    tris.Add(initial+4);

                    tris.Add(initial+2);
                    tris.Add(initial+3);
                    tris.Add(initial+4);

                    tris.Add(initial+3);
                    tris.Add(initial+1);
                    tris.Add(initial+4);

                }
            }
            

            mesh.SetVertices(verts);
            mesh.colors32 = colours.ToArray();
            mesh.SetTriangles(tris, 0);
            //mesh.SetColors(colours);

            return mesh;

        }



        private static int[,] cellOffsets = new int[4,2]
                                             {
                                                 {0,0},
                                                 {-1,0},
                                                 {0,-1},
                                                 {-1,-1}
                                             };

        
        [TweakValue("_NV", 1, 255)]
        public static int alphaBaseGlow = 0;

        public static Color32 clear = new Color32(255,255,255,0);

        public static Mesh MakeNewMesh(MovingGlowCells glowCells, int rootCellIndex, int glowRadius, ColorInt glowColor, out Vector3 offset)
        {
            var mesh = new Mesh();
            float y =  0;
            
            List<Vector3> verts = new List<Vector3>();
            List<Color32> colours = new List<Color32>();

            int[,] rowIndices = new int[glowCells.NumRows + 1, 2];
            int currentIndex = 0;
            offset = Vector3.zero;
            
            float AttenLinearSlope = -1f / glowRadius;
            foreach (GlowCell glowCell in glowCells)
            {
                int dist      = glowCell.dist;

                float    rad         = (float) dist / 100f;
                //ColorInt addedColour = baseColour;
                
                    //float b = 1f / (rad * rad);
                    //float a = 1f + AttenLinearSlope * rad;

                    //addedColour += (glowColor - baseColour) * Math.Min(1 ,(0.8f * a) +( 0.2f * b));
                    Color32 addedColour = Color32.Lerp(glowColor.ToColor32, clear, rad / glowRadius);
                    glowCell.displayColour = addedColour;



            }

            
            for (int z = 0; z <= glowCells.NumRows; z++)
            {
                int rowLength = glowCells[z]?.Count ?? glowCells[z - 1].Count;
                bool foundFirstCell = false;
                rowIndices[z,0] = currentIndex;
                for (int x = 0; x <= rowLength; x++)
                {

                    if (glowCells[z,x] != null || glowCells[z - 1,x] != null || glowCells[z, x -1] != null || glowCells[z - 1, x - 1] != null)
                    {
                        ColorInt averageColour = default(ColorInt);

                        for (int nCell = 0; nCell < 4; nCell++)
                        {
                            averageColour += glowCells[z + cellOffsets[nCell, 0],x + cellOffsets[nCell, 1]]?.displayColour ?? clear;
                        }

                        averageColour /= 4;
                        averageColour.ClampToNonNegative();
                        Log.Message($"averageColour = {averageColour.ToColor32}");

                        colours.Add(averageColour.ToColor32);
                        verts.Add(new Vector3(x, y, z));
                        currentIndex++;

                        if (glowCells[z,x] != null)
                        {
                            if (glowCells[z, x].index == rootCellIndex)
                            {
                                offset = new Vector3(-x - 0.5f, y, -z - 0.5f);
                            }

                            if (!foundFirstCell)
                            {
                                foundFirstCell = true;
                                rowIndices[z, 1] = x;
                            }
                        }
                    }

                    
                }
            }

            rowIndices[glowCells.NumRows, 1] = rowIndices[glowCells.NumRows - 1, 1];

            int firstCentreIndex = verts.Count;

            for (int z = 0; z < glowCells.NumRows; z++)
            {
                var row = glowCells[z];
                for (int x = 0; x < row.Count; x++)
                {
                    if (row[x] != null)
                    {
                        verts.Add(new Vector3(x, y, z));
                        colours.Add(glowCells[z,x].displayColour);
                    }
                }
            }

            List<int> tris = new List<int>();

            int count = 0;
            for (int z = 0; z < glowCells.matrix.Count; z++)
            {
                var row = glowCells[z];
                int botOffset = z == 0 ? 0 : Math.Max(0 , rowIndices[z, 1] - rowIndices[z - 1, 1]);
                int topOffset = Math.Max(rowIndices[z, 1] - rowIndices[z + 1, 1], 0);
                int botIndex = rowIndices[z, 0] + botOffset;
                int topIndex = rowIndices[z + 1, 0] + topOffset;
                for (int x = 0; x < row.Count; x++)
                {
                    if (row[x] != null)
                    {
                        
                        tris.Add(botIndex);
                        tris.Add(firstCentreIndex + count);
                        tris.Add(botIndex + 1);

                        tris.Add(botIndex);
                        tris.Add(topIndex);
                        tris.Add(firstCentreIndex + count);

                        tris.Add(topIndex);
                        tris.Add(topIndex + 1);
                        tris.Add(firstCentreIndex + count);

                        tris.Add(topIndex + 1);
                        tris.Add(botIndex + 1);
                        tris.Add(firstCentreIndex + count);

                        count++;
                        botIndex++;
                        topIndex++;

                    }
                }
            }
            

            mesh.SetVertices(verts);
            mesh.colors32 = colours.ToArray();
            mesh.SetTriangles(tris, 0);
            //mesh.SetColors(colours);

            return mesh;
        }

        private void CalculateVertexIndices(int worldX, int worldZ, out int botLeft, out int topLeft, out int topRight, out int botRight, out int center)
        {
            int localX = worldX - this.sectRect.minX;
            int localZ = worldZ - this.sectRect.minZ;
            botLeft = localZ * (this.sectRect.Width + 1) + localX;
            botRight = botLeft + 1;
            topLeft = botLeft + sectRect.Width + 1;
            topRight = topLeft + 1;
            center = firstCentreInd + (localZ * this.sectRect.Width + localX);
        }

        //public DynamicLightingOverlay(Section section) : base(section) { }
    }
}