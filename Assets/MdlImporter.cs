using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[ScriptedImporter(1, "mdl")]
public class MdlImporter : ScriptedImporter
{

    [HideInInspector]
    public Mesh mesh;
    public List<Vector3> verts;
    public int[] tris;
    bool exitLoopEarly; //used for debugging while the data reading code is incomplete, remove this eventually

    public override void OnImportAsset(AssetImportContext ctx)
    {
        byte[] data = File.ReadAllBytes(ctx.assetPath);
        string filename = Path.GetFileNameWithoutExtension(ctx.assetPath);
        int fileOffset = 0;

        mesh = new Mesh();
        verts = new List<Vector3>();

        while(fileOffset < data.Length && exitLoopEarly == false){
            //byte that signifies a type of section following in the 3d model file, followed by that section.
            byte id = ReadByte(data, ref fileOffset);
            switch(id){
                case 0x04: //normal vertex list
                int numberOfVertices = ReadByte(data, ref fileOffset);
                //Debug.Log("Number of vertices in normal vertex list: " + numberOfVertices);
                for(int i = 0; i < numberOfVertices; i++)
                {
                    //the x/y/z values are stored as signed 8-bit numbers, so convert them twice to float
                    float x = (float)(sbyte)ReadByte(data, ref fileOffset);
                    float y = (float)(sbyte)ReadByte(data, ref fileOffset);
                    float z = (float)(sbyte)ReadByte(data, ref fileOffset);

                    verts.Add(new Vector3(x,y,z));
                }
                break;
                case 0x0C: //end of vertex data
                mesh.vertices = verts.ToArray();
                break;
                case 0x30: //triangle list
                int numberOfTris = ReadByte(data, ref fileOffset);
                tris = new int[numberOfTris * 3];
                for(int i = 0; i < numberOfTris; i++)
                {
                    tris[3*i] = ReadByte(data, ref fileOffset);
                    tris[3*i + 1] = ReadByte(data, ref fileOffset);
                    tris[3*i + 2] = ReadByte(data, ref fileOffset);
                }
                mesh.triangles = tris;
                exitLoopEarly = true;
                break;
                case 0x38: //mirrored (x-axis) vertex list (each vertex is copied with the x position flipped (-x,y,z))
                numberOfVertices = ReadByte(data, ref fileOffset);
                //Debug.Log("Number of vertices in x-axis mirrored vertex list: " + numberOfVertices);
                for(int i = 0; i < numberOfVertices; i++)
                {
                    //the x/y/z values are stored as signed 8-bit numbers, so convert them twice to float
                    float x = (float)(sbyte)ReadByte(data, ref fileOffset);
                    float y = (float)(sbyte)ReadByte(data, ref fileOffset);
                    float z = (float)(sbyte)ReadByte(data, ref fileOffset);

                    //Add both the normal and x position mirrored vertices to the list
                    verts.Add(new Vector3(x,y,z));
                    verts.Add(new Vector3(-x,y,z));
                }
                break;
                default:
                throw new UnityException("Unimplemented or invalid id value 0x" + id.ToString("X2") + " at offset 0x" + fileOffset.ToString("X"));
            }
        }

        //create mesh asset here
        mesh.RecalculateNormals();
        ctx.AddObjectToAsset(filename, mesh);
        ctx.SetMainObject(mesh);
        Debug.Log("Model imported!");
    }

    public byte ReadByte(byte[] data, ref int offset){
        byte readByte = data[offset++];
        return readByte;
    }
}
