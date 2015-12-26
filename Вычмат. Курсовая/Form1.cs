using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Numerics;

namespace Вычмат.Курсовая
{
    public partial class Form1 : Form
    {
        int N;
        double[,] Matrix;// Исходная матрица
        public Form1()
        {
            InitializeComponent();
            ComputeButton.Hide();
            StreamWriter sw = new StreamWriter("input.txt");
            int n = 500;
            Random rnd = new Random();            // Генерация случайной матрицы в файл input.txt
            sw.WriteLine(n.ToString());
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    sw.WriteLine(100 * rnd.NextDouble());
            sw.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FileButton_Click(object sender, EventArgs e)// Ввод из файла
        {
            String FileName = FileTextBox.Text;
            StreamReader sr;
            try
            {
                sr = new StreamReader(FileName);
            }
            catch(FileNotFoundException)
            {
                MessageBox.Show("Файл не найден");
                return;
            }
            int n = Convert.ToInt32(sr.ReadLine());
            this.N = n;
            Matrix = new double[n, n];
            for (int i = 0; i < Math.Min(500, n); i++)
            {
                dataGridView.Columns.Add("", (i + 1).ToString());// Заполнение таблицы
            }
            dataGridView.Rows.Add(n);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Matrix[i, j] = Convert.ToDouble(sr.ReadLine());
                    if(i<500&&j<500)
                        dataGridView.Rows[i].Cells[j].Value = Matrix[i, j];
                }
            }

            ComputeButton.Show();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ComputeButton_Click(object sender, EventArgs e)
        {
            int amount;
            try
            {
                amount = Convert.ToInt32(amountTextBox.Text);
                if (amount > N)
                    throw new AmountException();
            }
            catch(FormatException)
            {
                MessageBox.Show("Вы неправильно ввели кол-во собственных чисел");
                return;
            }
            catch(AmountException)
            {
                MessageBox.Show("Кол-во собственных чисел указанных вами больше размера матрицы");
                return;
            }
            DateTime dt1 = DateTime.Now;
            double[,] H = Householder(Matrix);
            H = QRalgorithm(H,0.01);
            DateTime dt2 = DateTime.Now;
            MessageBox.Show((dt2 - dt1).ToString());
            for (int i = 0; i < Math.Min(500, N); i++)
            {
                for (int j = 0; j < Math.Min(500, N); j++)
                {
                    dataGridView.Rows[i].Cells[j].Value = "";
                }

            }
            for (int i = 0; i < Math.Min(500, amount); i++)
            {
                for (int j = 0; j < Math.Min(500, amount); j++)
                {
                    dataGridView.Rows[i].Cells[j].Value = H[i, j];
                }

            }
            StreamWriter sw = new StreamWriter("output.txt");
            Complex[,] Shifts = Eigenvals(H);
            for (int i = 0; i < Math.Min(500, amount) ; i++)
            {
                listBox.Items.Add(Shifts[i, 0].ToString("0.000"));
                sw.WriteLine(Shifts[i, 0].ToString("0.0000000"));
            }
            sw.Close();
        }
        double[,] Submatrix(double[,] A, int m1, int m2, int n1, int n2)// Функция, возвращающая подматрицу
        {
            double[,] C = new double[m2 - m1 + 1, n2 - n1 + 1];
            for (int i = m1; i <= m2; i++)
                for (int j = n1; j <= n2; j++)
                {
                    C[i - m1, j - n1] = A[i, j];
                }
            return C;
        }
        double[,] Transpose(double[,] A)// Транспонирование
        {
            int m = A.GetLength(0);
            int n = A.GetLength(1);
            double[,] C = new double[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    C[i, j] = A[j, i];
                }
            }
            return C;
        }
        double AbsVector(double[,] x)// Модуль вектора
        {
            double sum = 0;
            for (int i = 0; i < x.GetLength(0); i++)
            {
                sum += Math.Pow(x[i, 0], 2.0);
            }
            return Math.Pow(sum, 0.5);
        }
        double[,] Multiply(double[,] A, double[,] B)// Умножение матриц
        {
            if (A.GetLength(1) != B.GetLength(0))
            {
                MessageBox.Show("Something Wrong");
            }
            int m = A.GetLength(0);
            int k = B.GetLength(1);
            int n = A.GetLength(1);
            double[,] C = new double[m, k];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    C[i, j] = 0;
                    for (int r = 0; r < n; r++)
                    {
                        C[i, j] += A[i, r] * B[r, j];
                    }

                }
            }
            return C;
        }
        double[,] Multiply(double[,] A, double w)// Умножение матрицы на вектор
        {
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    A[i, j] *= w;
                }
            }
            return A;
        }
        double[,] Plus(double[,] A, double[,] B)// Сложение матриц
        {
            double[,] C = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < A.GetLength(0); i++)
            {
                for (int j = 0; j < A.GetLength(1); j++)
                {
                    C[i, j] = A[i, j] + B[i, j];
                }
            }
            return C;
        }
        double[,] House(double[,] x)// Получение вектора Хаусхолдера
        {
            int n = x.GetLength(0);
            double u = AbsVector(x);
            if (u != 0)
            {
                double b = x[0, 0] + Math.Sign(x[0, 0]) * u;
                for (int i = 1; i < n; i++)
                {
                    x[i, 0] = x[i, 0] / b;
                }
            }
            x[0, 0] = 1;
            return x;
        }
        double[,] rowhouse(double[,] A, double[,] x)// Умножение слева на матрицу Хаусхолдера
        {
            double b = (-2) / Multiply(Transpose(x), x)[0, 0];
            double[,] w = Multiply(Multiply(Transpose(A), x), b);
            return Plus(A, Multiply(x, Transpose(w)));
        }
        double[,] colhouse(double[,] A, double[,] x)// Умножение справа на матрицу Хаусхолдера
        {
            double b = (-2) / Multiply(Transpose(x), x)[0, 0];
            double[,] w = Multiply(Multiply(A, x), b);
            return Plus(A, Multiply(w, Transpose(x)));
        }
        double[,] QRstepFrancis(double[,] H)// QR-разложение Френсиса
        {
            int n = H.GetLength(0) - 1;
            int m = n - 1;
            double s = H[m, m] + H[n, n];
            double t = H[m, m] * H[n, n] - H[m, n] * H[n, m];
            double x = H[0, 0] * H[0, 0] + H[0, 1] * H[1, 0] - s * H[0, 0] + t;
            double y = H[1, 0] * (H[0, 0] + H[1, 1] - s);
            double z = H[1, 0] * H[2, 1];
            for (int k = -1; k <= n - 3; k++)
            {
                double[,] p = new double[,] { { x }, { y }, { z } };
                double[,] u = House(p);
                int q = Math.Max(0, k);
                double[,] H1 = rowhouse(Submatrix(H, k + 1, k + 3, q, n), u);
                for (int i = k + 1; i <= k + 3; i++)
                    for (int j = q; j <= n; j++)
                    {
                        H[i, j] = H1[i - k - 1, j - q];
                    }
                int r = Math.Min(k + 4, n);
                double[,] H2 = colhouse(Submatrix(H, 0, r, k + 1, k + 3), u);
                for (int i = 0; i <= r; i++)
                    for (int j = k + 1; j <= k + 3; j++)
                    {
                        H[i, j] = H2[i, j - k - 1];
                    }
                x = H[k + 2, k + 1];
                y = H[k + 3, k + 1];
                if (k < n - 3)
                {
                    z = H[k + 4, k + 1];
                }
            }
            double[,] l = new double[,] { { x }, { y } };
            double[,] v = House(l);
            double[,] H3 = rowhouse(Submatrix(H, n - 1, n, n - 2, n), v);
            for (int i = n - 1; i <= n; i++)
                for (int j = n - 2; j <= n; j++)
                {
                    H[i, j] = H3[i - n + 1, j - n + 2];
                }
            double[,] H4 = colhouse(Submatrix(H, 0, n, n - 1, n), v);
            for (int i = 0; i <= n; i++)
                for (int j = n - 1; j <= n; j++)
                {
                    H[i, j] = H4[i, j - n + 1];
                }
            return H;
        }
        double[,] QRstepFrancis(double[,] H, Complex root1,Complex root2)// QR-разложение с заранее заданными сдвигами
        {
            int n = H.GetLength(0) - 1;
            int m = n - 1;
            double x = Complex.Abs(Math.Pow(H[0, 0], 2.0) - (root1 + root2) * H[0, 0] + root1 * root2 + H[1, 0] * H[0, 1]);
            double y = Complex.Abs(H[1, 0] * ((H[0, 0] + H[1, 1]) - root1 + root2));
            double z = H[1, 0] * H[0, 1];
            for (int k = -1; k <= n - 3; k++)
            {
                double[,] p = new double[,] { { x }, { y }, { z } };
                double[,] u = House(p);
                int q = Math.Max(0, k);
                double[,] H1 = rowhouse(Submatrix(H, k + 1, k + 3, q, n), u);
                for (int i = k + 1; i <= k + 3; i++)
                    for (int j = q; j <= n; j++)
                    {
                        H[i, j] = H1[i - k - 1, j - q];
                    }
                int r = Math.Min(k + 4, n);
                double[,] H2 = colhouse(Submatrix(H, 0, r, k + 1, k + 3), u);
                for (int i = 0; i <= r; i++)
                    for (int j = k + 1; j <= k + 3; j++)
                    {
                        H[i, j] = H2[i, j - k - 1];
                    }
                x = H[k + 2, k + 1];
                y = H[k + 3, k + 1];
                if (k < n - 3)
                {
                    z = H[k + 4, k + 1];
                }
            }
            double[,] l = new double[,] { { x }, { y } };
            double[,] v = House(l);
            double[,] H3 = rowhouse(Submatrix(H, n - 1, n, n - 2, n), v);
            for (int i = n - 1; i <= n; i++)
                for (int j = n - 2; j <= n; j++)
                {
                    H[i, j] = H3[i - n + 1, j - n + 2];
                }
            double[,] H4 = colhouse(Submatrix(H, 0, n, n - 1, n), v);
            for (int i = 0; i <= n; i++)
                for (int j = n - 1; j <= n; j++)
                {
                    H[i, j] = H4[i, j - n + 1];
                }
            return H;
        }
        double[,] QRalgorithm(double[,] H, double tol)// QR-алгоритм
        {
            int n = H.GetLength(0) - 1;
            int q = 0;
            int p = 0;
            bool flag = false;
            while (p != -1 && q != -1 && !flag)
            {
                q = -1;
                flag = false;
                p = -1;
                for (int i = 1; i <= n; i++)
                    for (int j = 0; j <= i - 1; j++)
                        if (Math.Abs(H[i, j]) < tol)
                            H[i, j] = 0;
                /*for (int i = 1; i <= n;i++ )
                {
                    if(Math.Abs(H[i,i-1]) < tol*(Math.Abs(H[i,i]) + Math.Abs(H[i-1,i-1])))
                    {
                        H[i, i - 1] = 0;
                    }
                }*/
                    for (int i = 0; i <= n - 1; i++)
                    {
                        if (H[i + 1, i] != 0 && !flag)
                        {
                            flag = true;
                            p = i;
                        }
                        if (H[i + 1, i] == 0 && flag)
                        {
                            q = i;
                            if (q == p + 1)
                            {
                                flag = false;
                                p = -1;
                                q = -1;
                            }
                            if (q > p + 1)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                if (q == -1 && p != -1 && flag && p + 1 < n)
                {
                    q = n;
                    flag = false;
                }
                if (q <= n && p >= 0 && p + 1 < n)
                {
                    double[,] H1 = QRstepFrancis(Submatrix(H, p, q, p, q));
                    for (int i = p; i <= q; i++)
                        for (int j = p; j <= q; j++)
                        {
                            H[i, j] = H1[i - p, j - p];
                        }
                }
            }
            return H;
        }
        
        double[,] IRAM(double [,] A, double[,] q, int k, double tol, int iimax)// Алгоритм Арнольди с неявным перезапуском
        {
            int n = A.GetLength(0) - 1;
            double[,]  q1 = Multiply(q,1/AbsVector(q));
            int j = k;
            int m = k+j;
            double[,] Hk = ArnoldiFactorization(A,q1,k);
            double[,] Qk = ArnoldiFactorization(A,q1,k);
            for(int ii = 0;ii < iimax;ii++)
            {
                double[,] Qm = Submatrix(ArnoldiFactorization(A,Qk,Hk,k,m),0,n,0,n);
                double[,] Hm = Submatrix(ArnoldiFactorization(A,Qk,Hk,k,m),0,n,n+1,2*n+1);
                Complex[,] Shifts = Eigenvals(QRalgorithm(Hm,tol));

            }
            return Hk;
        }
        double[,] Householder(double[,] A)// Разложение Хаусхолдера
        {
            int n = A.GetLength(0) - 1;
            for (int k = 0; k <= n - 2; k++)
            {
                double[,] u = House(Submatrix(A, k + 1, n, k, k));
                double[,] A1 = rowhouse(Submatrix(A, k + 1, n, k, n), u);
                for (int i = k + 1; i <= n; i++)
                    for (int j = k; j <= n; j++)
                        A[i, j] = A1[i - k - 1, j - k];
                double[,] A2 = colhouse(Submatrix(A, 0, n, k + 1, n), u);
                for (int i = 0; i <= n; i++)
                    for (int j = k + 1; j <= n; j++)
                        A[i, j] = A2[i, j - k - 1];
            }
            return A;
        }
        Complex [,] Eigenvals(double[,] H)// Функция, возвращающая собственные числа из матрицы после применения QR-алгоритма
        {
            int n = H.GetLength(0) - 1;
            Complex[,] Shifts = new Complex[n+1, 1];
            int i = 0;
            int j = 0;
            while(i < n)
            {
                if(H[i+1,i]==0)
                {
                    Shifts[j, 0] = H[i, i];
                    j++;
                }
                else
                {
                    double[,] A = Submatrix(H,i,i+1,i,i+1);
                    double a = 1;
                    double b = A[0, 0] + A[1, 1];
                    double c = A[0, 0] * A[1, 1] - A[1, 0] * A[0, 1];
                    double D = Math.Pow(b, 2.0) - 4.0 * a * c;
                    Complex x1 = ((-b) + Complex.Pow(new Complex(D, 0), 0.5)) / (2.0 * a);
                    Complex x2 = ((-b) - Complex.Pow(new Complex(D, 0), 0.5)) / (2.0 * a);
                    Shifts[j, 0] = x1;
                    Shifts[j + 1, 0] = x2;
                    i++;
                    j += 2;
                }
                i++;
            }
            if (i == n)
                Shifts[j, 0] = H[i, i];
            return Shifts;
        }
        double[,] ArnoldiFactorization(double [,] A, double [,] q, int m)// Разложение Арнольди с нуля
        {
            int n = A.GetLength(0)-1;
            double[,] H = Plus(A,Multiply(A,-1.0));
            double[,] Q = new double[n,n];
            double[,] Abs = Multiply(q,1/AbsVector(q));
            for (int i = 0; i <= n;i++)
            {
                Q[i, 0] = Abs[i, 0];
            }
            for (int k = 0; k < m;k++ )
            {
                double[,] Q1 = Multiply(A, Submatrix(Q, 0, n, k, k));
                for(int i = 0;i <= n;i++)
                {
                    Q[i, k] = Q1[i, 0];
                }
                for(int j = 0;j <=k; j++)
                {
                    H[j,k] = Multiply(Transpose(Submatrix(Q, 0, n, j, j)), Submatrix(Q, k + 1, k + 1, 0, n))[0,0];

                    double[,] Q2 = Plus(Submatrix(Q, 0, n, k + 1, k + 1), Multiply(Multiply(Submatrix(Q, 0, n, j, j), H[j, k]), -1.0));
                    for(int i = 0;i < n;i++)
                    {
                        Q[i, j] = Q2[i, 0];
                    }
                }
                for (int j = 0; j <= k; j++)
                {
                    double a = Multiply(Transpose(Submatrix(Q, 0, n, j, j)), Submatrix(Q, k + 1, k + 1, 0, n))[0, 0];

                    double[,] Q2 = Plus(Submatrix(Q, 0, n, k + 1, k + 1), Multiply(Multiply(Submatrix(Q, 0, n, j, j), a), -1.0));
                    for (int i = 0; i < n; i++)
                    {
                        Q[i, j] = Q2[i, 0];
                    }
                    H[j, k] += a;
                }
                H[k + 1, k] = AbsVector(Submatrix(Q, 0, n, k + 1, k + 1));
                Q1 = Multiply(Submatrix(Q, 0, n, k + 1, k + 1), 1 / H[k + 1, k]);
                for (int i = 0; i < n; i++)
                {
                    Q[i, k+1] = Q1[i, 0];
                }
            }
                double[,] QH = new double[n,2*n];
                for(int i = 0;i < n;i++)
                {
                    for(int j = 0;j < 2*n;j++)
                    {
                        if(j<n)
                            QH[i,j] = H[i,j];
                        else
                            QH[i,j] = Q[i,j-n];
                    }
                }
            return QH;
        }
        double[,] ArnoldiFactorization(double [,] A, double [,] Qk, double [,] Hk, int k0, int m)// Разложение Арнольди с шага k0 по m
        {
            int n = A.GetLength(0) - 1;
            double[,] H = Plus(A, Multiply(A, -1.0));
            double[,] Q = new double[n, n];
            for (int i = 0; i < n;i++ )
            {
                for(int j = 0;j < k0;j++)
                {
                    Q[i, j] = Qk[i, j];
                    H[i, j] = Hk[i, j];
                }
            }
            for (int k = 0; k < m; k++)
            {
                double[,] Q1 = Multiply(A, Submatrix(Q, 0, n, k, k));
                for (int i = 0; i <= n; i++)
                {
                    Q[i, k] = Q1[i, 0];
                }
                for (int j = 0; j <= k; j++)
                {
                    H[j, k] = Multiply(Transpose(Submatrix(Q, 0, n, j, j)), Submatrix(Q, k + 1, k + 1, 0, n))[0, 0];

                    double[,] Q2 = Plus(Submatrix(Q, 0, n, k + 1, k + 1), Multiply(Multiply(Submatrix(Q, 0, n, j, j), H[j, k]), -1.0));
                    for (int i = 0; i < n; i++)
                    {
                        Q[i, j] = Q2[i, 0];
                    }
                }
                for (int j = 0; j <= k; j++)
                {
                    double a = Multiply(Transpose(Submatrix(Q, 0, n, j, j)), Submatrix(Q, k + 1, k + 1, 0, n))[0, 0];

                    double[,] Q2 = Plus(Submatrix(Q, 0, n, k + 1, k + 1), Multiply(Multiply(Submatrix(Q, 0, n, j, j), a), -1.0));
                    for (int i = 0; i < n; i++)
                    {
                        Q[i, j] = Q2[i, 0];
                    }
                    H[j, k] += a;
                }
                H[k + 1, k] = AbsVector(Submatrix(Q, 0, n, k + 1, k + 1));
                Q1 = Multiply(Submatrix(Q, 0, n, k + 1, k + 1), 1 / H[k + 1, k]);
                for (int i = 0; i < n; i++)
                {
                    Q[i, k + 1] = Q1[i, 0];
                }
            }
            double[,] QH = new double[n,2*n];
                for(int i = 0;i < n;i++)
                {
                    for(int j = 0;j < 2*n;j++)
                    {
                        if(j<n)
                            QH[i,j] = H[i,j];
                        else
                            QH[i,j] = Q[i,j-n];
                    }
                }
                return QH;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
    class AmountException : Exception { }
}
