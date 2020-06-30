using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Single;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Collections;

namespace GrafikaProjekt
{

    public class Vertex
    {
        public float x, y, z;
        public Vertex(float x = 0, float y = 0, float z = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    //vektor3 = Vector<float> vector= Vector<float>(array)
    //matrix 4x4 = Matrix<float> matrix = Matrix<float>.Build.Dense(4,4);
    public class Triangle
    {
        public Color color;
        //public Vector<float> vector1;
        //public Vector<float> vector2;
        //public Vector<float> vector3;
        public Vertex[] vertarr = new Vertex[3];
        //public Vector<float>[] vertarr = Vector<float>[
        public Triangle(Vertex v1, Vertex v2, Vertex v3)
        {
            vertarr[0] = v1;
            vertarr[1] = v2;
            vertarr[2] = v3;

        }
        public Triangle()
        {
          
        }

    }
    public class Mesh : IComparer<Triangle>
    {
       public List<Triangle> meshtri;
        public Mesh()
        {
            meshtri = new List<Triangle>();
        }
        public int Compare(Triangle x, Triangle y)
        {
            float a = (x.vertarr[2].z + x.vertarr[1].z + x.vertarr[0].z)/3.0f;
            float b = (y.vertarr[2].z + y.vertarr[1].z + y.vertarr[0].z)/3.0f;
            if (a>b)
            {
                return 1;
            }
            if (a==b)
            {
                return 0;
            }
            return -1;
        }

        public void LoadObject(string path)
        {
            StreamReader sr = new StreamReader(path);
            List<Vertex> vec = new List<Vertex>(); 
            while (!sr.EndOfStream)
            {
               string a =  sr.ReadLine();
                if (a[0] == 'v')
                {
                    Vertex v = new Vertex();
                    v.x = float.Parse(a.Split(' ')[1], CultureInfo.InvariantCulture);
                    v.y = float.Parse(a.Split(' ')[2], CultureInfo.InvariantCulture);
                    v.z = float.Parse(a.Split(' ')[3], CultureInfo.InvariantCulture);
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
