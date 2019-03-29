using System.Collections.Generic;
using UnityEngine;

namespace Paths
{
    [System.Serializable]
    public class Path
    {
        public string name;
        public List<PathPoint> Points { get; private set; }
        public int Count { get; private set; }
        public int PointsNameCount { get; private set; }


        public float LastPointSpeed
        {
            get
            {
                if (Points.Count >= 1)
                    return Points[Points.Count - 1].speed;
                else
                    return 1;
            }
        }

        public Path(string name = "no name")
        {
            Points = new List<PathPoint>();
            Count = Points.Count;
            this.name = name;
        }

        public Path(List<PathPoint> points, string name = "no name")
        {
            Points = points;
            Count = Points.Count;
            this.name = name;
        }

        public Path(PathPoint[] points, string name = "no name")
        {
            if (points == null)
                Points = new List<PathPoint>();
            Points = new List<PathPoint>(points);
            Count = Points.Count;
            this.name = name;
        }

        public void AddPoint(PathPoint point)
        {
            if (Points == null)
                Points = new List<PathPoint>();
            Points.Add(point);
            Count = Points.Count;
            PointsNameCount++;
        }

        public void AddPoints(PathPoint[] points)
        {
            if (Points == null)
                Points = new List<PathPoint>(points);
            else
                Points.AddRange(points);

            Count = Points.Count;
            PointsNameCount += points.Length;
        }

        public void AddPoints(List<PathPoint> points)
        {
            if (Points == null)
                Points = points;
            else
                Points.AddRange(points);

            Count = Points.Count;
        }

        public void RemovePoint(int index)
        {
            if (index < Points.Count)
            {
                Points.RemoveAt(index);
                Count = Points.Count;
                if (index == Points.Count)
                    PointsNameCount--;
            }
        }
    }

    [System.Serializable]
    public class PathPoint
    {
        public string name;
        private Vector3_Serializable Position;
        public Quaternion_Serializable Rotation;
        public float speed;
        public float FOV;

        public Vector3_Serializable SunEuler;
        public Color_Serializable SunColor;
        public float sunIntensity;

        public Vector3 position
        {
            get
            {
                return Position.GetVector3();
            }

            set
            {
                if (Position == null)
                    Position = new Vector3_Serializable(value);
                else
                    Position.Set(value);
            }
        }
        public Vector3 sunEuler
        {
            get
            {
                return SunEuler.GetVector3();
            }

            set
            {
                if (SunEuler == null)
                    SunEuler = new Vector3_Serializable(value);
                else
                    SunEuler.Set(value);
            }
        }
        public Quaternion rotation
        {
            get
            {
                return Rotation.GetQuaternion();
            }

            set
            {
                if (Rotation == null)
                    Rotation = new Quaternion_Serializable(value);
                else
                    Rotation.Set(value);
            }
        }
        public Color sunColor
        {
            get
            {
                return SunColor.GetColor();
            }

            set
            {
                if (SunColor == null)
                    SunColor = new Color_Serializable(value);
                else
                    SunColor.Set(value);
            }
        }

        public PathPoint(Vector3 position, Quaternion rotation, float speed = 1, float FOV = 60, string name = "no name")
        {
            this.position = position;
            this.rotation = rotation;
            this.speed = speed;
            this.FOV = FOV;
            this.name = name;
        }

        public void SetWeather(Vector3 euler, Color color, float intensity)
        {
            sunEuler = euler;
            sunColor = color;
            sunIntensity = intensity;
        }
    }

    [System.Serializable]
    public class Vector3_Serializable
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;

        public Vector3_Serializable()
        {
        }

        public Vector3_Serializable(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 GetVector3()
        {
            return new Vector3(x, y, z);
        }

        public void Set(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }

    [System.Serializable]
    public class Quaternion_Serializable
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float w = 0;

        public Quaternion_Serializable()
        {
        }

        public Quaternion_Serializable(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public Quaternion GetQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public void Set(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }
    }

    [System.Serializable]
    public class Color_Serializable
    {
        public float r = 0;
        public float g = 0;
        public float b = 0;
        public float a = 1;

        public Color_Serializable()
        {
        }

        public Color_Serializable(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color GetColor()
        {
            return new Color(r, g, b, a);
        }

        public void Set(Color c)
        {
            r = c.r;
            g = c.g;
            b = c.b;
            a = c.a;
        }
    }

}