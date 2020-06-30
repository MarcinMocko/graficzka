using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace GrafikaProjekt
{
   
    public partial class Scene3D : Form
    {
        Vector<float> vcamera; // punkt w którym jest kamera
        Vector<float> vLookDir; //kierunek który kamera wskazuje
        float fYaw = 0; //rotacje kamery xz w fps
        float Theta = 0; //to chyba do wyrzucenia
        Mesh mesh;
        Bitmap b;
        float near = 0.1f;
        float far = 1000.0f;
        float fovdegrees = 90.0f;
        public Scene3D()
        {
            InitializeComponent();
            //bitmapa do wyświetlania
            vcamera = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, 0, 1.0f });
            vLookDir = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, 0, 1.0f });
            //wczytywanie obiektu
            mesh = new Mesh();
            mesh.LoadObject(Path.Combine(Directory.GetCurrentDirectory(), "GRAFIKA.obj"));
            //bufor do przechowywania mapy
            //Matrix<float> z_bufor = Matrix<float>.Build.Dense(1500, 750, float.PositiveInfinity);
            //siatka do?
            b = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            
           
        }

        #region funkcje
        public Matrix<float> Identity()
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            mat[0, 0] = 1.0f;
            mat[1, 1] = 1.0f;
            mat[2, 2] = 1.0f;
            mat[3, 3] = 1.0f;
            return mat;
        }
        //translation matrix
        public Matrix<float> Translation(float x, float y, float z)
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            mat[0,0] = 1.0f;
            mat[1,1] = 1.0f;
            mat[2,2] = 1.0f;
            mat[3,3] = 1.0f;
            mat[3,0] = x;
            mat[3,1] = y;
            mat[3,2] = z;
            return mat;
        }
        //Pointat
        public Matrix<float> Pointmatrix(Vector<float> pos, Vector<float> target, Vector<float> up)
        {
            //new forward
            Vector<float> newforward = target - pos;
            newforward = Norm(newforward);
            //new up
            Vector<float> newup = newforward * (up.DotProduct(newforward));
            newup = Norm(newforward);
            //new right
            Vector<float> newright = Crossproduct(newup, newforward);
            //tworzenie macierzy translation i dimensioning
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            mat[0,0] = newright[0];
            mat[0,1] = newright[1];
            mat[0,2] = newright[2];
            mat[0,3] = 0.0f;
            mat[1,0] = newup[0];
            mat[1,1] = newup[1];
            mat[1,2] = newup[2];
            mat[1,3] = 0.0f;
            mat[2,0] = newforward[0];
            mat[2,1] = newforward[1];
            mat[2,2] = newforward[2];
            mat[2,3] = 0.0f;
            mat[3,0] = pos[0];
            mat[3,1] = pos[1];
            mat[3,2] = pos[2];
            mat[3,3] = 1.0f;

            return mat;
        }
        //macierz rzutowania
        public Matrix<float> Projection(float fovdegrees, float aspectratio, float near, float far)
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            float fovrad = 1.0f / (float)Math.Tan((fovdegrees*0.5f)/(180.0f*(float)Math.PI));
            //float aspectfarnear = far / (far - near);
            mat[0, 0] = aspectratio * fovrad;
            mat[1, 1] = fovrad;
            mat[2, 2] = far/(far-near);
            mat[3, 2] = (-far * near)/(far-near);
            mat[2, 3] = 1.0f;
            return mat;
        }
        public Matrix<float> RotateX(float angle)
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            mat[0, 0] = 1.0f;
            mat[1, 1] = (float)Math.Cos(angle);
            mat[1, 2] = (float)Math.Sin(angle);
            mat[2, 1] = -(float)Math.Sin(angle);
            mat[2, 2] = (float)Math.Cos(angle);
            mat[3, 3] = 1.0f;
            return mat;
        }
        //macierz obrót y
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
        //macierz obrót z
        public Matrix<float> RotateZ(float angle)
        {
            Matrix<float> mat = Matrix<float>.Build.Dense(4, 4);
            mat[0, 0] = (float)Math.Cos(angle);
            mat[0, 1] = (float)Math.Sin(angle);
            mat[1, 0] = -(float)Math.Sin(angle);
            mat[2, 1] = (float)Math.Cos(angle);
            mat[2, 2] = 1.0f;
            mat[3, 3] = 1.0f;
            return mat;
        }
        #endregion
        #region operacje na macierzach i wektorach
        public static Vector<float> vecAdd(Vector<float> v1, Vector<float> v2)
        {
            Vector<float> vec = Vector<float>.Build.Dense(3);
            for (int i = 0; i < 3; i++)
            {
                vec[i] = v1[i] + v2[i];
            }
            return vec;
        }
        public static Vector<float> vecSub(Vector<float> v1, Vector<float> v2)
        {
            Vector<float> vec = Vector<float>.Build.Dense(3);
            for (int i = 0; i < 3; i++)
            {
                vec[i] = v1[i] - v2[i];
            }
            return vec;
        }
        public static Vector<float> vecMultiply(Vector<float> v1, float v2)
        {
            Vector<float> vec = Vector<float>.Build.Dense(3);
            for (int i = 0; i < 3; i++)
            {
                vec[i] = v1[i] * v2;
            }
            return vec;
        }
        public static Vector<float> vecDiv(Vector<float> v1, float v2)
        {
            Vector<float> vec = Vector<float>.Build.Dense(3);
            for (int i = 0; i < 3; i++)
            {
                vec[i] = v1[i]/ v2;
            }
            return vec;
        }
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
        //rysowanie trójkątów 
        public void DrawTriangles(Mesh mesh, Bitmap b)
        {

            //bitmapa i graphics do pola na rysowanie

            Graphics g = Graphics.FromImage(b);
            //zmienne do macierzy rzutu

            float aspectRatio = (float)b.Height / b.Width;

            //macierz rzutu
            Matrix<float> projectionmatrix = Projection(fovdegrees, aspectRatio, near, far);

            //macierze obrotów
            Matrix<float> rotateX = RotateX(Theta);
            Matrix<float> rotateZ = RotateZ(Theta);

            Matrix<float> matTrans = Translation(0.0f, 0.0f, 5.0f);

            Matrix<float> matWorld = Identity();
            //obrót
            matWorld = rotateZ * rotateX;
            //przekształcenie
            matWorld = matWorld * matTrans;
            //pointat
            Vector<float> vUp = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
            Vector<float> vTarget = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 0.0f, 1.0f, 1.0f });
            Matrix<float> matCameraRot = RotateY(fYaw);
            //to usunąć
            vLookDir = matCameraRot * vTarget;
            vTarget = vcamera + vLookDir;
            Matrix<float> matCamera = Pointmatrix(vcamera, vTarget, vUp);

            //viev matrix from camera
            Matrix<float> matView = matCamera.Inverse();

            List<Triangle> vecTrianglesToRaster;

            //RYSOWANIE TRÓJKĄTÓW
            foreach (var t in mesh.meshtri)
            {
                //wektory x,y,z,1 z wierzchołków

                Vector<float> v1 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vertarr[0].x, t.vertarr[0].y ,t.vertarr[0].z, 1
                });
                Vector<float> v2 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vertarr[1].x, t.vertarr[1].y ,t.vertarr[1].z, 1
                    });
                Vector<float> v3 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vertarr[2].x, t.vertarr[2].y ,t.vertarr[2].z, 1
                });
                //
                Vector<float> normal, line, line2;
                line = v2 - v1;
                line[3] = 1.0f;
                line2 = v3 - v1;
                line2[3] = 1.0f;
                normal = Crossproduct(line, line2);
                normal = Norm(normal);
                Vector<float> vCameraRay = v1 - vcamera;
                if (dotProd(normal, vCameraRay) < 0.0f)
                {
                    using(var e = Graphics.FromImage(b))
                    { 
                    //illumination
                    Vector<float> lightdirection = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
                    //eee
                    float dp = Math.Max(0.1f, dotProd(lightdirection, normal));
                    //wybrać rgb
                    SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, (int)(255.0f-(dp * 255.0f))));
                    //rzut
                    //odsunąć ze środka

                    //world space --> view space
                    v1[2] += 10.0f;
                    v2[2] += 10.0f;
                    v3[2] += 10.0f;
                    //v1 *= matView;
                    //v2 *= matView;
                    //v3 *= matView;

                    v1 *= projectionmatrix;
                    v2 *= projectionmatrix;
                    v3 *= projectionmatrix;
                    v1 /= v1[3];
                    v2 /= v2[3];
                    v3 /= v3[3];

                    //odwrócony widok więc odwrócić
                    //v1[0] *= -1.0f;
                    //v1[1] *= -1.0f;
                    //v2[0] *= -1.0f;
                    //v2[1] *= -1.0f;
                    //v3[0] *= -1.0f;
                    //v3[1] *= -1.0f;

                    //v1[1] += 2.0f;
                    //v2[1] += 2.0f;
                    //v3[1] += 2.0f;

                     for (int i = 0; i <= 1; i++)
                    {
                        v1[i] += 1.0f;
                        v2[i] += 1.0f;
                        v3[i] += 1.0f;
                    }
                    v1[0] *= 0.25f * b.Width;
                    v1[1] *= 0.25f * b.Height;
                    v2[0] *= 0.25f * b.Width;
                    v2[1] *= 0.25f * b.Height;
                    v3[0] *= 0.25f * b.Width;
                    v3[1] *= 0.25f * b.Height;

                    PointF point1 = new PointF(v1[0], v1[1]);
                    PointF point2 = new PointF(v2[0], v2[1]);
                    PointF point3 = new PointF(v3[0], v3[1]);
                    PointF[] pointarr = { point1, point2, point3 };
                    // Triangle triangle = new Triangle(v1,v2,v3);

                    e.DrawPolygon(new Pen(Color.Black), pointarr);
                        //e.Graphics.FillPolygon(brush, pointarr);
                    }
                }


            }

        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        //wciskanie klawiszy
        //wsad - przesuwanie wzdłuż z i obroty
        //strzałki - przesuwanie wzdłuż x i y
        #region obsługa klawiszy
        private void Scene3D_KeyDown(object sender, KeyEventArgs e)
        {
            //Vector<float> vforward = 8.0f * vLookDir;
            if (e.KeyCode == Keys.Up)
            {
                vcamera[1] += 5.0f;

            }
            if (e.KeyCode == Keys.Down)
            {
                vcamera[1] -= 5.0f;
            }
            if (e.KeyCode == Keys.Left)
            {
                vcamera[0] -= 5.0f;
            }
            if (e.KeyCode == Keys.Right)
            {
                vcamera[0] += 5.0f;
            }
            if (e.KeyCode == Keys.W)
            {
                Vector<float> vforward = 5.0f * vLookDir;
                vcamera += vforward;
            }
            if (e.KeyCode == Keys.S)
            {
                Vector<float> vforward = 5.0f * vLookDir;
                vcamera -= vforward;
            }
            if (e.KeyCode == Keys.A)
            {
                fYaw -= 5.0f;
            }
            if (e.KeyCode == Keys.D)
            {
                fYaw += 5.0f;
            }
            //pictureBox1.Image = 
        }
        #endregion
        #region do usunięcia
        private void Scene3D_Move(object sender, EventArgs e)
        {
           
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
        }
        private void Form1_Load(object sender, EventArgs e)
        {


        }
        #endregion
        //rysowanie
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            //bitmapa i graphics do pola na rysowanie

            Graphics g = Graphics.FromImage(b);
            //zmienne do macierzy rzutu

            float aspectRatio = (float)b.Height / b.Width;

            //macierz rzutu
            Matrix<float> projectionmatrix = Projection(fovdegrees, aspectRatio, near, far);

            //macierze obrotów
            Matrix<float> rotateX = RotateX(Theta);
            Matrix<float> rotateZ = RotateZ(Theta);

            Matrix<float> matTrans = Translation(0.0f, 0.0f, 5.0f);

            Matrix<float> matWorld = Identity();
            //obrót
            matWorld = rotateZ * rotateX;
            //przekształcenie
            matWorld = matWorld * matTrans;
            //pointat
            Vector<float> vUp = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
            Vector<float> vTarget = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 0.0f, 1.0f, 1.0f });
            Matrix<float> matCameraRot = RotateY(fYaw);
            //to usunąć
            vLookDir = matCameraRot * vTarget;
            vTarget = vcamera + vLookDir;
            Matrix<float> matCamera = Pointmatrix(vcamera, vTarget, vUp);

            //viev matrix from camera
            Matrix<float> matView = matCamera.Inverse();

            List<Triangle> vecTrianglesToRaster;

            //RYSOWANIE TRÓJKĄTÓW
            foreach (var t in mesh.meshtri)
            {
                //wektory x,y,z,1 z wierzchołków

                Vector<float> v1 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vertarr[0].x, t.vertarr[0].y ,t.vertarr[0].z, 1
                });
                Vector<float> v2 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vertarr[1].x, t.vertarr[1].y ,t.vertarr[1].z, 1
                    });
                Vector<float> v3 = Vector<float>.Build.DenseOfArray(new float[] {
                    t.vertarr[2].x, t.vertarr[2].y ,t.vertarr[2].z, 1
                });
                //
                Vector<float> normal, line, line2;
                line = v2 - v1;
                line[3] = 1.0f;
                line2 = v3 - v1;
                line2[3] = 1.0f;
                normal = Crossproduct(line, line2);
                normal = Norm(normal);
                Vector<float> vCameraRay = vcamera-v1;
                vCameraRay[3] = 1.0f;
                if (dotProd(normal, vCameraRay) < 0.0f)
                {

                    //illumination
                    Vector<float> lightdirection = Vector<float>.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
                    //eee
                    float dp = Math.Max(0.1f, dotProd(lightdirection, normal));
                    //wybrać rgb
                    SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, (int)(255.0f - (dp * 255.0f))));
                    //rzut
                    //odsunąć ze środka

                    //world space --> view space
                    v1[2] += 10.0f;
                    v2[2] += 10.0f;
                    v3[2] += 10.0f;
                    v1 *= matView;
                    v2 *= matView;
                    v3 *= matView;

                    v1 *= projectionmatrix;
                    v2 *= projectionmatrix;
                    v3 *= projectionmatrix;
                    v1 /= v1[3];
                    v2 /= v2[3];
                    v3 /= v3[3];

                    //odwrócony widok więc odwrócić
                    v1[0] *= -1.0f;
                    v1[1] *= -1.0f;
                    v2[0] *= -1.0f;
                    v2[1] *= -1.0f;
                    v3[0] *= -1.0f;
                    v3[1] *= -1.0f;

                    v1[1] += 2.0f;
                    v2[1] += 2.0f;
                    v3[1] += 2.0f;

                    for (int i = 0; i <= 1; i++)
                    {
                        v1[i] += 1.0f;
                        v2[i] += 1.0f;
                        v3[i] += 1.0f;
                    }
                    v1[0] *= 0.25f * b.Width;
                    v1[1] *= 0.25f * b.Height;
                    v2[0] *= 0.25f * b.Width;
                    v2[1] *= 0.25f * b.Height;
                    v3[0] *= 0.25f * b.Width;
                    v3[1] *= 0.25f * b.Height;

                    PointF point1 = new PointF(v1[0], v1[1]);
                    PointF point2 = new PointF(v2[0], v2[1]);
                    PointF point3 = new PointF(v3[0], v3[1]);
                    PointF[] pointarr = { point1, point2, point3 };
                    // Triangle triangle = new Triangle(v1,v2,v3);

                    e.Graphics.DrawPolygon(new Pen(Color.Black), pointarr);
                    //e.Graphics.FillPolygon(brush, pointarr);
                }



            }
        }

        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                vcamera[1] += 5.0f;

            }
            if (e.KeyCode == Keys.Down)
            {
                vcamera[1] -= 5.0f;
            }
            if (e.KeyCode == Keys.Left)
            {
                vcamera[0] -= 5.0f;
            }
            if (e.KeyCode == Keys.Right)
            {
                vcamera[0] += 5.0f;
            }
            if (e.KeyCode == Keys.W)
            {
                Vector<float> vforward = 5.0f * vLookDir;
                vcamera += vforward;
            }
            if (e.KeyCode == Keys.S)
            {
                Vector<float> vforward = 5.0f * vLookDir;
                vcamera -= vforward;
            }
            if (e.KeyCode == Keys.A)
            {
                fYaw -= 5.0f;
            }
            if (e.KeyCode == Keys.D)
            {
                fYaw += 5.0f;
            }
        }
    }
}
