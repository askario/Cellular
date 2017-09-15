using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MM4
{
    public partial class Form3 : Form
    {
        //нейросеть
        const int width = 256;
        const int heigth = 256;
        const int s = 2;
        const int max_generation = 3000;
        public float[,] field = new float[width + 2, heigth + 2];
        public float[,] next_field = new float[width + 2, heigth + 2];
        public int[,] st_vozb = new int[width + 2, heigth + 2];
        public int[,] st_vost = new int[width + 2, heigth + 2];
        int game_used;
        int generation; // поколение
        int friends; //соседи клетки

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
          
        }

        private void Form3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Pen black = new Pen(Color.Black, 1);
            SolidBrush white = new SolidBrush(Color.White);
        }

        void Start()//зануляю поля
        {
            generation = 0;
            friends = 0;
            for (int i = 0; i < heigth + 2; i++)
            {
                for (int j = 0; j < width + 2; j++)
                {
                    field[i, j] = next_field[i, j] = 0;
                    st_vost[i, j] = st_vozb[i, j] = 0;
                }
            }
        }

        void Iteration()
        {
            for (int i = 1; i <= heigth; i++)
            {
                for (int j = 1; j <= width; j++)
                {
                    if (field[i, j] == 0)
                    {
                        float counter = 0;
                        counter = field[i, j] + field[i, j - 1] + field[i, j + 1] +
                            field[i - 1, j - 1] + field[i - 1, j] + field[i - 1, j + 1] +
                            field[i + 1, j - 1] + field[i + 1, j] + field[i + 1, j + 1];
                        if (counter >= 3) next_field[i, j] = 1;
                        else next_field[i, j] = field[i, j];
                    }
                    else if (st_vozb[i, j] < 5)
                    {
                        next_field[i, j] = 1;
                        st_vozb[i, j]++;
                    }
                    else if (st_vost[i, j] < 8)
                    {
                        next_field[i, j] = field[i, j] * 0.7f;
                        st_vost[i, j]++;
                    }
                    else
                    {
                        st_vost[i, j] = 0;
                        st_vozb[i, j] = 0;
                        float counter = 0;
                        counter = field[i, j] + field[i, j - 1] + field[i, j + 1] +
                            field[i - 1, j - 1] + field[i - 1, j] + field[i - 1, j + 1] +
                            field[i + 1, j - 1] + field[i + 1, j] + field[i + 1, j + 1];
                        if (counter >= 3) next_field[i, j] = 1;
                        else next_field[i, j] = field[i, j] * 0.7f;
                    }
                }
            }
            for (int i = 1; i <= heigth; i++)
            {
                for (int j = 1; j <= width; j++)
                {
                    field[i, j] = next_field[i, j];
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //generation = 0;
            Front();
            if ((generation % 15) == 0)
                Round();
            Draw();
            timer1.Start();
        }

        void Draw()
        {
            SolidBrush red = new SolidBrush(Color.Red);
            Pen black = new Pen(Color.Black, 1);
            SolidBrush white = new SolidBrush(Color.White);

            BufferedGraphicsContext currentContext;
            BufferedGraphics myBuffer;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(this.CreateGraphics(),
               this.DisplayRectangle);
            myBuffer.Graphics.Clear(Color.White);
       
            for (int i = 1; i <= heigth; i++)
            {
                for (int j = 1; j <= width; j++)
                {
                    SolidBrush green = new SolidBrush(Color.FromArgb((int)((float)field[i, j] / 1.0 * 255), 0, 255, 0));
                    myBuffer.Graphics.FillRectangle(green, (i - 1) * s + 50, (j - 1) * s + 50, s, s);
                }
            }
           

            myBuffer.Render();
            label1.Text = generation.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (generation < max_generation)
            {
                Draw();
                if (game_used == 0)
                    Iteration();
                generation++;
                timer1.Start();
            }
        }

        void Front()
        {
            for (int j = 1; j <= width; j++)
            {
                field[1, j] = 1;
                field[heigth, j] = 1;
            }

        }
        
        void Round()
        {
            field[125, 126] = 1;
            field[125, 127] = 1;
            field[125, 128] = 1;
            field[126, 126] = 1;
            field[126, 127] = 1;
            field[126, 128] = 1;
            field[127, 126] = 1;
            field[127, 127] = 1;
            field[127, 128] = 1;
        }
    }
}
