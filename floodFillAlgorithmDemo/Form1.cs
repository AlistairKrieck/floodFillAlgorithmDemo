using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;

namespace floodFillAlgorithmDemo
{
    //Create a flood fill algorith
    //Check if the tile that was clicked is on (only allow filling, not clearing)
    //Check its neighbours states
    //If they are on, ignore them
    //If they are off, turn them on, then check their neighbours
    //Repeat until all tiles in selected area are turned on


    public partial class Form1 : Form
    {
        Grid gameGrid;
        Grid prevGameState;

        const int tileSize = 15; // Size of each tile
        int gridWidth;
        int gridHeight;

        bool mouseDown = false;
        int mouseX;
        int mouseY;

        string gameState = "startScreen";
        string selectedTool = "pen";

        const string openMsg = "sampleStartScreen"; //Placeholder for start screen message

        Tile clickedTile = null; //Made public because I don't know any better solutions

        public Form1()
        {
            InitializeComponent();
            GameInit();

            // Set the form to start maximized
            this.WindowState = FormWindowState.Maximized;

            // Disable the maximize box
            this.MaximizeBox = false;

            // Disable the ability to restore down
            this.Resize += Form1_Resize;
        }

        private void GameInit()
        {
            //Opens the game on the start screen
            gameState = "startScreen";

            // Hook up the Paint, KeyDown, and Resize events
            this.BackColor = Color.Black;
            this.Paint += new PaintEventHandler(Form1_Paint);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.Resize += new EventHandler(Form1_Resize);

            // Initialize the game grid
            UpdateGridSize();
        }

        private void UpdateGridSize()
        {
            //Changes grid size to match clients screen dimentions
            gridWidth = this.ClientSize.Width / tileSize;
            gridHeight = this.ClientSize.Height / tileSize;

            // Initialize game grid and player position
            gameGrid = new Grid(gridWidth, gridHeight, tileSize);

            // Redraw the grid
            Refresh();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //Displays start screen
            if (gameState == "startScreen")
            {
                titleLabel.Text = openMsg;
                titleLabel.Visible = true;
            }

            //Draws grid to screen upon leaving the start screen
            else
            {
                titleLabel.Visible = false;

                // Draw the game grid

                titleLabel.Visible = false;

                // Draw the game grid
                foreach (var tile in gameGrid.Tiles)
                {
                    //Colors tiles in the "on" state white
                    using (Brush onBrush = new SolidBrush(Color.White))
                    {
                        if (tile.on == true)
                        {
                            e.Graphics.FillRectangle(onBrush, tile.X, tile.Y, tile.Size, tile.Size);
                        }
                    }

                    //Outlines each tile gray
                    using (Pen gridOutline = new Pen(Color.Gray))
                    {
                        e.Graphics.DrawRectangle(gridOutline, tile.X, tile.Y, tile.Size, tile.Size);
                    }
                }
            }
        }

        //Allows player to interact with the game
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //Default pause / start key
                case Keys.Space:
                    if (gameState != "paused")
                    {
                        gameState = "paused";
                        Refresh();
                    }

                    else
                    {
                        gameState = "running";
                    }
                    break;

                //Default "close form" key
                case Keys.Escape:
                    this.Close();
                    break;

                //Default "clear grid" key
                case Keys.R:
                    if (gameState == "paused")
                    {
                        ClearGrid();
                    }

                    break;

                case Keys.F:
                    FloodFill();
                    break;
            }
        }

        private void ClearGrid()
        {
            //Sets all tiles to the "off" state
            foreach (var tile in gameGrid.Tiles)
            {
                if (tile.on == true)
                {
                    tile.on = false;
                }
            }
            Refresh();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // Ensure the form stays maximized
            if (this.WindowState != FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                // Update grid size and redraw when form is resized
                UpdateGridSize();
            }
        }

        // Define a class to represent a tile
        public class Tile
        {
            //Sets positions on screen
            public int X { get; set; }
            public int Y { get; set; }

            //Defines size of tiles according to tileSize variable
            public int Size { get; set; }
            public bool on { get; set; }

            //Gives a tiles X/Y coordinates relative to its grid
            public int refX { get; set; }
            public int refY { get; set; }
        }

        // Define a class to represent the game grid
        public class Grid
        {
            //Holds each individual tile in the grid
            public List<Tile> Tiles { get; set; }

            //Defines a grid
            public Grid(int width, int height, int tileSize)
            {
                //Generates grid with tiles defaulting to the "off" state
                Tiles = new List<Tile>();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Tiles.Add(new Tile
                        {
                            X = x * tileSize,
                            Y = y * tileSize,
                            Size = tileSize,
                            on = false,
                            refX = x,
                            refY = y
                        });
                    }
                }
            }
        }

        //Gets the states of each tiles neighbours
        public List<Tile> GetNeighbourStates(Tile tile)
        {
            //Defines positions of neighbours relative to a given tile
            List<Tile> neighbours = new List<Tile>();
            int[] dx = { 0, -1, 1, 0, 0 };
            int[] dy = { -1, 0, 0, 1, 0 };

            //Checks neighbours states
            for (int i = 0; i < 4; i++)
            {
                int newX = tile.refX + dx[i];
                int newY = tile.refY + dy[i];

                Tile neighbour = gameGrid.Tiles.Find(t => t.refX == newX && t.refY == newY);

                //Adds existing tiles to the requested list
                if (neighbour != null && neighbour.on == false)
                {
                    neighbours.Add(neighbour);
                }
            }

            return neighbours;
        }

        //Updates game state each frame
        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (gameState == "paused")
            {
                Draw();

                Refresh();
            }
        }

        private void FloodFill()
        {
            //Create a flood fill algorith
            //Check if the tile that was clicked is on (only allow filling, not clearing)
            //Check its neighbours states
            //If they are on, ignore them
            //If they are off, turn them on, then check their neighbours
            //Repeat until all tiles in selected area are turned on
            List<Tile> refTiles = new List<Tile>();
            List<Tile> temp = new List<Tile>();

            refTiles.Clear();

            if (clickedTile.on == true)
            {
                refTiles.Add(clickedTile);

                while (refTiles.Count > 0)
                {
                    for (int i = 0; i < refTiles.Count; i++)
                    {
                        foreach (var tile in GetNeighbourStates(refTiles[i]))
                        {
                            tile.on = true;
                            Refresh();

                            if (GetNeighbourStates(tile).Count > 0)
                            {
                                temp.Add(tile);
                            }
                        }
                    }

                    refTiles.Clear();

                    for (int i = 0; i < temp.Count; i++)
                    {
                        refTiles.Add(temp[i]);
                    }

                    temp.Clear();
                }
            }
        }

        private void Draw()
        {
            if (mouseDown == true)
            {
                mouseX = MousePosition.X;
                mouseY = MousePosition.Y - tileSize;

                //Allows user to switch tile states "on" using pen tool
                clickedTile = gameGrid.Tiles.Find(t => t.X <= mouseX && t.X + tileSize >= mouseX
                    && t.Y <= mouseY && t.Y + tileSize >= mouseY);
                clickedTile.on = true;
            }
        }
        
        private void Erase()
        {
            if (mouseDown == true)
            {
                mouseX = MousePosition.X;
                mouseY = MousePosition.Y - tileSize;

                //Allows user to switch tile states "off" using eraser tool
                clickedTile = gameGrid.Tiles.Find(t => t.X <= mouseX && t.X + tileSize >= mouseX
                    && t.Y <= mouseY && t.Y + tileSize >= mouseY);
                clickedTile.on = false;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }
    }
}