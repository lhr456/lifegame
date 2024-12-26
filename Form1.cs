


using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        const int hi = 200;
        private bool[,] grid = new bool[hi, hi]; // ���������С�� 50x50

        // ���峣���ͱ���
        private const int CellSize = 8;  // ÿ����Ԫ��Ĵ�С���Ŵ�2��
        private const int GridWidth = hi;   // ����Ŀ�ȣ�50�У�
        private const int GridHeight = hi;  // ����ĸ߶ȣ�50�У�

        private Thread gameThread;  // ��Ϸ�߼��߳�
        private bool gameRunning = false;  // �����Ϸ�Ƿ�������
        private object lockObj = new object(); // ������
        private Bitmap offscreenBitmap; // �������Ƶ� Bitmap
        private Panel gamePanel;  // ��Ϸ���

        public Form1()
        {
            InitializeComponent();
            InitializeLayout();
            InitializeGrid();
            offscreenBitmap = new Bitmap(GridWidth * CellSize, GridHeight * CellSize); // ��ʼ������ͼƬ

            // ���ô����˫����
            this.DoubleBuffered = true;
           
        }

        // ��ʼ���������������ʼ״̬
        private void InitializeGrid()
        {
            Random rand = new Random();
            //for (int x = 0; x < GridWidth; x++)
            //{
            //    for (int y = 0; y < GridHeight; y++)
            //    {
            //        grid[x, y] = rand.Next(2) == 0; // �������Ϊ��ϸ������ϸ��
            //    }
            //}
            int centerX = grid.GetLength(0) / 2;  // ����grid�Ƕ�ά���飬GetLength(0)��ȡ��һά���ȣ�ͨ����x����
            int centerY = grid.GetLength(1) / 2;  // ��ȡ�ڶ�ά���ȣ�ͨ����y����

            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[centerX, y] = true;
            }
        }

        // ������Ϸ״̬
        private void UpdateGame()
        {
            bool[,] newGrid = new bool[GridWidth, GridHeight];

            // ʹ��Parallel.For���б���x���꣨���ѭ����
            Parallel.For(0, GridWidth, x =>
            {
                // ����ÿ��x���꣬˳�����y���꣨�ڲ�ѭ����
                for (int y = 0; y < GridHeight; y++)
                {
                    int neighbors = CountNeighbors(x, y);
                    if (grid[x, y] && (neighbors == 2 || neighbors == 3))
                    {
                        newGrid[x, y] = true;
                    }
                    else if (!grid[x, y] && neighbors == 3)
                    {
                        newGrid[x, y] = true;
                    }
                    else if (!grid[x, y] && neighbors ==1  || neighbors > 3)
                    {
                        newGrid[x, y] = false;
                    }
                }
            });

            // ���� grid ʱ���������Ⲣ����ͻ
            lock (lockObj)
            {
                grid = newGrid;
            }

            // ���»��ƽ���
            gamePanel.Invalidate();
        }
        int index;
        // ��Ϸ��ѭ��
        private void GameLoop()
        {
            var updateInterval = 1000; // ����Ƶ�ʣ�100ms����һ��
            while (gameRunning)
            {
                if (cont> index)
                {
                    lock (lockObj)
                    {
                        gameRunning = false;
                        cont = 0;
                       
                    }
                    
                }
              

                UpdateGame(); // ������Ϸ״̬
                Thread.Sleep(updateInterval); // ��ͣһ��ʱ�䣬�ٽ�����һ�θ���
                cont++;
            }
           
           
        }

        // ����ĳ��λ�õ�ϸ���ھ�����
        private int CountNeighbors(int x, int y)
        {
            int count = 0;

            // ���8���ھӵ�״̬
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // �ų�����

                    int nx = x + dx;
                    int ny = y + dy;

                    // ����ھ�����Ч��Χ�ڣ����ǻ�ϸ�������Ӽ���
                    if (nx >= 0 && nx < GridWidth && ny >= 0 && ny < GridHeight && grid[nx, ny])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // ��ȡָ��λ�õ�ϸ��״̬
        private bool GetLiveByPosition(int x, int y)
        {
            if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
            {
                return grid[x, y]; // ���ظ�λ�õ�ϸ��״̬
            }
            return false;  // ���λ�ò�����Ч��Χ�ڣ�������ϸ��
        }

        // ��ʼ���ؼ�����
        TextBox iterationsTextBox = new TextBox
        {
            Location = new Point(10, 140),
            Size = new Size(100, 30),
            Text = "10" // Ĭ��ֵΪ10
        };
        private void InitializeLayout()
        {
            this.Text = "������Ϸ";
            this.ClientSize = new Size(GridWidth * CellSize + 150, GridHeight * CellSize + 150); // ����������Ӧ�¿ؼ�

            // ���������ÿؼ�
            var startButton = new Button
            {
                Text = "��ʼ",
                Location = new Point(10, 10),
                Size = new Size(100, 40)
            };

            var stopButton = new Button
            {
                Text = "ֹͣ",
                Location = new Point(10, 60),
                Size = new Size(100, 40)
            };

            var iterationsLabel = new Label
            {
                Text = "��������:",
                Location = new Point(10, 110),
                Size = new Size(100, 30)
            };

           

          

            gamePanel = new Panel
            {
                Location = new Point(150, 10),
                Size = new Size(GridWidth * CellSize, GridHeight * CellSize),
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(startButton);
            this.Controls.Add(stopButton);
            this.Controls.Add(iterationsLabel);
            this.Controls.Add(iterationsTextBox);
          
            this.Controls.Add(gamePanel);

            // �󶨻���������¼�
            gamePanel.Paint += GamePanel_Paint;

            // �󶨰�ť����¼�
            startButton.Click += StartButton_Click;
            stopButton.Click += StopButton_Click;
            // �����е�����ť�ĵ���¼�
    

        }
        static int cont = 0;
        // ����ָ�������ĵ���
        
        // ��ʼ��ť����¼�
        private void StartButton_Click(object sender, EventArgs e)
        {
          
            // ��õ�������
            if (!gameRunning)
            {
                lock (lockObj)
                {
                    gameRunning = true;
                }
                string text = iterationsTextBox.Text.ToString();
                int result;
                if (int.TryParse(text, out result))
                {
                    index = result;
                }
                else
                {
                    // ת��ʧ�ܣ�˵��������ı�������������ʽҪ��ͬ�����Խ��д�����ʾ�ȴ���
                    MessageBox.Show("��������ݲ��ǺϷ������������������롣");
                }
                gameThread = new Thread(GameLoop); // �������߳�
              
                gameThread.Start();  // �����߳�
                (sender as Button).Text = "�ѿ�ʼ"; // ���°�ť�ı�
            }
        }

        // ֹͣ��ť����¼�
        private void StopButton_Click(object sender, EventArgs e)
        {
            if (gameRunning)
            {
                lock (lockObj)
                {
                    gameRunning = false;
                }
               
                gameThread.Join();  // �ȴ��߳̽���
                (sender as Button).Text = "��ֹͣ"; // ���°�ť�ı�
            }
        }

        // ��������
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            var g = Graphics.FromImage(offscreenBitmap);
            g.Clear(Color.White); // ��ջ���

            // ������������λ��
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    // ���ƻ�ϸ��
                    if (grid[x, y])
                    {
                        g.FillRectangle(Brushes.Black, x * CellSize, y * CellSize, CellSize, CellSize);
                    }
                    // ����ϸ���߿�
                    g.DrawRectangle(Pens.Gray, x * CellSize, y * CellSize, CellSize, CellSize);
                }
            }

            // �������ͼ����Ƶ������
            e.Graphics.DrawImage(offscreenBitmap, 0, 0);
        }
    }
}
