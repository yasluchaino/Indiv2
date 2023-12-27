using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Security.Policy;

namespace CornishRoom
{
    public class Polygon
    {
        public Object host = null; 
        public List<int> points = new List<int>(); // точки, определяющие сторону
        public Pen drawing_pen = new Pen(Color.White);
        public Point3D Normal; // Нормаль стороны 
        public Material material;

        public Polygon(Object h = null)
        {
            host = h; // Конструктор с инициализацией фигуры-хозяина стороны
        }

        public Polygon(Polygon s)
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

        public static Point3D norm(Polygon S)
        {
            if (S.points.Count() < 3)
                return new Point3D(0, 0, 0); // если у стороны менее 3 точек
            Point3D U = S.getPoint(1) - S.getPoint(0); // Вектор U - разность векторов между первой и второй точками
            Point3D V = S.getPoint(S.points.Count - 1) - S.getPoint(0); // Вектор V - разность векторов между первой и последней точками
            Point3D normal = U * V; // Вычисление векторного произведения U и V для получения нормали
            return Point3D.norm(normal); // Нормализация вектора и возврат нормали
        }

        public void CalculateSideNormal()
        {
            Normal = norm(this); // нормаль для текущей стороны
        }
    }
}