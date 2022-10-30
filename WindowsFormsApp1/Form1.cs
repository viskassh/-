using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;
using Brushes = System.Windows.Media.Brushes;
using LiveCharts.Defaults;
using LiveCharts.Wpf.Charts.Base;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        double A1, A2, A3;
        double v1, v2, v3;
        double f1, f2, f3;
        double f_d;
        double a;
        double j;
        int N;
        double Es = 0;

        public double[,] val;
        public double[,] A_points;
        public double[,] val_after;
        public Form1()
        {
            InitializeComponent();
        }

        private void CartesianChart1OnDataClick(object sender, ChartPoint chartPoint)
        {
            MessageBox.Show("(" + chartPoint.X + ";" + chartPoint.Y + ")");
        }

        public double[,] SumSin()
        {
            if (for_A1.Text != "" || for_A2.Text != "" || for_A3.Text != "")
            {
                A1 = Convert.ToDouble(for_A1.Text);
                A2 = Convert.ToDouble(for_A2.Text);
                A3 = Convert.ToDouble(for_A3.Text);
            }
            else { MessageBox.Show("Вы не ввели параметр A", "Внимание!"); }
            if (for_v1.Text != "" || for_v2.Text != "" || for_v3.Text != "")
            {
                v1 = Convert.ToDouble(for_v1.Text);
                v2 = Convert.ToDouble(for_v2.Text);
                v3 = Convert.ToDouble(for_v3.Text);
            }
            else { MessageBox.Show("Вы не ввели параметр f", "Внимание!"); }
            if (for_f1.Text != "" || for_f2.Text != "" || for_f3.Text != "")
            {
                f1 = Convert.ToDouble(for_f1.Text);
                f2 = Convert.ToDouble(for_f2.Text);
                f3 = Convert.ToDouble(for_f3.Text);
            }
            else { MessageBox.Show("Вы не ввели параметр fi", "Внимание!"); }
            if (for_f_d.Text != "" || for_N.Text != "")
            {
                f_d = Convert.ToDouble(for_f_d.Text);
                N = Convert.ToInt32(for_N.Text);
                a = Convert.ToDouble(for_a.Text);
                j = Convert.ToDouble(for_J.Text);
            }
            else { MessageBox.Show("Вы не ввели параметр N, a или f_d", "Внимание!"); }

            //генерация шума
            double[] rand_noise = new double[N];
            Random rnd = new Random();
            for (int i = 0; i < N; i++)//генерируем случайный шум
            {
                double summN = 0;
                for (int k = 0; k < 12; k++)
                {
                    summN += (rnd.NextDouble()-0.5)*2;
                }
                rand_noise[i] = summN;
            }

            //создание сигнала
            double[] sign = new double[N];
            double En = 0;
            for (int i = 0; i < N; i++)
            {
                sign[i] = (float)((A1 * Math.Sin(2 * Math.PI * f1 * (float)i / f_d + v1) + A2 * Math.Sin(2 * Math.PI * f2 * (float)i / f_d + v2) + A3 * Math.Sin(2 * Math.PI * f3 * (float)i / f_d + v3)));
                Es += sign[i] * sign[i];
                En += rand_noise[i] * rand_noise[i];
            }

            //вычисляем b
            double b = Math.Sqrt(a * Es / En);

            val = new double[2, N]; //Массив значений 
            float dt = (float)(1 / f_d);
            for (int i = 0; i < N; i++)
            {
                val[1, i] = (float)i * dt;
            }
            for (int j = 0; j < N; j++)
            {
                val[0, j] = ((float)sign[j] + (float)b * (float)rand_noise[j]);
            }

            //val = new double[2, N];//Массив значений 
            //float dt = (float)(1 / f_d);

            //for (int i = 0; i < N; i++)
            //{
            //    val[1, i] = (float)i * dt;
            //}
            //for (int j = 0; j < N; j++)
            //{
            //    val[0, j] = (A1 * Math.Sin(2 * Math.PI * f1 * (float)j / f_d + v1) + A2 * Math.Sin(2 * Math.PI * f2 * (float)j / f_d + v2) + A3 * Math.Sin(2 * Math.PI * f3 * (float)j / f_d + v3));

            //}
            return val;
        }

        public List<double[,]> BDVPF()
        {
            cmplx[] arr = new cmplx[N];
            for (int i = 0; i < N; i++)
            {
                    arr[i] = new cmplx(val[0, i], 0);
            }
            cmplx.fourea(N, ref arr, -1);

            double Esum = 0; //полная энергия зашумленного сигнала
            for (int i = 0; i < N; i++)
            {
                Esum += (arr[i].re * arr[i].re + arr[i].im * arr[i].im) * (arr[i].re * arr[i].re + arr[i].im * arr[i].im); //определяем как квадрат амплитуды
            }
            double Es = j * Esum; //предполагаемя энергия очищенного сигнала
            double E_srav = 0; //переменная для сравнения
            int k = 0;

            //очищаем спектр
            do
            {
                E_srav += (arr[k].re * arr[k].re + arr[k].im * arr[k].im) * (arr[k].re * arr[k].re + arr[k].im * arr[k].im); //считываем энергию с левого
                E_srav += (arr[N - 1 - k].re * arr[N - 1 - k].re + arr[N - 1 - k].im * arr[N - 1 - k].im) * (arr[N - 1 - k].re * arr[N - 1 - k].re + arr[N - 1 - k].im * arr[N - 1 - k].im); //и правого концов частотного графика параллельно
                k++;
            } while (E_srav <= Es);

            //зануляем середину спектра
            double[,] A_points = new double [2,N];
            float df = (float)f_d / (N - 1);
            for (int i = 0; i <= k; i++)
            {
                A_points[1, i] = (float)(i * df);
            }
            for (int j = 0; j <= k; j++)
            {
                A_points[0,j] = (float)Math.Sqrt(arr[j].re * arr[j].re + arr[j].im * arr[j].im);
            }

            for (int i = k + 1; i < N - 1 - k; i++)
            {
                A_points[1, i] = (float)(i * df);

            }
            for (int j = k + 1; j < N - 1 - k; j++)
            {
                arr[j].re = 0;
                arr[j].im = 0;
                A_points[0, j] = (float)Math.Sqrt(arr[j].re * arr[j].re + arr[j].im * arr[j].im);

            }
            for (int i = N - 1 - k; i < N; i++)
            {
                A_points[1, i] = (float)(i * df);
            }
            for (int j = N - 1 - k; j < N; j++)
            {
                A_points[0, j] = (float)Math.Sqrt(arr[j].re * arr[j].re + arr[j].im * arr[j].im);
            }
            

            //рисуем очищенный сигнал по ощиченному спектру
            cmplx.fourea(N, ref arr, 1);
            double[,] val_after = new double[2, N];
            for (int i = 0; i < N; i++)
            {
                val_after[1, i] = (float)(i / f_d);
            }
            for (int j = 0; j < N; j++)
            {
                val_after[0, j] = (float)arr[j].re;
            }
            List<double[,]> doubles=new List<double[,]> ();
            doubles.Add(A_points);
            doubles.Add(val_after);
          
            return doubles;
        }


                private void button1_Click(object sender, EventArgs e)
        {
            build_Graph(SumSin());
            build_Graph1(BDVPF() [0]);
            build_Graph2(SumSin(), BDVPF() [1]);
        }

        private void build_Graph(double[,] val)
        {
            //Очистка предыдущих коллекций
            cartesianChart1.Series.Clear();
            cartesianChart1.AxisX.Clear();
            cartesianChart1.AxisY.Clear();

            SeriesCollection series = new SeriesCollection();//Коллекция линий
            LineSeries ln = new LineSeries();//Линия

            ChartValues <ObservablePoint> Values = new ChartValues<ObservablePoint>();//Коллекция значений по Oy
            for (int j = 0; j < val.GetLength(1); j++)
            {
                Values.Add(new ObservablePoint(val[1, j], val[0, j]));
            }
            ln.Values = Values;//Добавление значений на линию
            ln.PointGeometrySize = 1;
            series.Add(ln);//Добавление линии в коллекцию линий
            cartesianChart1.Series = series;//Добавление коллекции на график


            //Определение максимума и минимума по Ox
            double min = val[1, 0];
            double max = val[1, 0];
            for (int j = 0; j < val.GetLength(1); j++)
            {
                if (min > val[1, j])
                {
                    min = val[1, j];
                }
                if (max < val[1, j])
                {
                    max = val[1, j];
                }
            }
            //Ось Ox
            cartesianChart1.AxisX.Add(new Axis
            {
                Title = "t, с",//подпись
                LabelFormatter = value => value.ToString(""),
                MinValue = min,
                MaxValue = max,
            Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
            min = val[0, 0];
            max = val[0, 0];
            for (int j = 0; j < val.GetLength(1); j++)
            {
                if (min > val[0, j])
                {
                    min = val[0, j];
                }
                if (max < val[0, j])
                {
                    max = val[0, j];
                }
            }
            //Ось Oy
            cartesianChart1.AxisY.Add(new Axis
            {
                Title = "F(t)",
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
        }

        private void build_Graph1(double[,] A_points)
        {
            //Очистка предыдущих коллекций
            cartesianChart2.Series.Clear();
            cartesianChart2.AxisX.Clear();
            cartesianChart2.AxisY.Clear();

            SeriesCollection series = new SeriesCollection();//Коллекция линий
            LineSeries ln = new LineSeries();//Линия

            ChartValues<ObservablePoint> Values = new ChartValues<ObservablePoint>();//Коллекция значений по Oy
            for (int j = 0; j < A_points.GetLength(1); j++)
            {
                Values.Add(new ObservablePoint(A_points[1, j], A_points[0, j]));
            }
            ln.Values = Values;//Добавление значений на линию
            ln.PointGeometrySize = 1;
            series.Add(ln);//Добавление линии в коллекцию линий
            cartesianChart2.Series = series;//Добавление коллекции на график
            //Определение максимума и минимума по Ox
            double min = A_points[1, 0];
            double max = A_points[1, 0];
            for (int j = 0; j < A_points.GetLength(1); j++)
            {
                if (min > A_points[1, j])
                {
                    min = A_points[1, j];
                }
                if (max < A_points[1, j])
                {
                    max = A_points[1, j];
                }
            }
            //Ось Ox
            cartesianChart2.AxisX.Add(new Axis
            {
                Title = "f, Гц",//подпись
                LabelFormatter = value => value.ToString(""),
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
            min = A_points[0, 0];
            max = A_points[0, 0];
            for (int j = 0; j < A_points.GetLength(1); j++)
            {
                if (min > A_points[0, j])
                {
                    min = A_points[0, j];
                }
                if (max < A_points[0, j])
                {
                    max = A_points[0, j];
                }
            }
            //Ось Oy
            cartesianChart2.AxisY.Add(new Axis
            {
                Title = "A(f)",
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
            //cartesianChart2.LegendLocation = LegendLocation.Bottom;//Где расположить легенду график
            //cartesianChart2.DataClick += CartesianChart1OnDataClick;
        }

        private void build_Graph2( double[,] val, double[,] val_after)
        {
            //Очистка предыдущих коллекций
            cartesianChart3.Series.Clear();
            cartesianChart3.AxisX.Clear();
            cartesianChart3.AxisY.Clear();

            SeriesCollection series = new SeriesCollection();//Коллекция линий
            LineSeries ln = new LineSeries();//Линия

            ChartValues<ObservablePoint> Values = new ChartValues<ObservablePoint>();//Коллекция значений по Oy
            for (int j = 0; j < val_after.GetLength(1); j++)
            {
                Values.Add(new ObservablePoint(val_after[1, j], val_after[0, j]));
            }

            ln.Values = Values;//Добавление значений на линию
           
            ln.PointGeometrySize = 1;
            series.Add(ln);//Добавление линии в коллекцию линий
            cartesianChart3.Series = series;//Добавление коллекции на график
            //Определение максимума и минимума по Ox
            double min = val_after[1, 0];
            double max = val_after[1, 0];
            for (int j = 0; j < val_after.GetLength(1); j++)
            {
                if (min > val_after[1, j])
                {
                    min = val_after[1, j];
                }
                if (max < val_after[1, j])
                {
                    max = val_after[1, j];
                }
            }
            //Ось Ox
            cartesianChart3.AxisX.Add(new Axis
            {
                Title = "t, c",//подпись
                LabelFormatter = value => value.ToString(""),
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
            min = val_after[0, 0];
            max = val_after[0, 0];
            for (int j = 0; j < val_after.GetLength(1); j++)
            {
                if (min > val_after[0, j])
                {
                    min = val_after[0, j];
                }
                if (max < val_after[0, j])
                {
                    max = val_after[0, j];
                }
            }
            //Ось Oy
            cartesianChart3.AxisY.Add(new Axis
            {
                Title = "F(t)",
                MinValue = min,
                MaxValue = max,
                Separator = new Separator
                {
                    StrokeThickness = 1,
                    Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 79, 86))
                }
            });
        }

    }
}
