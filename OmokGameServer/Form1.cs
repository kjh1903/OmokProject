using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;  // �߰�
using System.Net; // �߰�
using System.Net.Sockets;  // �߰�
using System.IO;  // �߰�
using System.Security.Cryptography.Xml;
// ���� ���α׷�!!!!!

namespace OmokGame
{
    public partial class Form1 : Form
    {
        StreamReader streamReader1;  // ����Ÿ �б� ���� ��Ʈ������
        StreamWriter streamWriter1;  // ����Ÿ ���� ���� ��Ʈ��������   
        private const int BOARD_SIZE = 19;
        private const int CELL_SIZE = 30;
        private const int BOARD_OFFSET = 20;
        private int flag = 2;

        private enum StoneType { None, Black, White }

        private StoneType[,] board = new StoneType[BOARD_SIZE, BOARD_SIZE];
        private StoneType currentPlayer = StoneType.Black; // Black starts the game

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
        }


        private void InitializeBoard()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    board[row, col] = StoneType.None;
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)  // '�����ϱ�' ��ư�� Ŭ���Ǹ�
        {
            Thread thread1 = new Thread(connect); // Thread ��ä ����, Form���� ���� �����忡�� connect �Լ��� �����.
            thread1.IsBackground = true; // Form�� ����Ǹ� thread1�� ����.
            thread1.Start(); // thread1 ����.
        }
        private void connect()  // thread1�� ����� �Լ�. ���������� ������ �����Ѵ�.
        {
            TcpListener tcpListener1 = new TcpListener(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text)); // ���� ��ü ���� �� IP�ּҿ� Port��ȣ�� �Ҵ�
            tcpListener1.Start();  // ���� ����
            writeRichTextbox("���� �غ�...Ŭ���̾�Ʈ ��ٸ��� ��...");

            TcpClient tcpClient1 = tcpListener1.AcceptTcpClient(); // Ŭ���̾�Ʈ ���� Ȯ��
            writeRichTextbox("Ŭ���̾�Ʈ �����...");

            streamReader1 = new StreamReader(tcpClient1.GetStream());  // �б� ��Ʈ�� ����
            streamWriter1 = new StreamWriter(tcpClient1.GetStream());  // ���� ��Ʈ�� ����
            streamWriter1.AutoFlush = true;  // ���� ���� �ڵ����� ���� ó��..

            while (tcpClient1.Connected)  // Ŭ���̾�Ʈ�� ����Ǿ� �ִ� ����
            {
                string receiveData1 = streamReader1.ReadLine();  // ���� ����Ÿ�� �о receiveData1 ������ ����
                writeRichTextbox(receiveData1);  // ����Ÿ�� ����â�� ����
                string[] receiveData2 = receiveData1.Split(',');
                int x = int.Parse(receiveData2[0]);
                int y = int.Parse(receiveData2[1]);
                if (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE && board[y, x] == StoneType.None && this.flag!=1)
                {
                    board[y, x] = currentPlayer;
                    this.flag = 1;
                    this.Refresh(); // Redraw the board after the move
                    if (CheckWin(x, y) == true)
                    {

                        writeRichTextbox($"{currentPlayer} Win!");
                        writeRichTextbox($"5�� �Ŀ� �����մϴ�.");
                        Thread.Sleep(5000);
                        Close();
                    }
                    else
                    {
                        writeRichTextbox("����� ���� �Դϴ�.");  // ���ʸ� ����â�� ����
                    }
                    currentPlayer = currentPlayer == StoneType.Black ? StoneType.White : StoneType.Black;
                }

            }
        }
        private void writeRichTextbox(string str)  // richTextbox1 �� ���� �Լ�
        {
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.AppendText(str + "\r\n"); }); // ����Ÿ�� ����â�� ǥ��, �ݵ�� invoke ���. �浹����.
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); });  // ��ũ���� �� ������.
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            DrawBoard(e.Graphics);
            DrawStones(e.Graphics);
        }

        private void DrawBoard(Graphics g)
        {
            for (int row = 0; row < BOARD_SIZE-1; row++)
            {
                for (int col = 0; col < BOARD_SIZE-1; col++)
                {
                    Rectangle cellRect = new Rectangle(
                        BOARD_OFFSET + col * CELL_SIZE+15,
                        BOARD_OFFSET + row * CELL_SIZE+15,
                        CELL_SIZE, CELL_SIZE);

                    g.DrawRectangle(Pens.Black, cellRect);
                }
            }
        }

        private void DrawStones(Graphics g)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == StoneType.Black)
                    {
                        Rectangle stoneRect = new Rectangle(
                            BOARD_OFFSET + col * CELL_SIZE,
                            BOARD_OFFSET + row * CELL_SIZE,
                            CELL_SIZE, CELL_SIZE);

                        g.FillEllipse(Brushes.Black, stoneRect);
                    }
                    else if (board[row, col] == StoneType.White)
                    {
                        Rectangle stoneRect = new Rectangle(
                            BOARD_OFFSET + col * CELL_SIZE,
                            BOARD_OFFSET + row * CELL_SIZE,
                            CELL_SIZE, CELL_SIZE);

                        g.FillEllipse(Brushes.White, stoneRect);
                    }
                }
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            int x = (Cursor.Position.X - this.Location.X - BOARD_OFFSET) / CELL_SIZE;
            int y = (Cursor.Position.Y - this.Location.Y - BOARD_OFFSET - 30) / CELL_SIZE;

            if (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE && board[y, x] == StoneType.None && this.flag != 0)
            {
                int instant_flag = 0;
                if (currentPlayer == StoneType.Black)
                {
                    if (Check33Rule(x, y) == true)
                    {
                        writeRichTextbox("33�꿡 ����˴ϴ�. �ٽ� ���� �÷��ּ���.");
                        instant_flag = 1;
                    }
                }
                if (instant_flag == 0) {
                    board[y, x] = currentPlayer;
                    string sendData = $"{x},{y},{currentPlayer}";

                    streamWriter1.WriteLine(sendData);
                    this.flag = 0;
                    this.Refresh(); // Redraw the board after the move
                    if (CheckWin(x, y) == true)
                    {
                        writeRichTextbox($"{currentPlayer} Win!");
                        writeRichTextbox($"5�� �Ŀ� �����մϴ�.");
                        Thread.Sleep(5000);
                        Close();
                    }
                    else
                    {
                        writeRichTextbox("����� ���� �Դϴ�.");  // ���ʸ� ����â�� ����
                    }
                    currentPlayer = currentPlayer == StoneType.Black ? StoneType.White : StoneType.Black;
                }
            }
        }
        public bool CheckWin(int x, int y)
        {
            StoneType color = board[y, x];
            if (color == StoneType.None)
                return false;

            // Check horizontal, vertical, and diagonal directions for five consecutive stones
            int[] dx = { 0, 1, 1, 1 };
            int[] dy = { 1, 0, 1, -1 };

            for (int dir = 0; dir < 4; dir++)
            {
                int count = 1;
                for (int i = 1; i <= 4; i++)
                {
                    int nx = x + dx[dir] * i;
                    int ny = y + dy[dir] * i;

                    if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE && board[ny, nx] == color)
                        count++;
                    else
                        break;
                }

                for (int i = 1; i <= 4; i++)
                {
                    int nx = x - dx[dir] * i;
                    int ny = y - dy[dir] * i;

                    if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE && board[ny, nx] == color)
                        count++;
                    else
                        break;
                }

                if (count >= 5)
                    return true;
            }

            return false;
        }
        public bool Check33Rule(int x, int y)
        {
            if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE || board[y, x] != StoneType.None)
                return false;

            StoneType tempStone = board[y, x]; // ���� ���� ���� ����
            board[y, x] = StoneType.Black; // �ӽ÷� ���� ���� ���ƺ���

            int count = 0;
            int dx, dy;

            // �밢�� �������� 33 ��Ģ �˻�
            for (dx = -1; dx <= 1; dx += 2)
            {
                for (dy = -1; dy <= 1; dy += 2)
                {
                    int nx1 = x + dx;
                    int ny1 = y + dy;
                    int nx2 = x + dx * 2;
                    int ny2 = y + dy * 2;

                    if (nx1 >= 0 && nx1 < BOARD_SIZE && ny1 >= 0 && ny1 < BOARD_SIZE &&
                        nx2 >= 0 && nx2 < BOARD_SIZE && ny2 >= 0 && ny2 < BOARD_SIZE)
                    {
                        if (board[ny1, nx1] == StoneType.Black && board[ny2, nx2] == StoneType.Black)
                            count++;
                    }
                }
            }

            if (count >= 2)
            {
                // 33 ��Ģ�� �����Ǹ� ���� �����ϰ� false�� ��ȯ
                board[y, x] = tempStone; // ���� �� ���·� �ǵ�����
                return true;
            }

            // 33 ��Ģ�� �������� ������ true�� ��ȯ�ϰ� ���� �״�� ��������
            board[y, x] = tempStone; // ���� �� ���·� �ǵ�����
            return false;
        }

    }




}
