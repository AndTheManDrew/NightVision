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

        public static Mesh MakeNewMesh(MovingGlowCells glowCells, int rootCellIndex, out Vector3 offset)
        {
            var mesh = new Mesh();
            float y =  AltitudeLayer.LightingOverlay.AltitudeFor();

            List<Vector3> verts = new List<Vector3>();

            int[,] rowIndices = new int[glowCells.NumRows + 1, 2];
            int currentIndex = 0;
            offset = Vector3.zero;
            
            for (int z = 0; z <= glowCells.NumRows; z++)
            {
                int rowLength = glowCells[z]?.Count ?? glowCells[z - 1].Count;
                bool foundFirstCell = false;
                rowIndices[z,0] = currentIndex;
                for (int x = 0; x <= rowLength; x++)
                {
                    if (glowCells[z,x] != null)
                    {
                        if (glowCells[z,x].index == rootCellIndex)
                        {
                            offset = new Vector3(-x - 0.5f, y, -z - 0.5f);
                        }

                        if (!foundFirstCell)
                        {
                            foundFirstCell = true;
                            rowIndices[z, 1] = x;
                        }
                        verts.Add(new Vector3(x, y, z));
                        currentIndex++;
                    }
                    else if (glowCells[z - 1,x] != null || glowCells[z, x-1] != null || glowCells[z - 1, x - 1] != null)
                    {
                        verts.Add(new Vector3(x, y, z));
                        currentIndex++;
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

            Color32[] colours = new Color32[verts.Count];

            for (int i = 0; i < colours.Length; i++)
            {
                colours[i] = new Color32(100, 0, 0, 100);
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.colors32 = colours;


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