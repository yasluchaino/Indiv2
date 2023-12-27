﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CornishRoom
{
    public class Material
    {
        public float reflection;    // отражения
        public float refraction;    // преломления
        public float ambient;       // фонового освещения
        public float diffuse;       // диффузного освещения
        public float environment;   // преломления среды
        public Point3D color;       // цвет материала

        public Material(float refl, float refr, float amb, float dif, float env = 1)
        {
            reflection = refl;
            refraction = refr;
            ambient = amb;
            diffuse = dif;
            environment = env;
        }

        public Material(Material m)
        {
            reflection = m.reflection;
            refraction = m.refraction;
            environment = m.environment;
            ambient = m.ambient;
            diffuse = m.diffuse;
            color = new Point3D(m.color);
        }

        public Material() {

        }
    }
}
