using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CornishRoom
{
    public partial class Form1 : Form
    {
        public List<Object> scene = new List<Object>();
        public List<Light> lights = new List<Light>();   // список источников света
        public Color[,] pixels_color;                    // цвета пикселей для отображения на pictureBox
        public Point3D[,] pixels;
        public Point3D cameraPoint;
        public Point3D up_left, up_right, down_left, down_right;
        public int h, w;

        public Form1()
        {
            InitializeComponent();
            cameraPoint = new Point3D();
            up_left = new Point3D();
            up_right = new Point3D();
            down_left = new Point3D();
            down_right = new Point3D();
            h = pictureBox1.Height;
            w = pictureBox1.Width;
            pictureBox1.Image = new Bitmap(w, h);
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            checkBox5.Visible = false;
            textBox1.Visible = false; 
            textBox2.Visible = false; 
            textBox3.Visible = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            BuildScene();
           
            BackwardRayTracing(); 
            checkBox1.Visible = true;
            checkBox2.Visible = true; 
            textBox1.Visible = true;
            textBox2.Visible = true;
            textBox3.Visible = true;

            checkBox5.Visible = true;
            for (int i = 0; i < w; ++i){
                for (int j = 0; j < h; ++j)
                {
                    (pictureBox1.Image as Bitmap).SetPixel(i, j, pixels_color[i, j]);
                }
                pictureBox1.Invalidate();
            }
        }

        public void BuildScene()
        {     
            scene.Clear();
            lights.Clear();
            //сама комната
            Object room = Object.get_Cube(10);
            up_left = room.sides[0].getPoint(0);
            up_right = room.sides[0].getPoint(1);
            down_right = room.sides[0].getPoint(2);
            down_left = room.sides[0].getPoint(3);

            //ставим камеру
            Point3D normal = Polygon.norm(room.sides[0]);                            // нормаль стороны комнаты
            Point3D center = (up_left + up_right + down_left + down_right) / 4;   // центр стороны комнаты
            cameraPoint = center + normal * 11;

            //красим стены
            room.SetPen(new Pen(Color.Gray));
            room.sides[0].drawing_pen = new Pen(Color.Beige);
            room.sides[1].drawing_pen = new Pen(Color.Violet);
            room.sides[2].drawing_pen = new Pen(Color.Yellow);
            room.sides[3].drawing_pen = new Pen(Color.Red);
            
            room.fMaterial = new Material(0, 0, 0.05f, 0.7f);
      //это параллелепипед
            Object bigCube = Object.get_Rectangle(1.5f,2f,6f);
            bigCube.Offset(-0.5f, 2f, -4.6f);
            bigCube.RotateAround(-52, "CZ");
            bigCube.SetPen(new Pen(Color.HotPink));
            bigCube.fMaterial = new Material(0f, 0f, 0.3f, 0.7f, 1f);
            //куб
            Object transCube = Object.get_Cube(2f);
            transCube.Offset(-3f, 2.5f, -3.9f);
            transCube.RotateAround(-10, "CZ");
            transCube.SetPen(new Pen(Color.Red));
            transCube.fMaterial = new Material(0f, 0f, 0.3f, 0.7f, 1f);
            ////недосфера
            //Figure sphere = Figure.get_Sphere(1, 8,8);
            //sphere.fMaterial = new Material(10f, 0f, 0f, 0.1f, 1f);
            //sphere.Offset(-0.5f, 2f, -4.6f);
            //sphere.SetPen(new Pen(Color.Red));

            scene.Add(room);
            scene.Add(bigCube);
            scene.Add(transCube);
            //  scene.Add(sphere);
            if (checkBox1.Checked)
            {            
                //добавляем источники света
                Light l1 = new Light(new Point3D(0f, 2f, 4.9f), new Point3D(1f, 1f, 1f));//белый, посреди комнаты,как люстра
                lights.Add(l1);
                Light l2 = new Light(new Point3D(float.Parse(textBox1.Text), float.Parse(textBox2.Text), float.Parse(textBox3.Text)), new Point3D(1f, 1f, 1f));//белый, дальний верхний правый угол
                lights.Add(l2);            
            }
            else
            {
                //добавляем источник света
                Light l1 = new Light(new Point3D(0f, 2f, 4.9f), new Point3D(1f, 1f, 1f));//белый, посреди комнаты,как люстра
                lights.Add(l1);
            }
            //зеркальность  параллелепипеда 
            if (checkBox2.Checked)
            {
                bigCube.fMaterial = new Material(1f,0f, 0f, 0.1f,1);
            }
               
            //зеркальность куба
            if (checkBox5.Checked)
            {
                transCube.fMaterial = new Material(1f, 0f, 0f, 0.1f, 1);
            }
        
       
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void BackwardRayTracing()
        {
            /*
      Получение изображения с камеры и заполнение матрицы пикселей, соответствующей размеру экрана.
      */
            get_Image();

            /*
             Количество первичных лучей равно общему количеству пикселей в видовом окне.
             */
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    // луча от поозиции до текущего пикселя
                    Ray r = new Ray(cameraPoint, pixels[i, j]);
                    r.start = new Point3D(pixels[i, j]);

                    // трассировки луча с учетом количества итераций и коэффициента
                    Point3D color = RayTrace(r, 10, 1); 
                    // Нормализация цвета, если он выходит за пределы допустимого диапазона
                    if (color.x > 1.0f)
                    {
                        color.x = 1.0f;
                    }
                    if (color.y > 1.0f)
                    {
                        color.y = 1.0f;
                    }
                    if (color.z > 1.0f)
                    {
                        color.z = 1.0f;
                    }
                    pixels_color[i, j] = Color.FromArgb((int)(255 * color.x), (int)(255 * color.y), (int)(255 * color.z));
                }
            }
        }

        // получение всех пикселей сцены
        public void get_Image()
        {
           
            pixels = new Point3D[w, h];
            pixels_color = new Color[w, h];
            // шаги по вертикали и горизонтали для пикселей
            Point3D delta_y = (up_left - down_left) / (h - 1);
            Point3D delta_x = (up_right - up_left) / (w - 1);

            Point3D current_point = down_left;

            for (int i = 0; i < h; ++i)
            {
                Point3D current_row_point = current_point;
                for (int j = 0; j < w; ++j)
                {
                    pixels[j, i] = current_row_point;
                    current_row_point += delta_x;
                }
                current_point += delta_y;
            }
        }

        public bool IsVisible(Point3D light_point, Point3D hit_point)
        {
            float distanceToLight = (light_point - hit_point).length(); // Расстояние до источника света от точки пересечения

            Ray rayToLight = new Ray(hit_point, light_point);

            foreach (Object fig in scene)
            {
                if (fig.FigureIntersection(rayToLight, out float t, out Point3D n) && t < distanceToLight && t > Object.eps)
                {
                    return false; // если  обнаружено пересечение объекта со светом, то точка не видима
                }
            }

            return true; // если ни с одним объектом не было обнаружено пересечение, то точка видима
        }

        public Point3D RayTrace(Ray r, int iter, float env)
        {
            if (iter <= 0)
                return new Point3D(0, 0, 0);
            float rey_fig_intersect = 0;// позиция точки пересечения луча с фигурой на луче
            //нормаль стороны фигуры,с которой пересекся луч
            Point3D normal = null;
            Material material = new Material();
            Point3D res_color = new Point3D(0, 0, 0);
            //угол падения острый
            bool refract_out_of_figure = false;

            foreach (Object fig in scene)
            {
                if (fig.FigureIntersection(r, out float intersect, out Point3D norm))
                    if (intersect < rey_fig_intersect || rey_fig_intersect == 0)// нужна ближайшая фигура к точке наблюдения
                    {
                        rey_fig_intersect = intersect;
                        normal = norm;
                        material = new Material(fig.fMaterial);
                    }
            }

            if (rey_fig_intersect == 0)//если не пересекается с фигурой
                return new Point3D(0, 0, 0);//Луч уходит в свободное пространство .Возвращаем значение по умолчанию

            //угол между направление луча и нормалью стороны острый
            //определяем из какой среды в какую
            if (Point3D.scalar(r.direction, normal) > 0)
            {
                normal *= -1;
                refract_out_of_figure = true;
            }

            //Точка пересечения луча с фигурой
            Point3D hit_point = r.start + r.direction * rey_fig_intersect;
            /*В точке пересечения луча с объектом строится три вторичных
              луча – один в направлении отражения (1), второй – в направлении
              источника света (2), третий в направлении преломления
              прозрачной поверхностью (3).
             */

            foreach (Light light in lights)
            {
                //цвет коэффициент принятия фонового освещения
                Point3D ambient_coef = light.color_light * material.ambient;
                ambient_coef.x = (ambient_coef.x * material.color.x);
                ambient_coef.y = (ambient_coef.y * material.color.y);
                ambient_coef.z = (ambient_coef.z * material.color.z);
                res_color += ambient_coef;
                // диффузное освещение
                if (IsVisible(light.point_light, hit_point))//если точка пересечения луча с объектом видна из источника света
                    res_color += light.Shade(hit_point, normal, material.color, material.diffuse);
            }

            /*Для отраженного луча
              проверяется возможность
              пересечения с другими
              объектами сцены.

                Если пересечений нет, то
                интенсивность и цвет
                отраженного луча равна
                интенсивности и цвету фона.

                Если пересечение есть, то в
                новой точке снова строится
                три типа лучей – теневые,
                отражения и преломления. 
              */
            if (material.reflection > 0)
            {
                Ray reflected_ray = r.Reflect(hit_point, normal);
                res_color += material.reflection * RayTrace(reflected_ray, iter - 1, env);//получаем цвет и учитываем его в итоговом цвете текущего луча
            }

            if (material.refraction > 0)
            {
                //взависимости от того,из какой среды в какую,будет меняться коэффициент преломления
                float refract_coef;
                if (refract_out_of_figure)
                    refract_coef = material.environment;
                else
                    refract_coef = 1 / material.environment;

                Ray refracted_ray = r.Refract(hit_point, normal, material.refraction, refract_coef);//создаем преломленный луч

                /*
                 Как и в предыдущем случае,
                 проверяется пересечение вновь
                 построенного луча с объектами,
                 и, если они есть, в новой точке
                 строятся три луча, если нет – используется интенсивность и
                 цвет фона.
                 */
                if (refracted_ray != null)
                    res_color += material.refraction * RayTrace(refracted_ray, iter - 1, material.environment);
            }
            return res_color;
        }
    }
}

