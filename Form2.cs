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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        //Жизнь 
        const int L = 256;
        public int[,] avtomat = new int[L + 2, L + 2];
        public int[,] new_avtomat = new int[L + 2, L + 2];
        int total = 0;
        int genaration = 0;
        const int max_generation = 1000;
        const int s = 2;

        void ravomernoe()
        {
            Random r = new Random();

            for (int i = 1; i < L + 1; i++)
                for (int j = 1; j < L + 1; j++)
                    if(r.NextDouble() >=0.5)
                        avtomat[i, j] = 1;
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            ravomernoe();
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Pen black = new Pen(Color.Black, 1);
            SolidBrush white = new SolidBrush(Color.White);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        void iteration()
        {
            for (int i = 0; i < L + 2; i++)
                for (int j = 0; j < L + 2; j++)
                    new_avtomat[i, j] = avtomat[i, j];

                    for (int i = 1; i < L + 1; i++)
                    {
                        for (int j = 1; j < L + 1; j++)
                        {
                            total = avtomat[i - 1, j - 1] + avtomat[i - 1, j] + avtomat[i - 1, j + 1]
                                + avtomat[i, j - 1] + avtomat[i, j + 1] + avtomat[i + 1, j - 1]
                                + avtomat[i + 1, j] + avtomat[i + 1, j + 1];

                            if (avtomat[i, j] == 1)
                            {
                                if (total >= 3)
                                    new_avtomat[i, j] = 0;
                            }
                            else
                            {
                                if (total ==2)
                                    new_avtomat[i, j] = 1;
                            }
                        }
                    }

                    for (int i = 0; i < L + 2; i++)
                        for (int j = 0; j < L + 2; j++)
                            avtomat[i, j] = new_avtomat[i, j];

        }


        void Draw()
        {
            SolidBrush Black = new SolidBrush(Color.Black);
            SolidBrush White = new SolidBrush(Color.White);

            BufferedGraphicsContext currentContext;
            BufferedGraphics myBuffer;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(this.CreateGraphics(),this.DisplayRectangle);
            myBuffer.Graphics.Clear(Color.White);

            iteration();

            for (int i = 1; i < L + 1; i++)
            {
                for (int j = 1; j < L + 1; j++)
                {
                    if (avtomat[i, j] == 1)
                        myBuffer.Graphics.FillRectangle(Black, (i - 1) * s + 50, (j - 1) * s + 50, s, s);

                }
            }

            myBuffer.Render();
            label1.Text = genaration.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
      
            if (max_generation > genaration)
            {
                Draw();

                genaration++;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }
    }
}
