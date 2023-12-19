using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CornishRoom
{
    public class Sphere : Figure
    {
        float radius;       

        public Sphere(Point3D p, float r)
        {
            points.Add(p);
            radius = r;
     
        }
      
        public static bool RaySphereIntersection(Ray r, Point3D sphere_pos, float sphere_rad, out float t)
        {
            Point3D k = r.start - sphere_pos;
            float b = Point3D.scalar(k, r.direction);
            float c = Point3D.scalar(k, k) - sphere_rad * sphere_rad;
            float d = b * b - c;
            t = 0;
            if (d >= 0)
            {
                float sqrtd = (float)Math.Sqrt(d);
                float t1 = -b + sqrtd;
                float t2 = -b - sqrtd;

                float min_t = Math.Min(t1, t2);
                float max_t = Math.Max(t1, t2);

                t = (min_t > eps) ? min_t : max_t;
                return t > eps;
            }
            return false;
        }

        public override bool FigureIntersection(Ray r, out float t, out Point3D normal)
        {
            t = 0;
            normal = null;
            if (RaySphereIntersection(r, points[0], radius, out t) && (t > eps))
            {
                normal = (r.start + r.direction * t) - points[0];
                normal = Point3D.norm(normal);
                return true;
            }
            return false;
        }
    }
}
