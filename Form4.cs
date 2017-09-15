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
    public partial class Form4 : Form
    {
        const int width = 256;
        const int heigth = 256;
        const int s = 2;
        const int max_generation = 3000;

        public int[,] state_energy = new int[width + 2, heigth + 2];
        public int[,] state_timeLife = new int[width + 2, heigth + 2];
        public float[,] state_moveBusy = new float[width + 2, heigth + 2];

        //public float[,] Point_x = new float[width + 2, heigth + 2];
        //public float[,] Point_y = new float[width + 2, heigth + 2];

        int numCells;      // количество клеток
        public int[,] orgGrid = new int[width + 2, heigth + 2];// сетка нового поколения
        public int[,] foodGrid = new int[width + 2, heigth + 2];    // сетка питательности клеток

        int maxFoodValue;  // максимальная питательность клетки
        int deltaFood;     // прирост питательности в клетке
        int deltaEnergy;   // прирост запаса энергии организма
        int maxEnergy;     // максимальный запас энергии организма
        int deltaExpens;   // затраты энергии организма за такт
        int deltaDivision; // затраты энергии на деление организма

        int timeLife;       // время жизни организма (в тактах)
        int timeAdult;      // время, с которого начинается деление организма (в тактах)
        float startOrgCoef; // коэффициент, определяющий начальное количество организмов
        bool modification;  // Учёт модификации правил перехода в другую клетку


        //public bool[,] field = new bool[width+2,heigth+2];
        //public bool[,] next_field = new bool[width+2, heigth+2];
        int game_used;
        int generation; // поколение
        //int friends; //соседи клетки
        public Form4()
        {
            InitializeComponent();
            game_used = 0;
        }

        private void Form4_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Pen black = new Pen(Color.Black, 1);
            SolidBrush white = new SolidBrush(Color.White);
        }
        public struct Point
        {
            public int x;
            public int y;
        }
        private void clearOrg(int x, int y) // Очищаем клетку от организма
        {
            orgGrid[x, y] = 0;
            state_energy[x, y] = 0;
            state_timeLife[x, y] = 0;
            state_moveBusy[x, y] = 0;
        }
        bool deathOrg(int x, int y) // Проверяем, не умрёт ли организм
        {
            // Проверяем, не слишком ли стар организм
            if (state_timeLife[x, y] == timeLife)
            {
                clearOrg(x, y);
                return true;
            };

            // Тратим энергию организма
            if (state_energy[x, y] > deltaExpens)
                state_energy[x, y] -= deltaExpens;
            else // Иначе организм погибает
            {
                clearOrg(x, y);
                return true;
            };

            return false;
        }
        void eatOrg(int x, int y) // Организм "питается"
        {
            // Если питательность больше той, которая требуется организму
            if (foodGrid[x, y] >= deltaEnergy)
            {
                // Если клетка доходит до предела запасаемой энергии
                bool fatOrg = ((state_energy[x, y] + deltaEnergy) > maxEnergy) ? true : false;
                int food = (fatOrg) ? (maxEnergy - state_energy[x, y]) : deltaEnergy;

                state_energy[x, y] += food;
                foodGrid[x, y] = foodGrid[x, y] - food;
            }
            else // Иначе поедаем то, что осталось
            {
                // Если клетка доходит до предела запасаемой энергии
                bool fatOrg = ((state_energy[x, y] + foodGrid[x, y]) > maxEnergy) ? true : false;
                int food = (fatOrg) ? (maxEnergy - state_energy[x, y]) : foodGrid[x, y];

                state_energy[x, y] += food;
                foodGrid[x, y] = 0;
            };
        }
        void divMoveOrg(int x, int y) // Организм размножается или передвигается
        {
            // Проверяем наличие свободных поблизости клеток
            List<Point> points = new List<Point>();
            for (int xRel = -1; xRel <= 1; xRel++)
                for (int yRel = -1; yRel <= 1; yRel++)
                    if (orgGrid[x + xRel, y + yRel] == 0)
                        points.Add(new Point { x = x + xRel, y = y + yRel });

            // Если свободные клетки имеются, передвигаем или размножаем организм
            if (points.Count() != 0)
            {
                // Если клетка уже взрослая и у неё хватает энергии для размножения
                if ((state_timeLife[x, y] >= timeAdult) && (state_energy[x, y] > deltaDivision))
                {
                    // Уменьшаем энергию, чтобы организм "разделился"
                    state_energy[x, y] -= deltaDivision;

                    // Передвигаем организм на другую клетку случайным образом
                    Random rand = new Random();
                    Point move = points[rand.Next() % (points.Count())];
                    moveFromTo(new Point { x = x, y = y }, move);

                    // Создаём организм в клетке, в которой находились
                    orgGrid[x, y] = 1;
                    state_energy[x, y] = deltaDivision;
                    state_timeLife[x, y] = 0;
                }
                else // Перемещаем организм в зависимости от заданного правила
                    moveOrg(points, x, y);
            };
        }
        void moveOrg(List<Point> points, int x, int y)
        {
            if (!modification) // Перемещаем организм в зависимости от заданного правила
            {
                // Передвигаем организм на другую клетку случайным образом
                Random rand = new Random();
                Point move = points[rand.Next() % (points.Count())];
                moveFromTo(new Point { x = x, y = y }, move);
            }
            else
            {
                // Находим ближайшую клетку с максимальной питательностью
                Random rand = new Random();
                Point move = new Point { x = x, y = y };
                for (int index = 0; index < points.Count(); index++)
                    if (foodGrid[move.x, move.y] < foodGrid[points[index].x, points[index].y])
                        move = points[index];

                // Если такая клетка нашлась
                if (!((move.x == x) && (move.y == y)))
                    moveFromTo(new Point { x = x, y = y }, move);
            };
        }
        void moveFromTo(Point from, Point to) // Передвигаем организм на другую клетку
        {
            // Обновляем новую клетку, в которую переместились
            orgGrid[to.x, to.y] = 1;
            state_energy[to.x, to.y] = state_energy[from.x, from.y];
            state_timeLife[to.x, to.y] = state_timeLife[from.x, from.y];
            state_moveBusy[to.x, to.y] = 1;

            // Очищаем старую клетку, в которой находились
            clearOrg(from.x, from.y);
        }
        void updateGrids() // Обновляем данные по сеткам после "прохода" алгоритма
        {
            for (int x = 1; x <= numCells; x++)
                for (int y = 1; y <= numCells; y++)
                {
                    // Обновляем данные по сетке питательности
                    if ((foodGrid[x, y] + deltaFood) <= maxFoodValue)
                        foodGrid[x, y] += deltaFood;

                    // Cбрасываем флаги передвижения организмов
                    state_moveBusy[x, y] = 0;
                }
        }

        int calcEngine() // Алгоритм клеточного автомата "Организмы - питательная среда"
        {
            // Обход по всем клеткам с организмами
            for (int x = 1; x <= numCells; x++)
                for (int y = 1; y <= numCells; y++)
                    if (orgGrid[x, y] == 1)
                    {
                        // Проверяем, не передвигался ли организм ранее
                        if (state_moveBusy[x, y] == 1)
                            continue;

                        // Увеличиваем возраст организма
                        if (state_timeLife[x, y] < timeLife)
                            state_timeLife[x, y]++;

                        // Проверяем, не умрёт ли организм
                        if (deathOrg(x, y))
                            continue;

                        // Организм питается
                        eatOrg(x, y);

                        // Организм размножается или передвигается
                        divMoveOrg(x, y);
                    };

            // Обновляем данные по сеткам после "прохода" алгоритма
            updateGrids();

            return 0;
        }

        int getNumCells() // Получить размер сетки
        {
            return numCells;
        }

        void randomInit() // Заполнить сетку случайным образом
        {
            for (int coun = 0; coun < (int)(startOrgCoef * numCells * numCells); coun++)
            {
                Random rand = new Random();

                int x = rand.Next() % (numCells - 1) + 1;
                int y = rand.Next() % (numCells - 1) + 1;
                while (orgGrid[x, y] == 1)
                {
                    x = rand.Next() % (numCells - 1) + 1;
                    y = rand.Next() % (numCells - 1) + 1;
                };

                orgGrid[x, y] = 1;
                state_energy[x, y] = 10;
                state_timeLife[x, y] = -1;
            };
        }
        void userInit_1() // Задать пользовательскую сетку
        {
            for (int count = 0; count < (int)(startOrgCoef * 0.5 * numCells * numCells); count++)
            {
                Random rand = new Random();
                int x = rand.Next() % (numCells / 4 - 1) + 1;
                int y = rand.Next() % (numCells - 1) + 1;
                while (orgGrid[x, y] == 1)
                {
                    x = rand.Next() % (numCells / 4 - 1) + 1;
                    y = rand.Next() % (numCells - 1) + 1;
                };

                orgGrid[x, y] = 1;
                state_energy[x, y] = 10;
                state_timeLife[x, y] = -1;
            };


            for (int count = 0; count < (int)(startOrgCoef * 0.5 * numCells * numCells); count++)
            {
                Random rand = new Random();
                int x = rand.Next() % (numCells / 2 - 1) + numCells / 2;
                int y = rand.Next() % (numCells - 1) + 1;
                while (orgGrid[x, y] == 1)
                {
                    x = rand.Next() % (numCells / 2 - 1) + numCells / 2;
                    y = rand.Next() % (numCells - 1) + 1;
                };

                orgGrid[x, y] = 1;
                state_energy[x, y] = 10;
                state_timeLife[x, y] = -1;
            };
        }
        void userInit_2() // Задать пользовательскую сетку
        {
            orgGrid[113, 124] = 1; state_energy[113, 124] = 10; state_timeLife[113, 124] = -1;
            orgGrid[114, 124] = 1; state_energy[114, 124] = 10; state_timeLife[114, 124] = -1;
            orgGrid[115, 124] = 1; state_energy[115, 124] = 10; state_timeLife[115, 124] = -1;
            orgGrid[116, 124] = 1; state_energy[116, 124] = 10; state_timeLife[116, 124] = -1;
            orgGrid[117, 124] = 1; state_energy[117, 124] = 10; state_timeLife[117, 124] = -1;
            orgGrid[118, 124] = 1; state_energy[118, 124] = 10; state_timeLife[118, 124] = -1;
            orgGrid[113, 125] = 1; state_energy[113, 125] = 10; state_timeLife[113, 125] = -1;
            orgGrid[113, 126] = 1; state_energy[113, 126] = 10; state_timeLife[113, 126] = -1;
            orgGrid[113, 127] = 1; state_energy[113, 127] = 10; state_timeLife[113, 127] = -1;
            orgGrid[113, 128] = 1; state_energy[113, 128] = 10; state_timeLife[113, 128] = -1;
            orgGrid[113, 129] = 1; state_energy[113, 129] = 10; state_timeLife[113, 129] = -1;
            orgGrid[113, 130] = 1; state_energy[113, 130] = 10; state_timeLife[113, 130] = -1;
            orgGrid[113, 131] = 1; state_energy[113, 131] = 10; state_timeLife[113, 131] = -1;
            orgGrid[113, 132] = 1; state_energy[113, 132] = 10; state_timeLife[113, 132] = -1;
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

        private void button1_Click(object sender, EventArgs e)
        {
            //generation = 0;
            Start();
            Draw();
            timer1.Start();
        }
        void Start()//зануляю поля
        {
            generation = 0;
            numCells = 256;
            //modification = false; // Переменная, отвечающая за передвижение организма
            modification = true;

            maxFoodValue = 10;
            deltaFood = 1;
            deltaEnergy = 5;
            maxEnergy = 35;
            deltaExpens = 2;
            deltaDivision = 3;

            timeLife = 15;
            timeAdult = 3;
            startOrgCoef = 0.3f;

            // Обнуляем массивы
            for (int x = 0; x < numCells + 2; x++)
                for (int y = 0; y < numCells + 2; y++)
                {
                    orgGrid[x, y] = 0;
                    foodGrid[x, y] = 0;

                    state_energy[x, y] = 0;
                    state_timeLife[x, y] = 0;
                    state_moveBusy[x, y] = 0;
                };

            // Создаём фиктивные организмы на границе
            for (int x = 0; x < numCells + 2; x++)
            {
                orgGrid[0, x] = 1;
                orgGrid[x, 0] = 1;
                orgGrid[numCells + 1, x] = 1;
                orgGrid[x, numCells + 1] = 1;
            };

            //randomInit();
            userInit_1();
        }
        void Iteration()
        {
            calcEngine();
        }
        void Draw()
        {

            SolidBrush red = new SolidBrush(Color.Black);
            Pen black = new Pen(Color.Black, 1);
            SolidBrush white = new SolidBrush(Color.White);

            BufferedGraphicsContext currentContext;
            BufferedGraphics myBuffer;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(this.CreateGraphics(),
               this.DisplayRectangle);
            myBuffer.Graphics.Clear(Color.White);
            //myBuffer.Graphics.FillRectangle(white, 50, 50, 300, 300);

            //for (int d = 0; d < 200; d++)
            //{
            for (int i = 1; i <= heigth; i++)
            {
                for (int j = 1; j <= width; j++)
                {
                    if (orgGrid[i, j] == 1)
                        myBuffer.Graphics.FillRectangle(red, (i - 1) * s + 50, (j - 1) * s + 50, s, s);

                    SolidBrush green = new SolidBrush(Color.FromArgb((int)((float)foodGrid[i, j] / 10.0 * 255), 0, 255, 0));
                    myBuffer.Graphics.FillRectangle(green, (i - 1) * s + 50, (j - 1) * s + 50, s, s);
                }
            }
            //}

            // for (int i = 0; i <= width * s; i += s)
            // {
            //     myBuffer.Graphics.DrawLine(black, 50 + i, 50, 50 + i, 50 + width * s);
            //      myBuffer.Graphics.DrawLine(black, 50, 50 + i, 50 + width * s, 50 + i);
            //}

            myBuffer.Render();
            label1.Text = generation.ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

    }
}
