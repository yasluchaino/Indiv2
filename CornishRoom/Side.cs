using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CornishRoom
{
    public class Side
    {
        public Figure host = null;
        public List<int> points = new List<int>();
        public Pen drawing_pen = new Pen(Color.White);
        public Point3D Normal;
        public Material material;

        public Side(Figure h = null)
        {
            host = h;
        }

        public Side(Side s)
        {
            points = new List<int>(s.points);
            host = s.host;
            drawing_pen = s.drawing_pen.Clone() as Pen;
            Normal = new Point3D(s.Normal);
            material = s.material;
        }

        public Point3D getPoint(int index)
        {
            if (host != null)
                return host.points[points[index]];
            return null;
        }

        public static Point3D norm(Side S)
        {
            if (S.points.Count() < 3)
                return new Point3D(0, 0, 0);
            Point3D U = S.getPoint(1) - S.getPoint(0);
            Point3D V = S.getPoint(S.points.Count - 1) - S.getPoint(0);
            Point3D normal = U * V;
            return Point3D.norm(normal);
        }

        public void CalculateSideNormal()
        {
            Normal = norm(this);
        }
    }
}
