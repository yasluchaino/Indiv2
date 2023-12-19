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
        public List<Figure> scene = new List<Figure>();
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
            checkBox3.Visible = false;
            checkBox4.Visible = false;
            checkBox5.Visible = false;
            textBox1.Visible = false; 
            textBox2.Visible = false; 
            textBox3.Visible    = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            BuildScene();
           
            BackwardRayTracing(); 
            checkBox1.Visible = true;
            checkBox2.Visible = true; 
            checkBox3.Visible = true;
            textBox1.Visible = true;
            textBox2.Visible = true;
            textBox3.Visible = true;
            checkBox4.Visible = true;
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
            Figure room = Figure.get_Cube(10);
            up_left = room.sides[0].getPoint(0);
            up_right = room.sides[0].getPoint(1);
            down_right = room.sides[0].getPoint(2);
            down_left = room.sides[0].getPoint(3);

            //ставим камеру
            Point3D normal = Side.norm(room.sides[0]);                            // нормаль стороны комнаты
            Point3D center = (up_left + up_right + down_left + down_right) / 4;   // центр стороны комнаты
            cameraPoint = center + normal * 11;

            //красим стены
            room.SetPen(new Pen(Color.Gray));
            room.sides[0].drawing_pen = new Pen(Color.Beige);
            room.sides[1].drawing_pen = new Pen(Color.Violet);
            room.sides[2].drawing_pen = new Pen(Color.Yellow);
            room.sides[3].drawing_pen = new Pen(Color.Red);
            
            room.fMaterial = new Material(0, 0, 0.05f, 0.7f);
    
            Sphere mirrirSphere = new Sphere(new Point3D(2f, 0f, -1f), 1.3f);
                mirrirSphere.SetPen(new Pen(Color.White));
          
            mirrirSphere.fMaterial = new Material(0.9f, 0f, 0f, 0.1f, 1f);
            Figure bigCube = Figure.get_Rectangle(1.5f,2f,6f);
            bigCube.Offset(-0.5f, 2f, -4.6f);
            bigCube.RotateAround(-52, "CZ");
            bigCube.SetPen(new Pen(Color.HotPink));
            bigCube.fMaterial = new Material(0f, 0f, 0.3f, 0.7f, 1f);
            Figure transCube = Figure.get_Cube(2f);
            transCube.Offset(-3f, 2.5f, -3.9f);
            transCube.RotateAround(-10, "CZ");
            transCube.SetPen(new Pen(Color.Red));
            transCube.fMaterial = new Material(0f, 0f, 0.3f, 0.7f, 1f);
            scene.Add(room);
            scene.Add(mirrirSphere);
            scene.Add(bigCube);
            scene.Add(transCube);
            
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
     
            //прозрачность параллелепипеда 
            if (checkBox4.Checked)
            {
                bigCube.fMaterial = new Material(0f, 0.7f, 0.3f, 0.7f, 1f);
            }
     
            //прозрачность куба
            if (checkBox3.Checked)
            {
                transCube.fMaterial = new Material(0f, 0.7f, 0.3f, 0.7f, 1f);
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

        public void BackwardRayTracing()
        {
            /*
             Изображние с камеры в матрице пикселей,равной размеру экрана
             */
            GetPixels();
            /*
             Количество первичных лучей также известно – это общее
             количество пикселей видового окна
             */
            for (int i = 0; i < w; ++i)
                for (int j = 0; j < h; ++j)
                {
                    Ray r = new Ray(cameraPoint, pixels[i, j]);
                    r.start = new Point3D(pixels[i, j]);
                    Point3D color = RayTrace(r, 10, 1);//луч,кол-во итераций,коэфф
                    if (color.x > 1.0f || color.y > 1.0f || color.z > 1.0f)
                        color = Point3D.norm(color);
                    pixels_color[i, j] = Color.FromArgb((int)(255 * color.x), (int)(255 * color.y), (int)(255 * color.z));
                }
        }

        // получение всех пикселей сцены
        public void GetPixels()
        {
            /*
             Учитывая разницу между размером комнаты и экранным отображение приводим координаты к пикселям
             */
            pixels = new Point3D[w, h];
            pixels_color = new Color[w, h];
            Point3D step_up = (up_right - up_left) / (w - 1);//отношение ширины комнаты к ширине экрана
            Point3D step_down = (down_right - down_left) / (w - 1);//отношение высоты комнаты к высоте экрана
            Point3D up = new Point3D(up_left);
            Point3D down = new Point3D(down_left);
            for (int i = 0; i < w; ++i)
            {
                Point3D step_y = (up - down) / (h - 1);
                Point3D d = new Point3D(down);
                for (int j = 0; j < h; ++j)
                {
                    pixels[i, j] = d;
                    d += step_y;
                }
                up += step_up;
                down += step_down;
            }
        }

        //видима ли точка пересечения луча с фигурой из источника света
        public bool IsVisible(Point3D light_point, Point3D hit_point)
        {
            float max_t = (light_point - hit_point).length(); //позиция источника света на луче
            Ray r = new Ray(hit_point, light_point);
            foreach (Figure fig in scene)
                if (fig.FigureIntersection(r, out float t, out Point3D n))
                    if (t < max_t && t > Figure.eps)
                        return false;
            return true;
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

            foreach (Figure fig in scene)
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
                res_color += material.reflection * RayTrace(reflected_ray, iter - 1, env);
            }


            if (material.refraction > 0)
            {
                //взависимости от того,из какой среды в какую,будет меняться коэффициент приломления
                float refract_coef;
                if (refract_out_of_figure)
                    refract_coef = material.environment;
                else
                    refract_coef = 1 / material.environment;

                Ray refracted_ray = r.Refract(hit_point, normal, material.refraction, refract_coef);//создаем приломленный луч

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

