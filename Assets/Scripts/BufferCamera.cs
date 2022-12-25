using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
public class BufferCamera : MonoBehaviour
{
    [SerializeField] private RawImage image;
    // The width and height of the texture.
    private const int textureWidth = 256;
    private const int textureHeight = 256;

    // The format of the texture.
    private const TextureFormat textureFormat = TextureFormat.RGBA32;
    private IntPtr[] pixelData;
    void Start()
    {
        ARSessionFactory.SessionInitialized += ARSessionFactory_SessionInitialized;
        Invoke("TryToGetTexture", 1.5f);
    }

    private void ARSessionFactory_SessionInitialized(AnyARSessionInitializedArgs args)
    {
        args.Session.FrameUpdated += Session_FrameUpdated;
    }

    private void Session_FrameUpdated(FrameUpdatedArgs args)
    {

        if (args.Frame == null)
        {
            return;
        }

        var img = args.Frame.CapturedImageBuffer;
        IntPtr[] capturedImageBufferargs = args.Frame.CapturedImageTextures;
        pixelData = capturedImageBufferargs;
    }

    private void TryToGetTexture() 
    {
        Texture2D texture = new Texture2D(textureWidth, textureHeight, textureFormat, false);

        // Allocate a managed array to hold the pixel data.
        byte[] pixelDataBytes = new byte[textureWidth * textureHeight * 4];

        // Copy the pixel data from the IntPtr array into the managed array.
        for (int i = 0; i < pixelData.Length; i++)
        {
            Marshal.Copy(pixelData[i], pixelDataBytes, i * 4, 4);
        }

        // Convert the pixel data bytes to an array of Color objects.
        Color[] pixelColors = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixelDataBytes.Length; i += 4)
        {
            pixelColors[i / 4] = new Color(
                pixelDataBytes[i + 0] / 255f,
                pixelDataBytes[i + 1] / 255f,
                pixelDataBytes[i + 2] / 255f,
                pixelDataBytes[i + 3] / 255f
            );
        }

        // Set the pixel data for the texture.
        texture.SetPixels(pixelColors);

        // Apply the changes to the texture.
        texture.Apply();

        image.texture = texture;
    }

}
