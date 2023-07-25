using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;  // 추가
using System.Net; // 추가
using System.Net.Sockets;  // 추가
using System.IO;  // 추가
using System.Security.Cryptography.Xml;
// 서버 프로그램!!!!!

namespace OmokGame
{
    public partial class Form1 : Form
    {
        StreamReader streamReader1;  // 데이타 읽기 위한 스트림리더
        StreamWriter streamWriter1;  // 데이타 쓰기 위한 스트림라이터   
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
        private void button1_Click(object sender, EventArgs e)  // '연결하기' 버튼이 클릭되면
        {
            Thread thread1 = new Thread(connect); // Thread 객채 생성, Form과는 별도 쓰레드에서 connect 함수가 실행됨.
            thread1.IsBackground = true; // Form이 종료되면 thread1도 종료.
            thread1.Start(); // thread1 시작.
        }
        private void connect()  // thread1에 연결된 함수. 메인폼과는 별도로 동작한다.
        {
            TcpListener tcpListener1 = new TcpListener(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text)); // 서버 객체 생성 및 IP주소와 Port번호를 할당
            tcpListener1.Start();  // 서버 시작
            writeRichTextbox("서버 준비...클라이언트 기다리는 중...");

            TcpClient tcpClient1 = tcpListener1.AcceptTcpClient(); // 클라이언트 접속 확인
            writeRichTextbox("클라이언트 연결됨...");

            streamReader1 = new StreamReader(tcpClient1.GetStream());  // 읽기 스트림 연결
            streamWriter1 = new StreamWriter(tcpClient1.GetStream());  // 쓰기 스트림 연결
            streamWriter1.AutoFlush = true;  // 쓰기 버퍼 자동으로 뭔가 처리..

            while (tcpClient1.Connected)  // 클라이언트가 연결되어 있는 동안
            {
                string receiveData1 = streamReader1.ReadLine();  // 수신 데이타를 읽어서 receiveData1 변수에 저장
                writeRichTextbox(receiveData1);  // 데이타를 수신창에 쓰기
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
                        writeRichTextbox($"5초 후에 종료합니다.");
                        Thread.Sleep(5000);
                        Close();
                    }
                    else
                    {
                        writeRichTextbox("당신의 차례 입니다.");  // 차례를 수신창에 쓰기
                    }
                    currentPlayer = currentPlayer == StoneType.Black ? StoneType.White : StoneType.Black;
                }

            }
        }
        private void writeRichTextbox(string str)  // richTextbox1 에 쓰기 함수
        {
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.AppendText(str + "\r\n"); }); // 데이타를 수신창에 표시, 반드시 invoke 사용. 충돌피함.
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); });  // 스크롤을 젤 밑으로.
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
                        writeRichTextbox("33룰에 위배됩니다. 다시 돌을 올려주세요.");
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
                        writeRichTextbox($"5초 후에 종료합니다.");
                        Thread.Sleep(5000);
                        Close();
                    }
                    else
                    {
                        writeRichTextbox("상대의 차례 입니다.");  // 차례를 수신창에 쓰기
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

            StoneType tempStone = board[y, x]; // 기존 돌의 상태 저장
            board[y, x] = StoneType.Black; // 임시로 검은 돌을 놓아보기

            int count = 0;
            int dx, dy;

            // 대각선 방향으로 33 규칙 검사
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
                // 33 규칙이 만족되면 돌을 제거하고 false를 반환
                board[y, x] = tempStone; // 기존 돌 상태로 되돌리기
                return true;
            }

            // 33 규칙이 만족되지 않으면 true를 반환하고 돌은 그대로 남아있음
            board[y, x] = tempStone; // 기존 돌 상태로 되돌리기
            return false;
        }

    }




}
