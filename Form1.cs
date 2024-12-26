


using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        const int hi = 200;
        private bool[,] grid = new bool[hi, hi]; // 假设网格大小是 50x50

        // 定义常量和变量
        private const int CellSize = 8;  // 每个单元格的大小，放大2倍
        private const int GridWidth = hi;   // 网格的宽度（50列）
        private const int GridHeight = hi;  // 网格的高度（50行）

        private Thread gameThread;  // 游戏逻辑线程
        private bool gameRunning = false;  // 标记游戏是否在运行
        private object lockObj = new object(); // 锁对象
        private Bitmap offscreenBitmap; // 离屏绘制的 Bitmap
        private Panel gamePanel;  // 游戏面板

        public Form1()
        {
            InitializeComponent();
            InitializeLayout();
            InitializeGrid();
            offscreenBitmap = new Bitmap(GridWidth * CellSize, GridHeight * CellSize); // 初始化离屏图片

            // 设置窗体的双缓冲
            this.DoubleBuffered = true;
           
        }

        // 初始化网格并设置随机初始状态
        private void InitializeGrid()
        {
            Random rand = new Random();
            //for (int x = 0; x < GridWidth; x++)
            //{
            //    for (int y = 0; y < GridHeight; y++)
            //    {
            //        grid[x, y] = rand.Next(2) == 0; // 随机设置为活细胞或死细胞
            //    }
            //}
            int centerX = grid.GetLength(0) / 2;  // 假设grid是二维数组，GetLength(0)获取第一维长度（通常是x方向）
            int centerY = grid.GetLength(1) / 2;  // 获取第二维长度（通常是y方向）

            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[centerX, y] = true;
            }
        }

        // 更新游戏状态
        private void UpdateGame()
        {
            bool[,] newGrid = new bool[GridWidth, GridHeight];

            // 使用Parallel.For并行遍历x坐标（外层循环）
            Parallel.For(0, GridWidth, x =>
            {
                // 对于每个x坐标，顺序遍历y坐标（内层循环）
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

            // 更新 grid 时加锁，避免并发冲突
            lock (lockObj)
            {
                grid = newGrid;
            }

            // 重新绘制界面
            gamePanel.Invalidate();
        }
        int index;
        // 游戏主循环
        private void GameLoop()
        {
            var updateInterval = 1000; // 更新频率：100ms更新一次
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
              

                UpdateGame(); // 更新游戏状态
                Thread.Sleep(updateInterval); // 暂停一定时间，再进行下一次更新
                cont++;
            }
           
           
        }

        // 计算某个位置的细胞邻居数量
        private int CountNeighbors(int x, int y)
        {
            int count = 0;

            // 检查8个邻居的状态
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // 排除自身

                    int nx = x + dx;
                    int ny = y + dy;

                    // 如果邻居在有效范围内，且是活细胞，增加计数
                    if (nx >= 0 && nx < GridWidth && ny >= 0 && ny < GridHeight && grid[nx, ny])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // 获取指定位置的细胞状态
        private bool GetLiveByPosition(int x, int y)
        {
            if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1))
            {
                return grid[x, y]; // 返回该位置的细胞状态
            }
            return false;  // 如果位置不在有效范围内，返回死细胞
        }

        // 初始化控件布局
        TextBox iterationsTextBox = new TextBox
        {
            Location = new Point(10, 140),
            Size = new Size(100, 30),
            Text = "10" // 默认值为10
        };
        private void InitializeLayout()
        {
            this.Text = "生命游戏";
            this.ClientSize = new Size(GridWidth * CellSize + 150, GridHeight * CellSize + 150); // 扩大窗体以适应新控件

            // 创建并设置控件
            var startButton = new Button
            {
                Text = "开始",
                Location = new Point(10, 10),
                Size = new Size(100, 40)
            };

            var stopButton = new Button
            {
                Text = "停止",
                Location = new Point(10, 60),
                Size = new Size(100, 40)
            };

            var iterationsLabel = new Label
            {
                Text = "迭代次数:",
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

            // 绑定绘制网格的事件
            gamePanel.Paint += GamePanel_Paint;

            // 绑定按钮点击事件
            startButton.Click += StartButton_Click;
            stopButton.Click += StopButton_Click;
            // 绑定运行迭代按钮的点击事件
    

        }
        static int cont = 0;
        // 运行指定次数的迭代
        
        // 开始按钮点击事件
        private void StartButton_Click(object sender, EventArgs e)
        {
          
            // 获得迭代次数
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
                    // 转换失败，说明输入的文本不符合整数格式要求，同样可以进行错误提示等处理
                    MessageBox.Show("输入的内容不是合法的整数，请重新输入。");
                }
                gameThread = new Thread(GameLoop); // 创建新线程
              
                gameThread.Start();  // 启动线程
                (sender as Button).Text = "已开始"; // 更新按钮文本
            }
        }

        // 停止按钮点击事件
        private void StopButton_Click(object sender, EventArgs e)
        {
            if (gameRunning)
            {
                lock (lockObj)
                {
                    gameRunning = false;
                }
               
                gameThread.Join();  // 等待线程结束
                (sender as Button).Text = "已停止"; // 更新按钮文本
            }
        }

        // 绘制网格
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            var g = Graphics.FromImage(offscreenBitmap);
            g.Clear(Color.White); // 清空画布

            // 遍历所有网格位置
            for (int x = 0; x < GridWidth; x++)
            {
                for (int y = 0; y < GridHeight; y++)
                {
                    // 绘制活细胞
                    if (grid[x, y])
                    {
                        g.FillRectangle(Brushes.Black, x * CellSize, y * CellSize, CellSize, CellSize);
                    }
                    // 绘制细胞边框
                    g.DrawRectangle(Pens.Gray, x * CellSize, y * CellSize, CellSize, CellSize);
                }
            }

            // 最后将离屏图像绘制到面板上
            e.Graphics.DrawImage(offscreenBitmap, 0, 0);
        }
    }
}
