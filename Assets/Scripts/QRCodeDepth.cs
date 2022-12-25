using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QRDetection.Location
{
    public class QRCodeDepth
    {
        private float depthEstimation;

        public float DepthEstimation { get => depthEstimation; }

        private const int QR15 = 15;
        private const int QR20 = 20;
        private const int QR25 = 25;
        private const int QR30 = 30;

        private float[] limits = { 0.060f, 0.085f, 0.070f, 0.095f, 0.090f, 0.115f, 0.105f, 0.130f };

        public void ValidateDepthEstimation(string qRCodeContent, float depthFound)
        {
            int size = int.Parse(qRCodeContent.Substring(0, 2));
            switch (size)
            {
                case QR15:
                    Debug.Log("QRCode Size: " + QR15);
                    CheckDepthEstimation(depthFound, limits[0], limits[1]);

                    break;
                case QR20:
                    Debug.Log("QRCode Size: " + QR20);
                    CheckDepthEstimation(depthFound, limits[2], limits[3]);

                    break;
                case QR25:
                    Debug.Log("QRCode Size: " + QR25);
                    CheckDepthEstimation(depthFound, limits[3], limits[4]);

                    break;
                case QR30:
                    Debug.Log("QRCode Size:  " + QR30);
                    CheckDepthEstimation(depthFound, limits[4], limits[5]);

                    break;
                default:
                    break;
            }
        }

        public void GenerateArtificialDepthEstimate(string qRCodeContent)
        {
            int size = int.Parse(qRCodeContent.Substring(0, 2));
            switch (size)
            {
                case QR15:
                    Debug.Log("QRCode Size: " + QR15);
                    GenereteDepth(limits[0], limits[1]);

                    break;
                case QR20:
                    //Debug.Log("QRCode Size: " + QR20);
                    GenereteDepth(limits[2], limits[3]);

                    break;
                case QR25:
                    Debug.Log("QRCode Size: " + QR25);
                    GenereteDepth(limits[3], limits[4]);

                    break;
                case QR30:
                    Debug.Log("Size: " + QR30);
                    GenereteDepth(limits[4], limits[5]);

                    break;
                default:
                    break;
            }
        }


        private void CheckDepthEstimation(float depthFound, float min, float max)
        {
            if (depthFound < min || depthFound > max)
            {
                Debug.Log("Depth estimate not valid " + depthFound);
                GenereteDepth(min, max);
            }
            else
            {
                depthEstimation = depthFound;
                Debug.Log("Depth estimate valid: " + depthFound);
            }
        }

        private void GenereteDepth(float min, float max)
        {
#if !UNITY_EDITOR
            depthEstimation = Random.Range(min, max);
#endif
            depthEstimation = 0.1f;
        }
    }


}
