using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CornishRoom
{
    public class Light : Object           // источник света
    {
        public Point3D point_light;       // точка, где находится источник света
        public Point3D color_light;       // цвет источника света

        public Light(Point3D p, Point3D c)
        {
            point_light = new Point3D(p);
            color_light = new Point3D(c);
        }

        //вычисление локальной модели освещения
        public Point3D Shade(Point3D hit_point, Point3D normal, Point3D material_color, float diffuse_coef)
        {
            Point3D dir = point_light - hit_point;
            dir = Point3D.norm(dir);// направление луча 

            float scalarProduct = Point3D.scalar(normal, dir);
            float scalarResult = scalarProduct < 0 ? 0 : scalarProduct; 

            Point3D diff = diffuse_coef * color_light * scalarResult;

            return new Point3D(diff.x * material_color.x, diff.y * material_color.y, diff.z * material_color.z);
        }
    }
}
