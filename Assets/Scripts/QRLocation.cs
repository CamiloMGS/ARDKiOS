using System;
using UnityEngine;
namespace QRDetection.Location
{
    public class QRLocation
    {

        private Point topRight;
        private Point bottomLeft;
        private Point topLeft;
        private Point alignmentPattern;


        public QRLocation(Point topLeft, Point topRight, Point alignmentPattern, Point bottomLeft)
        {
            this.topRight = topRight;
            this.topLeft = topLeft;
            this.bottomLeft = bottomLeft;
            this.alignmentPattern = alignmentPattern;
        }

        public Point Center { get => center; }
        public float Angle { get => angle; }

        public Point TopRight { get => topRight; }
        public Point BottomLeft { get => bottomLeft; }
        public Point TopLeft { get => topLeft; }
        public Point AlignmentPattern { get => alignmentPattern; }

        private Point center;
        private float angle;

        public void Initialize()
        {
            center = FindCenter(bottomLeft, topRight, topLeft, alignmentPattern);
            FindAligmentARDK();
            //FindAlignment();
  
        }

        private void FindAligmentARDK()
        {
            if (alignmentPattern.x>center.x && alignmentPattern.y<center.y)
            {
                angle = GetAngle(center, AlignmentPattern, 90, -1);
                Debug.Log("4to");
            }
            else if(alignmentPattern.x > center.x && alignmentPattern.y>center.y)
            {
                angle = GetAngle(center, AlignmentPattern, 90, -1);
                Debug.Log("1ro");
            }
            else if (alignmentPattern.x<center.x && alignmentPattern.y > center.y)
            {
                angle = GetAngle(center, AlignmentPattern, 270);
                Debug.Log("2do");
            }
            else if (alignmentPattern.x< center.x && alignmentPattern.y < center.y)
            {
                angle = GetAngle(center, AlignmentPattern, 270);
                Debug.Log("3ro");
            }
        }

        private float GetAngle(Point center, Point app, float angle = 0.0f, int factor =1) 
        {
            var radius = Point.Distance(center, app);
            var yComp = center.y - app.y;
            var absoluteAngle = Math.Asin(yComp / radius)*(180 / Math.PI);
            return angle + factor*(float)absoluteAngle;
        }
        private void FindAlignment()
        {
            // Alignment Patterns First quadrant 
            if (alignmentPattern.y < center.y && alignmentPattern.x < center.x)
            {
                var relAP = Point.Subtract(alignmentPattern, center);
                var tempAP = Point.Subtract(center, relAP);
                alignmentPattern = tempAP;
                var relTL = Point.Subtract(topLeft, center);
                var tempTL = Point.Subtract(center, relTL);
                topLeft = tempTL;
                var TempBL = topRight;
                topRight = bottomLeft;
                bottomLeft = TempBL;
                angle = FindAngle(topLeft, bottomLeft, -90);
                Debug.Log("First quadrant ");
            }

            // Alignment Patterns Second quadrant 
            else if (alignmentPattern.y > center.y && alignmentPattern.x < center.x)
            {
                angle = FindAngle(topRight, topLeft, 180);
                Debug.Log("Second quadrant  " + angle);
            }

            // Alignment Patterns Third quadrant 
            else if (alignmentPattern.y > center.y && alignmentPattern.x > center.x)
            {
                var relAP = Point.Subtract(alignmentPattern, center);
                var tempAP = Point.Subtract(center, relAP);
                alignmentPattern = tempAP;
                var relTL = Point.Subtract(topLeft, center);
                var tempTL = Point.Subtract(center, relTL);
                topLeft = tempTL;
                var TempBL = topRight;
                topRight = bottomLeft;
                bottomLeft = TempBL;
                angle = FindAngle(bottomLeft, topLeft, -270);
                Debug.Log("Third quadrant  ");
            }

            // Alignment Patterns fourth quadrant 
            else if (alignmentPattern.y < center.y && alignmentPattern.x > center.x)
            {
                angle = FindAngle(topLeft, topRight, 360);
                Debug.Log("Alignment Patterns fourth quadrant  ");
            }

        }

        private Point FindCenter(Point p1, Point p2, Point p3, Point p4)
        {
            var dx1 = p2.x - p1.x;
            var m1 = (p2.y - p1.y) / dx1;
            var b1 = p1.y - (m1 * p1.x);


            var dx2 = p4.x - p3.x;
            var m2 = (p4.y - p3.y) / dx2;
            var b2 = p3.y - (m2 * p3.x);

            var x = (b1 - b2) / (m2 - m1);

            var y = m2 * ((b1 - b2) / (m2 - m1)) + b2;

            var QRCenter = new Point(x, y);

            return QRCenter;
        }

        private float FindAngle(Point P1, Point P2, float quadrantRotation)
        {
            var inter = new Point(P1.x, P2.y);

            var D1 = Point.Distance(new Point(P2.x, P2.y), inter);
            var D2 = Point.Distance(new Point(P1.x, P1.y), inter);

            var angle = 90 - Math.Atan2(D1, D2) * (180 / Math.PI);

            if (angle < 5)
            {
                angle = 0;
            }

            var resultAngle = (float)angle - quadrantRotation;
            return resultAngle;
        }
    }
}
[Serializable]
public struct Point
{
    public float x;
    public float y;

    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static Point Subtract(Point a, Point b)
    {
        var subX = a.x - b.x;
        var subY = a.y - b.y;
        var result = new Point(subX, subY);
        return result;
    }
    public static float Distance(Point a, Point b)
    {
        return (float)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
    }
}


