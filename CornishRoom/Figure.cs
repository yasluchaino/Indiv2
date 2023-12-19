﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CornishRoom
{

    public class Figure
    {
        public static float eps = 0.0001f;
        public List<Point3D> points = new List<Point3D>(); 
        public List<Side> sides = new List<Side>();        
        public Material fMaterial;
        public Figure() { }

        public Figure(Figure f)
        {
            foreach (Point3D p in f.points)
                points.Add(new Point3D(p));

            foreach (Side s in f.sides)
            {
                sides.Add(new Side(s));
                sides.Last().host = this;

            }
        }
        public bool RayIntersectsTriangle(Ray ray, Point3D p0, Point3D p1, Point3D p2, out float intersection)
        {
            intersection = -1;
            Point3D edge1 = p1 - p0;
            Point3D edge2 = p2 - p0;
            Point3D h = ray.direction * edge2;
            float a = Point3D.scalar(edge1, h);

            if (a > -eps && a < eps)
                return false; // The ray is parallel to the triangle.

            float f = 1.0f / a;
            Point3D s = ray.start - p0;
            float u = f * Point3D.scalar(s, h);

            if (u < 0 || u > 1)
                return false;

            Point3D q = s * edge1;
            float v = f * Point3D.scalar(ray.direction, q);

            if (v < 0 || u + v > 1)
                return false;

            float t = f * Point3D.scalar(edge2, q);

            if (t > eps)
            {
                intersection = t;
                return true;
            }
            else
            {
                return false; // There is an intersection of lines but not rays.
            }
        }

        public virtual bool FigureIntersection(Ray ray, out float intersect, out Point3D normal)
        {
            intersect = 0;
            normal = null;
            Side intersectedSide = null;

            foreach (Side figureSide in sides)
            {
                if (figureSide.points.Count == 3) // Triangle side
                {
                    if (RayIntersectsTriangle(ray, figureSide.getPoint(0), figureSide.getPoint(1), figureSide.getPoint(2), out float t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        intersectedSide = figureSide;
                    }
                }
                else if (figureSide.points.Count == 4) // Quadrilateral side
                {
                    if (RayIntersectsTriangle(ray, figureSide.getPoint(0), figureSide.getPoint(1), figureSide.getPoint(3), out float t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        intersectedSide = figureSide;
                    }
                    else if (RayIntersectsTriangle(ray, figureSide.getPoint(1), figureSide.getPoint(2), figureSide.getPoint(3), out t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        intersectedSide = figureSide;
                    }
                }
            }

            if (intersect != 0)
            {
                normal = Side.norm(intersectedSide);
                fMaterial.color = new Point3D(intersectedSide.drawing_pen.Color.R / 255f, intersectedSide.drawing_pen.Color.G / 255f, intersectedSide.drawing_pen.Color.B / 255f);
                return true;
            }

            return false;
        }
        public float[,] GetMatrix()
        {
            var res = new float[points.Count, 4];
            for (int i = 0; i < points.Count; i++)
            {
                res[i, 0] = points[i].x;
                res[i, 1] = points[i].y;
                res[i, 2] = points[i].z;
                res[i, 3] = 1;
            }
            return res;
        }

        public void ApplyMatrix(float[,] matrix)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].x = matrix[i, 0] / matrix[i, 3];
                points[i].y = matrix[i, 1] / matrix[i, 3];
                points[i].z = matrix[i, 2] / matrix[i, 3];
            }
        }

        private Point3D GetCenter()
        {
            Point3D res = new Point3D(0, 0, 0);
            foreach (Point3D p in points)
            {
                res.x += p.x;
                res.y += p.y;
                res.z += p.z;

            }
            res.x /= points.Count();
            res.y /= points.Count();
            res.z /= points.Count();
            return res;
        }

        public void RotateArondRad(float rangle, string type)
        {
            float[,] mt = GetMatrix();
            Point3D center = GetCenter();
            switch (type)
            {
                case "CX":
                    mt = ApplyOffset(mt, -center.x, -center.y, -center.z);
                    mt = ApplyRotation_X(mt, rangle);
                    mt = ApplyOffset(mt, center.x, center.y, center.z);
                    break;
                case "CY":
                    mt = ApplyOffset(mt, -center.x, -center.y, -center.z);
                    mt = ApplyRotation_Y(mt, rangle);
                    mt = ApplyOffset(mt, center.x, center.y, center.z);
                    break;
                case "CZ":
                    mt = ApplyOffset(mt, -center.x, -center.y, -center.z);
                    mt = ApplyRotation_Z(mt, rangle);
                    mt = ApplyOffset(mt, center.x, center.y, center.z);
                    break;
                case "X":
                    mt = ApplyRotation_X(mt, rangle);
                    break;
                case "Y":
                    mt = ApplyRotation_Y(mt, rangle);
                    break;
                case "Z":
                    mt = ApplyRotation_Z(mt, rangle);
                    break;
                default:
                    break;
            }
            ApplyMatrix(mt);
        }

        public void RotateAround(float angle, string type)
        {
            RotateArondRad(angle * (float)Math.PI / 180, type);
        }

        public void Scale_axis(float xs, float ys, float zs)
        {
            float[,] pnts = GetMatrix();
            pnts = ApplyScale(pnts, xs, ys, zs);
            ApplyMatrix(pnts);
        }

        public void Offset(float xs, float ys, float zs)
        {
            ApplyMatrix(ApplyOffset(GetMatrix(), xs, ys, zs));
        }

        public void SetPen(Pen dw)
        {
            foreach (Side s in sides)
                s.drawing_pen = dw;
        }

        public void ScaleAroundCenter(float xs, float ys, float zs)
        {
            float[,] pnts = GetMatrix();
            Point3D p = GetCenter();
            pnts = ApplyOffset(pnts, -p.x, -p.y, -p.z);
            pnts = ApplyScale(pnts, xs, ys, zs);
            pnts = ApplyOffset(pnts, p.x, p.y, p.z);
            ApplyMatrix(pnts);
        }


        private static float[,] MultiplyMatrix(float[,] m1, float[,] m2)
        {
            float[,] res = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    for (int k = 0; k < m2.GetLength(0); k++)
                    {
                        res[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return res;
        }

        private static float[,] ApplyOffset(float[,] transform_matrix, float offset_x, float offset_y, float offset_z)
        {
            float[,] translationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { offset_x, offset_y, offset_z, 1 } };
            return MultiplyMatrix(transform_matrix, translationMatrix);
        }

        private static float[,] ApplyRotation_X(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, (float)Math.Cos(angle), (float)Math.Sin(angle), 0 },
                { 0, -(float)Math.Sin(angle), (float)Math.Cos(angle), 0}, { 0, 0, 0, 1} };
            return MultiplyMatrix(transform_matrix, rotationMatrix);
        }

        private static float[,] ApplyRotation_Y(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(angle), 0, -(float)Math.Sin(angle), 0 }, { 0, 1, 0, 0 },
                { (float)Math.Sin(angle), 0, (float)Math.Cos(angle), 0}, { 0, 0, 0, 1} };
            return MultiplyMatrix(transform_matrix, rotationMatrix);
        }

        private static float[,] ApplyRotation_Z(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(angle), (float)Math.Sin(angle), 0, 0 }, { -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 }, { 0, 0, 0, 1} };
            return MultiplyMatrix(transform_matrix, rotationMatrix);
        }

        private static float[,] ApplyScale(float[,] transform_matrix, float scale_x, float scale_y, float scale_z)
        {
            float[,] scaleMatrix = new float[,] { { scale_x, 0, 0, 0 }, { 0, scale_y, 0, 0 }, { 0, 0, scale_z, 0 }, { 0, 0, 0, 1 } };
            return MultiplyMatrix(transform_matrix, scaleMatrix);
        }

        static public Figure get_Cube(float sz)
        {
            Figure res = new Figure();
            res.points.Add(new Point3D(sz / 2, sz / 2, sz / 2)); // 0 
            res.points.Add(new Point3D(-sz / 2, sz / 2, sz / 2)); // 1
            res.points.Add(new Point3D(-sz / 2, sz / 2, -sz / 2)); // 2
            res.points.Add(new Point3D(sz / 2, sz / 2, -sz / 2)); //3

            res.points.Add(new Point3D(sz / 2, -sz / 2, sz / 2)); // 4
            res.points.Add(new Point3D(-sz / 2, -sz / 2, sz / 2)); //5
            res.points.Add(new Point3D(-sz / 2, -sz / 2, -sz / 2)); // 6
            res.points.Add(new Point3D(sz / 2, -sz / 2, -sz / 2)); // 7

            Side s = new Side(res);
            s.points.AddRange(new int[] { 3, 2, 1, 0 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 4, 5, 6, 7 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 2, 6, 5, 1 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 0, 4, 7, 3 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 1, 5, 4, 0 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 2, 3, 7, 6 });
            res.sides.Add(s);
            return res;
        }
        static public Figure get_Rectangle(float width, float depth, float height)
        {
            Figure res = new Figure();
            // Основание прямоугольника в плоскости XY
            res.points.Add(new Point3D(width / 2, depth / 2, 0));        // 0 
            res.points.Add(new Point3D(-width / 2, depth / 2, 0));       // 1
            res.points.Add(new Point3D(-width / 2, -depth / 2, 0));      // 2
            res.points.Add(new Point3D(width / 2, -depth / 2, 0));       // 3

            // Вершины высоты прямоугольника вдоль оси Z
            res.points.Add(new Point3D(width / 2, depth / 2, height));    // 4
            res.points.Add(new Point3D(-width / 2, depth / 2, height));   // 5
            res.points.Add(new Point3D(-width / 2, -depth / 2, height));  // 6
            res.points.Add(new Point3D(width / 2, -depth / 2, height));   // 7

            Side s = new Side(res);
            s.points.AddRange(new int[] { 0, 1, 2, 3 }); // Основание
            res.sides.Add(s);

            // Стороны параллелепипеда
            s = new Side(res);
            s.points.AddRange(new int[] { 4, 5, 1, 0 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 5, 6, 2, 1 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 6, 7, 3, 2 });
            res.sides.Add(s);

            s = new Side(res);
            s.points.AddRange(new int[] { 7, 4, 0, 3 });
            res.sides.Add(s);

            // Высота прямоугольника
            s = new Side(res);
            s.points.AddRange(new int[] { 4, 5, 6, 7 });
            res.sides.Add(s);

            return res;
        }

    }
}
