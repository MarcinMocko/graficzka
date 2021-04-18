using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using System.Drawing;
using System.Globalization;

namespace GrafikaProjekt
{
    public class Triangle
    {
        public SolidBrush brush;
        public Vector<float> vector1 = Vector<float>.Build.Dense(3);
        public Vector<float> vector2 = Vector<float>.Build.Dense(3);
        public Vector<float> vector3 = Vector<float>.Build.Dense(3);
        public Triangle(Vector<float> v1, Vector<float> v2, Vector<float> v3)
        {
            vector1 = v1;
            vector2 = v2;
            vector3 = v3;

        }

    }
    public class Mesh
    {
       public List<Triangle> meshtri;
        public Mesh()
        {
            meshtri = new List<Triangle>();
        }

        public void LoadObject(string path)
        {
            StreamReader sr = new StreamReader(path);
            List<Vector<float>> vec = new List<Vector<float>>(); 
            while (!sr.EndOfStream)
            {
               string a =  sr.ReadLine();
                if (a[0] == 'v')
                {
                    Vector<float> v = Vector<float>.Build.Dense(3);
                    v[0] = float.Parse(a.Split(' ')[1], CultureInfo.InvariantCulture);
                    v[1] = float.Parse(a.Split(' ')[2], CultureInfo.InvariantCulture);
                    v[2] = float.Parse(a.Split(' ')[3], CultureInfo.InvariantCulture);
                    vec.Add(v);
                }
                if (a[0]=='f')
                {
                    int[] f = new int[3];
                    Triangle t;
                    for (int i = 0; i < f.Length; i++)
                    {
                        f[i] = int.Parse(a.Split(' ')[i + 1]);
                    }
                    t = new Triangle(vec[f[0] - 1], vec[f[1] - 1], vec[f[2] - 1]);
                    meshtri.Add(t);
                }
            }

        }
    }
  
    static class Program
    {

       
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Scene3D());
        }
    }
}
