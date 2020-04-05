using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

[ScriptedImporter(1, "fxgfx")]
public class FXGFXImporter : ScriptedImporter
{


    //color format: TODO
    private uint[] PALETTE = {
                0xFF000000, 0xFF111111, 0xFF222222, 0xFF333333,

                0xFF444444, 0xFF555555, 0xFF666666, 0xFF777777,

                0xFF888888, 0xFF999999, 0xFFAAAAAA, 0xFFBBBBBB,

                0xFFCCCCCC, 0xFFDDDDDD, 0xFFEEEEEE, 0xFFFFFFFF
        };

    //the file stores two individual textures inside one texture by having each take up a nybble of each byte for each pixel
    public int imageIndex = 1; //which of the two textures to import

    public override void OnImportAsset(AssetImportContext ctx)
    {
        int width = 256, height = 256;
        var tex = new Texture2D(width, height);
        byte[] data = File.ReadAllBytes(ctx.assetPath);
        string filename = Path.GetFileNameWithoutExtension(ctx.assetPath);

        for(int i = 0; i < data.Length; i++){
            uint paletteColor = PALETTE[imageIndex == 0 ? data[i] >> 4 : data[i] & 0xF];
            float r = (float)(paletteColor & 0xFF)/256f;
            float g = (float)((paletteColor >> 8) & 0xFF)/256f;
            float b = (float)((paletteColor >> 16) & 0xFF)/256f;
            float a = (float)(paletteColor >> 24)/256f;
            Color color = new Color(r,g,b,a);
            tex.SetPixel(i % 256,256 - i / 256, color);
        }

        ctx.AddObjectToAsset(filename, tex);
        ctx.SetMainObject(tex);
    }
}