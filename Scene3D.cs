using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GrafikaProjekt
{
   
    public partial class Scene3D : Form
    {
        Vector<float> cameraPosition; // punkt w którym jest kamera
        Vector<float> cameraPointerZ; //kierunek który kamera wskazuje
        float fY = 0; //rotacje kamery y 
        Mesh mesh; //siatka z Listą trójkątów
        Bitmap b;
        float near = 1.0f; //odległość widza od najbliższego punktu wyświetlanego
        float far = 1000.0f; //odległość widza od najbliższego punktu wyświetlanego
        float f = 120.0f;// kąt szerokości widzenia w stopniach
        float scalesize = 0.2f;
        Vector<float> lightdirection;
        public Scene3D()
        {
            InitializeComponent();
            //bitmapa do wyświetlania
            //wczytywanie obiektu
            mesh = new Mesh();
            mesh.LoadObject(Path.Combine(Directory.GetCurrentDirectory(), "GRAFIKA.obj"));
            b = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            cameraPosition = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, 0, 1.0f });
            cameraPointerZ = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, 1.0f, 1.0f });
            lightdirection = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.7f });

        }

        #region funkcje
        public Bitmap Drawobject()
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            Graphics g = Graphics.FromImage(b);
            
            float aspectRatio = (float)b.Height / b.Width;
            Matrix<float> projectionmatrix = Projection(f, aspectRatio, near, far);
            Vector<float> UpY = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
            Vector<float> ForwardZ = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 0.0f, 1.0f, 1.0f });
            Matrix<float> CamRotateY = RotateY(fY);
            cameraPointerZ = CamRotateY * ForwardZ;
            ForwardZ = cameraPosition + cameraPointerZ;
            ForwardZ[3] = 1.0f;
            Matrix<float> PointedCamera = Pointmatrix(cameraPosition, ForwardZ, UpY);
            Matrix<float> PointedView = PointedCamera.Inverse();
            List<Triangle> TriangletoColor = new List<Triangle>();
            foreach (var t in mesh.meshtri)
            {

                Vector<float> v1 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vector1[0], t.vector1[1] ,t.vector1[2], 1.0f
                });
                Vector<float> v2 = Vector<float>.Build.DenseOfArray(new float[] {
                   t.vector2[0], t.vector2[1] ,t.vector2[2], 1.0f
                    });
                Vector<float> v3 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vector3[0], t.vector3[1] ,t.vector3[2], 1.0f
                });
                
                Vector<float> normal, vec1, vec2;
                vec1 = v2-v1;
                vec1[3] = 1.0f;
                vec2 = v3-v1;
                vec2[3] = 1.0f;
                normal = Crossproduct(vec1, vec2);
                normal = Norm(normal);

                    float intensity = Math.Max(0.1f, dotProd(lightdirection, normal));
                    SolidBrush brush = new SolidBrush(Color.FromArgb((int)(intensity * 100.0f), (int)(intensity * 100.0f), 0));

                    v1[2] += 10.0f;
                    v2[2] += 10.0f;
                    v3[2] += 10.0f;

                    v1 *= PointedView;
                    v2 *= PointedView;
                    v3 *= PointedView;

                    v1 *= projectionmatrix;
                    v2 *= projectionmatrix;
                    v3 *= projectionmatrix;

                    v1 /= v1[3];
                    v2 /= v2[3];
                    v3 /= v3[3];

                    v1[0] *= -1.0f;
                    v1[1] *= -1.0f;
                    v2[0] *= -1.0f;
                    v2[1] *= -1.0f;
                    v3[0] *= -1.0f;
                    v3[1] *= -1.0f;

                //v1[1] += 2.0f;
                //v2[1] += 2.0f;
                //v3[1] += 2.0f;
                //v1[0] += 1.0f;
                //v2[0] += 1.0f;
                //v3[0] += 1.0f;

                for (int i = 0; i <= 1; i++)
                {
                    v1[i] += 1.0f;
                    v2[i] += 1.0f;
                    v3[i] += 1.0f;
                }

                v1[0] *= scalesize * b.Width;
                    v1[1] *= scalesize * b.Height;
                    v2[0] *= scalesize * b.Width;
                    v2[1] *= scalesize * b.Height;
                    v3[0] *= scalesize * b.Width;
                    v3[1] *= scalesize * b.Height;

                    Triangle triangle = new Triangle(v1, v2, v3);
                    triangle.brush = brush;
                    TriangletoColor.Add(triangle);



            }
            TriangletoColor = TriangletoColor.OrderBy(tri => (tri.vector1[2] + tri.vector2[2] + tri.vector3[2])/3).ToList();
            foreach (var item in TriangletoColor)
            {
                PointF point1 = new PointF(item.vector1[0], item.vector1[1]);
                PointF point2 = new PointF(item.vector2[0], item.vector2[1]);
                PointF point3 = new PointF(item.vector3[0], item.vector3[1]);
                PointF[] pointarr = { point1, point2, point3 };
                try
                {
                    if (item.vector1[2] < 1 || item.vector2[2] < 1 || item.vector3[2] < 1)
                    {
                        g.FillPolygon(item.brush, pointarr);
                    }
                }
                catch (Exception)
                {

                   
                }
              
            }
            
        
            return bitmap;
        }
        //macierz przenosząca kamere w punkt
        public Matrix<float> Pointmatrix(Vector<float> camerapos, Vector<float> forwardZ, Vector<float> upY)
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            //nowy Z
            Vector<float> newforwardZ = forwardZ-camerapos;
            newforwardZ[3] = 1.0f;
            newforwardZ = Norm(newforwardZ);
            // nowy Y
            Vector<float> a = newforwardZ * dotProd(upY, newforwardZ);
            Vector<float> newupY = upY-a;
            newupY[3] = 1.0f;
            newupY = Norm(newupY);
            //nowy X
            Vector<float> newright = Crossproduct(newupY, newforwardZ);
            newright = Norm(newright);
            mat[0, 0] =newright[0];
            mat[0, 1] =newright[1];
            mat[0, 2] =newright[2];
            mat[0, 3] =0.0f;
            mat[1, 0] =newupY[0];
            mat[1, 1] =newupY[1];
            mat[1, 2] =newupY[2];
            mat[1, 3] =0.0f;
            mat[2, 0] =newforwardZ[0];
            mat[2, 1] = newforwardZ[1];
            mat[2, 2] = newforwardZ[2];
            mat[2, 3] =0.0f;
            mat[3, 0] =camerapos[0];
            mat[3, 1] = camerapos[1];
            mat[3, 2] = camerapos[2];
            mat[3, 3] =1.0f;
            return mat;
        }
        //macierz rzutowania
        public Matrix<float> Projection(float f, float aspectratio, float near, float far)
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            float f_in_rad = 1.0f / (float)Math.Tan((f*0.5f)/(180.0f*(float)Math.PI));
            //float aspectfarnear = far / (far - near);
            mat[0, 0] = aspectratio * f_in_rad;
            mat[1, 1] = f_in_rad;
            mat[2, 2] = far/(far-near);
            mat[3, 2] = (-far * near)/(far-near);
            mat[2, 3] = 1.0f;
            return mat;
        }
        //macierz obrót względem y
        public Matrix<float> RotateY(float angle)
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            mat[0, 0] = (float)Math.Cos(angle);
            mat[0, 2] = (float)Math.Sin(angle);
            mat[2, 0] = -(float)Math.Sin(angle);
            mat[1, 1] = 1.0f;
            mat[2, 2] = (float)Math.Cos(angle);
            mat[3, 3] = 1.0f;
            return mat;
        }
        #endregion
        #region operacje na macierzach i wektorach
        public static float dotProd(Vector<float> v1, Vector<float> v2)
        {
            float prod=0;
            for (int i = 0; i < 3; i++)
            {
                prod += v1[i] * v2[i];
            }
            return prod;
        }
        public static float vecLen(Vector<float> v1)
        {
           return (float)Math.Sqrt(dotProd(v1,v1));
        }
        public static Vector<float> Norm(Vector<float> v1)
        {
            float len = vecLen(v1);
            
            for (int i = 0; i < 3; i++)
            {
                v1[i] = v1[i] / len;
            }
            return v1;
        }
        //iloczyn wektorowy
        public Vector<float> Crossproduct(Vector<float> a, Vector<float> b)
        {
            Vector<float> crossproduct = Vector<float>.Build.Dense(3);
            crossproduct[0] = a[1] * b[2] - a[2] * b[1];
            crossproduct[1] = a[2] * b[0] - a[0] * b[2];
            crossproduct[2] = a[0] * b[1] - a[1] * b[0];
            return crossproduct;
        }
        #endregion
        #region obsługa klawiszy
        private void Scene3D_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Up)
            {
                cameraPosition[1] += 1.0f;
            }
            if (e.KeyCode == Keys.Down)
            {
                cameraPosition[1] -= 1.0f;
            }
            if (e.KeyCode == Keys.Left)
            {
               
                cameraPosition[0] += 1.0f;
            }
            if (e.KeyCode == Keys.Right)
            {
               
                cameraPosition[0] -= 1.0f;
            }
            if (e.KeyCode == Keys.W)
            {
                cameraPosition += cameraPointerZ;
            }
            if (e.KeyCode == Keys.S)
            {

               cameraPosition -= cameraPointerZ;
            }
            if (e.KeyCode == Keys.A)
            {
                fY += 0.15f;
            }
            if (e.KeyCode == Keys.D)
            {
                fY -= 0.15f;
            }
            pictureBox1.Image = Drawobject();
        }
        #endregion
        
        //rysowanie
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = Graphics.FromImage(b);
            //zmienne do macierzy rzutu
            float aspectRatio = (float)b.Height / b.Width;
            //macierz rzutu
            Matrix<float> projectionmatrix = Projection(f, aspectRatio, near, far);
            //wektory wskazujące na Y i Z
            Vector<float> UpY = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
            Vector<float> ForwardZ = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 0.0f, 1.0f, 1.0f });
            //macierz obrotu y
             Matrix<float> CamRotateY = RotateY(fY);
            //obracanie kamery względem y 
            cameraPointerZ = CamRotateY * ForwardZ;
            // aktualne wskazanie kamery + pozycja 
            ForwardZ = cameraPosition+cameraPointerZ;
            ForwardZ[3] = 1.0f;
            // macierz przenosząca kamerę w dany punkt
            Matrix<float> PointedCamera = Pointmatrix(cameraPosition, ForwardZ, UpY);
            //odwrócona macierz żeby kamera nie poruszała obiektów
            Matrix<float> PointedView = PointedCamera.Inverse();

            List<Triangle> TriangletoColor = new List<Triangle>();

            //RYSOWANIE TRÓJKĄTÓW
            foreach (var t in mesh.meshtri)
            {
                Vector<float> v1 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vector1[0], t.vector1[1] ,t.vector1[2], 1.0f
                });
                Vector<float> v2 = Vector<float>.Build.DenseOfArray(new float[] {
                   t.vector2[0], t.vector2[1] ,t.vector2[2], 1.0f
                    });
                Vector<float> v3 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vector3[0], t.vector3[1] ,t.vector3[2], 1.0f
                });
                //obliczanie prostopadłej do trójkąta z iloczynu wektorowego
                Vector<float> normal, vec1, vec2;
                vec1 = v2 - v1;
                vec1[3] = 1.0f;
                vec2 = v3 - v1;
                vec2[3] = 1.0f;
                normal = Crossproduct(vec1, vec2);
                normal = Norm(normal);
                //źródło światła
                //intensity - określa jasność koloru
                float intensity = Math.Max(0.1f, dotProd(lightdirection, normal));
                //wybrać rgb
                SolidBrush brush = new SolidBrush(Color.FromArgb((int)(intensity * 100.0f), (int)(intensity * 100.0f), 0));
                //rzut
                //odsunięcie się z początku układu współrzędnych
                v1[2] += 10.0f;
                v2[2] += 10.0f;
                v3[2] += 10.0f;
                //przeniesienie kamery w punkt
                v1 *= PointedView;
                v2 *= PointedView;
                v3 *= PointedView;
                //rzut całego obrazu
                v1 *= projectionmatrix;
                v2 *= projectionmatrix;
                v3 *= projectionmatrix;
                v1 /= v1[3];
                v2 /= v2[3];
                v3 /= v3[3];


                //odwrócony widok
                v1[0] *= -1.0f;
                v1[1] *= -1.0f;
                v2[0] *= -1.0f;
                v2[1] *= -1.0f;
                v3[0] *= -1.0f;
                v3[1] *= -1.0f;

                v1[1] += 2.0f;
                v2[1] += 2.0f;
                v3[1] += 2.0f;
                v1[0] += 1.0f;
                v2[0] += 1.0f;
                v3[0] += 1.0f;

                for (int i = 0; i <= 1; i++)
                {
                    v1[i] += 1.0f;
                    v2[i] += 1.0f;
                    v3[i] += 1.0f;
                }

                v1[0] *= scalesize * b.Width;
                v1[1] *= scalesize * b.Height;
                v2[0] *= scalesize * b.Width;
                v2[1] *= scalesize * b.Height;
                v3[0] *= scalesize * b.Width;
                v3[1] *= scalesize * b.Height;

                Triangle triangle = new Triangle(v1, v2, v3);
                triangle.brush = brush;
                TriangletoColor.Add(triangle);




            }
            TriangletoColor = TriangletoColor.OrderBy(tri => (tri.vector1[2] + tri.vector2[2] + tri.vector3[2])/3).ToList();
            foreach (var item in TriangletoColor)
            {
                PointF point1 = new PointF(item.vector1[0], item.vector1[1]);
                PointF point2 = new PointF(item.vector2[0], item.vector2[1]);
                PointF point3 = new PointF(item.vector3[0], item.vector3[1]);
                PointF[] pointarr = { point1, point2, point3 };
                if (item.vector1[2]<1 || item.vector2[2]< 1 || item.vector3[2]< 1)
                {
                    e.Graphics.FillPolygon(item.brush, pointarr);
                }
                //e.Graphics.DrawPolygon(new Pen(Color.Black), pointarr);
               
            }
           

        }
    }
}
