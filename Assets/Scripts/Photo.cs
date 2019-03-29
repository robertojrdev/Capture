using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Photo 
{
    public RenderTexture renderTexture;
    public List<Puzzle> resolvablesPuzzles = new List<Puzzle>();

    private static RenderTexture rt;
    private static Texture2D screenShot;

    public static Photo TakeAPhoto(Camera camera, PostProcessLayer postProcessLayer , LayerMask? mask = null, RectTransform sizeReferenceImage = null, TextureFormat format = TextureFormat.RGB24)
    {
        //postProcessLayer.enabled = false;

        LayerMask originalMask = camera.cullingMask;
        if (mask.HasValue)
        {
            camera.cullingMask = mask.Value;
        }

        //cut image
        int width = Screen.width;
        int height = Screen.height;
        int startX = 0;
        int startY = 0;
        int multiplier = 2;
        if (sizeReferenceImage)
        {
            int rectHeight = (int)sizeReferenceImage.rect.height;
            int rectWidth = (int)sizeReferenceImage.rect.width;

            int startPixelWidth = (width / 2) - (rectWidth / 2);
            int startPixelHeight = (height / 2) - (rectHeight / 2);

            startX = startPixelWidth * multiplier;
            startY = startPixelHeight * multiplier;

            width = rectWidth * multiplier;
            height = rectHeight * multiplier;
        }

        rt = new RenderTexture(width, height, 32, RenderTextureFormat.RGB565);
        camera.targetTexture = rt;
        if(!screenShot)
            screenShot = new Texture2D(width, height, format, false);

        camera.Render();
        RenderTexture.active = rt;

        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors

        if (mask.HasValue)
        {
            camera.cullingMask = originalMask;
        }

        //postProcessLayer.enabled = true;

        return new Photo { renderTexture = rt };
    }

    public Texture GetSprite()
    {
        if (!renderTexture)
            return null;

        Texture2D t = new Texture2D(renderTexture.width, renderTexture.height);
        int diference = renderTexture.width - renderTexture.height;
        int widthReadInit = diference / 2;
        Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        t.ReadPixels(rect, 0, 0);
        Sprite s = Sprite.Create(t, rect, Vector2.one / 2);

        return t;
    }

    public bool CanResolvePuzzle(Puzzle puzzle)
    {
        for (int i = 0; i < resolvablesPuzzles.Count; i++)
        {
            if(resolvablesPuzzles[i] == puzzle)
                return true;
        }
        return false;
    }
}
