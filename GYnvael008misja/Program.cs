using System;
using System.Net;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using OpenTK.Input;
using System.Threading;


namespace GYnvael008misja
{
    class Program
    {
        public static Move start;
        static void Main(string[] args)
        {
            MainWindow Window = new MainWindow();
            Window.Run();
            Console.ReadKey();
        }
    }
    public static class Simulation
    {
        public static Move start = new Move("68eb1a7625837e38d55c54dc99257a17.txt", "north");
        public static List<Point> Vertices = new List<Point>();
        public static Point DronePosition;
        public static List<Point> DronePath = new List<Point>();
        public static Point MaxValue = new Point(64, 64);
        public static bool bythewall = false;
        public static float grid = 0.05f;
        public static bool snaptoWall = true;
        public static int resx = 800;
        public static int resy = 800;  
    }
    public static class Download
    {
        public static Random rand = new Random();
        public static WebClient download = new WebClient();
        public static int counter = 0;
        public static Move DownloadFile(string path, string olddirection)
        {
            string direction = olddirection;
            while (true)
            {
                try
                {
                    download.DownloadFile(String.Format("http://gynvael.coldwind.pl/misja008_drone_io/scans/" + path), path);
                    counter++;
                    Console.Clear();
                    Console.WriteLine(counter + " Files downloaded");
                    break;
                }
                catch
                {
                    Console.WriteLine("Connection Error: Another Try...");
                    continue;
                }
            }
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            file.ReadLine();
            string[] numbers = file.ReadLine().Split(' ');
            int[] position = { int.Parse(numbers[0]), int.Parse(numbers[1]) };
            Simulation.DronePosition = new Point(position[0], position[1]);
            if (Simulation.DronePosition.X + 64 > Simulation.MaxValue.X || Simulation.DronePosition.Y + 64 > Simulation.MaxValue.Y)
            {
                Simulation.MaxValue.X = Math.Max(Simulation.DronePosition.X + 32, Simulation.DronePosition.Y + 32);
                Simulation.MaxValue.Y = Simulation.MaxValue.X; 
            }
            
            for (int i = 0; i < 36; i++)
            {
                numbers = file.ReadLine().Split();
                try
                {
                    if (numbers[0] != "inf")
                    {
                        double scan = double.Parse(numbers[0].Replace('.', ','));
                        double x = scan * Math.Sin(i * 10 * Math.PI / 180);
                        double y = scan * Math.Cos(i * 10 * Math.PI / 180);

                        Simulation.Vertices.Add(new Point((int)(Simulation.DronePosition.X + x), Simulation.DronePosition.Y - (int)y));
                    }

                }
                catch
                {
                    continue;
                }
            }
            numbers = file.ReadLine().Split();
            string east = numbers[1];
            numbers = file.ReadLine().Split();
            string west = numbers[1];
            numbers = file.ReadLine().Split();
            string south = numbers[1];
            numbers = file.ReadLine().Split();
            string north = numbers[1];

            if(Simulation.bythewall == false)
            {
                if (direction == "north" && north == "not") { direction = "east"; Simulation.bythewall = true; }
                else if (direction == "east" && east == "not") { direction = "south"; Simulation.bythewall = true; }
                else if (direction == "south" && south == "not") { direction = "west"; Simulation.bythewall = true; }
                else if (direction == "west" && west == "not") { direction = "north"; Simulation.bythewall = true; }
                else { direction = olddirection; }
            }
            if(Simulation.bythewall == true)
            {
                if(direction == "east")
                {
                    if(north != "not")
                    {
                        direction = "north";
                    }
                    else if(east == "not")
                    {
                        direction = "south";
                    }
                    else
                    {
                        direction = "east";
                    }
                }
                else if (direction == "north")
                {
                    if (west != "not")
                    {
                        direction = "west";
                    }
                    else if (north == "not")
                    {
                        direction = "east";
                    }
                    else
                    {
                        direction = "north";
                    }
                }
                else if(direction == "south")
                {
                    if (east != "not")
                    {
                        direction = "east";
                    }
                    else if (south == "not")
                    {
                        direction = "west";
                    }
                    else
                    {
                        direction = "south";
                    }
                }
                else if (direction == "west")
                {
                    if (south != "not")
                    {
                        direction = "south";
                    }
                    else if (west == "not")
                    {
                        direction = "north";
                    }
                    else
                    {
                        direction = "west";
                    }
                }
                
            }
            if (direction == "north" && north == "not") direction = "east";
            if (direction == "south" && south == "not") direction = "west";
            if (direction == "west" && west == "not") direction = "north";
            if (direction == "east" && east == "not") direction = "south";
            if (direction == "east")
            {
                return new Move(east, direction);
            }
            else if (direction == "west") return new Move(west, direction);
            else if (direction == "north") return new Move(north, direction);
            else if (direction == "south") return new Move(south, direction);

            else
            {
                throw new Exception("ERROR: NO DIRECTION RETURNED");
            }

        }
    }
    public class Move
    {
        public string adress;
        public string direction;
        public Move(string adress, string direction)
        {
            this.adress = adress;
            this.direction = direction;
        }
    }
    public sealed class MainWindow : GameWindow
    {
        public MainWindow() : base(Simulation.resx,Simulation.resy,GraphicsMode.Default,"SECRET: DRONESCAN")
        {

        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            Move start = new Move("68eb1a7625837e38d55c54dc99257a17.txt", "north");
            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Simulation.start = Download.DownloadFile(Simulation.start.adress, Simulation.start.direction);
            if (Keyboard[Key.Escape])
                Exit();

            if (Keyboard[Key.Down])
            {
                Simulation.bythewall = false;
                Simulation.start.direction = "south";
            }
            if (Keyboard[Key.Up])
            {
                Simulation.bythewall = false;
                Simulation.start.direction = "north";
            }
            if (Keyboard[Key.Right])
            {
                Simulation.bythewall = false;
                Simulation.start.direction = "east";
            }
            if (Keyboard[Key.Left])
            {
                Simulation.bythewall = false;
                Simulation.start.direction = "west";
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.1f, 0.15f, 0.07f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, Simulation.MaxValue.X, Simulation.MaxValue.Y, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.PushMatrix();
            GL.Begin(BeginMode.Lines);
            GL.Color3(0.2, 0.2, 0.2);
            for (int i = 0; i < Simulation.MaxValue.Y; i += 64)
            {
                GL.Vertex2(0,i);
                GL.Vertex2(Simulation.MaxValue.X, i);
            }
            for (int i = 0; i < Simulation.MaxValue.X; i += 64)
            {
                GL.Vertex2(i, 0);
                GL.Vertex2(i,Simulation.MaxValue.Y);
            }
            GL.End();
            GL.PointSize(1.0f);
            GL.Begin(PrimitiveType.Points);
            for(int i=0; i<Simulation.MaxValue.X; i += 8)
            {
                for (int j = 0; j < Simulation.MaxValue.Y; j += 8)
                {
                    GL.Color3(0.2, Simulation.grid, 0.2);
                    GL.Vertex2(i, j);
                }
            }
            GL.End();
            GL.PointSize(2.0f);
            GL.Begin(BeginMode.Points);
            GL.Color3(1.0, 1.0, 1.0);
            for (int i = 0; i < Simulation.Vertices.Count; i++)
            {
                GL.Vertex2(Simulation.Vertices[i].X, Simulation.Vertices[i].Y);
            }
            GL.End();
            GL.PointSize(5.0f);
            GL.Color3(1.0, 0, 0);
            GL.Begin(BeginMode.Points);
            GL.Vertex2(Simulation.DronePosition.X, Simulation.DronePosition.Y);
            GL.End();
            Simulation.grid += 0.05f;
            Simulation.grid = (float)((double)Simulation.grid % 1.0); 
            this.SwapBuffers();
            Thread.Sleep(120);        }
    }
}