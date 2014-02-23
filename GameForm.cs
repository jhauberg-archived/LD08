using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Oodles
{
    public partial class GameForm : DisplayForm
    {
        private float elapsedTime = 0.0f;
        private bool mouseDown = false;
        private bool paused = true;

        private Random r = new Random();

        private List<Swarm> swarms = new List<Swarm>();

        private Sprite sprite;
        private Texture texture;
        private Microsoft.DirectX.Direct3D.Font font;

        private CustomVertex.TransformedColored[] backgroundVertices = new CustomVertex.TransformedColored[4];
        private short[] backgroundIndices;

        private Swarm playerSwarm;
        private PointF playerPosition;

        private Color[] tribes;

        private Timer timer;

        private HighScoreTable scores;

        private bool lost = false;

        private double highestSwarm = 0;
        private string playerName;

        public GameForm()
            : base("Dewd", new Size(1024, 768), false)
        {
            InitializeGraphics();

            playerName = Utility.UserName;

            scores = new HighScoreTable(@"Data\HighScores.xml", 15);
            scores.Load();

            this.font = new Microsoft.DirectX.Direct3D.Font(this.Device, new System.Drawing.Font("Tahoma", 10));

            sprite = new Sprite(this.Device);
            texture = TextureLoader.FromFile(this.Device, @"Data\Graphics\dot.png");

            playerPosition = new PointF(this.Width / 2, this.Height / 2);
            
            playerSwarm = new Swarm(
                25, 
                playerPosition,
                Color.Lime);

            playerSwarm.PlayerControlled = true;

            highestSwarm = 25;

            tribes = new Color[4] 
            {
                Color.Black,
                Color.Crimson,
                Color.Orange,
                Color.Snow
            };

            this.swarms.Add(playerSwarm);

            timer = new Timer();
            timer.Interval = 5000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Enabled = true;
            timer.Start();

            // defining our screen sized quad, note the Z value of 1f to place it in the background
            backgroundVertices[0].Position = new Vector4(0, 0, 1f, 1f);
            backgroundVertices[0].Color = System.Drawing.Color.SteelBlue.ToArgb();

            backgroundVertices[1].Position = new Vector4(this.ClientSize.Width, 0, 1f, 1f);
            backgroundVertices[1].Color = System.Drawing.Color.SteelBlue.ToArgb();

            backgroundVertices[2].Position = new Vector4(0, this.ClientSize.Height, 1f, 1f);
            backgroundVertices[2].Color = System.Drawing.Color.DarkSlateBlue.ToArgb();

            backgroundVertices[3].Position = new Vector4(this.ClientSize.Width, this.ClientSize.Height, 1f, 1f);
            backgroundVertices[3].Color = System.Drawing.Color.DarkSlateBlue.ToArgb();

            backgroundIndices = new short[] { 0, 1, 2, 1, 3, 2 };

            Utility.Timer(DirectXTimer.Start);

            timer_Tick(null, null);
            timer_Tick(null, null);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Color tribe = tribes[r.Next(0, tribes.Length)];

            int minAmount = 5;

            if (minAmount > playerSwarm.Members.Count / 2)
                minAmount = 0;

            int membersAmount = r.Next(minAmount, playerSwarm.Members.Count / 2);

            if (membersAmount >= 100)
                membersAmount = 100;

            foreach (Swarm swarm in swarms)
            {
                if (!swarm.PlayerControlled)
                {
                    if (swarm.Color == tribe)
                    {
                        swarm.AddMembers(membersAmount);
                        return;
                    }
                }
            }

            Swarm newSwarm = new Swarm(
                membersAmount,
                new PointF((float)r.Next(100, this.Width - 100), (float)r.Next(100, this.Height - 100)),
                tribe);

            swarms.Add(newSwarm);
        }

        protected override void OnFrame()
        {
            elapsedTime = Utility.Timer(DirectXTimer.GetElapsedTime);

            if (paused)
                return;

            // loop through all swarms for drawing (foreach would be faster, but we want to modify the collection (empty swarm -> remove))
            for (int i = 0; i < swarms.Count; i++)
            {
                swarms[i].Update();

                if (swarms[i].Members.Count <= 1)
                {
                    swarms.RemoveAt(i);
                    break;
                }

                // loop through all swarms
                for (int j = 0; j < swarms.Count; j++)
                {
                    if (swarms[j] != swarms[i] &&
                        swarms[j].Color != swarms[i].Color)
                    {
                        // .. not same swarm / same "tribe", so proceed applying AI and do collision checking
                        if (!swarms[i].PlayerControlled)
                        {
                            // this swarm is not player controlled.. Apply AI
                            float dX = swarms[j].Position.X - swarms[i].Position.X;
                            float dY = swarms[j].Position.Y - swarms[i].Position.Y;

                            double dirX = Math.Cos(Math.Atan2(dY, dX));
                            double dirY = Math.Sin(Math.Atan2(dY, dX));

                            // this is aggressive behavior, for fleeing behavior switch blocks (note: swarms will flee if theres no smaller prey)
                            if (swarms[j].Members.Count <= swarms[i].Members.Count)
                            {
                                float newX = swarms[i].Position.X - (float)-dirX;
                                float newY = swarms[i].Position.Y - (float)-dirY;

                                swarms[i].Position = new PointF(
                                    newX,
                                    newY);

                                // TODO: make choice, if theres both smaller and bigger swarms, choose whether or not to pursue smaller or evade bigger ones
                                // Update: actually the current behavior works nicely

                                // BUG/ANNOYANCE: swarms move at same speed, so a pursuit will last until the pursued is at an edge of screen :|
                                // Temp fix: swarms only flee from player
                            }
                            else
                            {
                                // flee (only if enemy is close, else all swarms will stay in corners/edges of screen)
                                if (swarms[j].PlayerControlled &&
                                    (dX < 200 && dX > -200) &&
                                    (dY < 200 && dY > -200))
                                {
                                    float newX = swarms[i].Position.X - (float)dirX;
                                    float newY = swarms[i].Position.Y - (float)dirY;

                                    swarms[i].Position = new PointF(
                                        newX,
                                        newY);
                                }
                                else
                                {
                                    // cruise around :D
                                    float newX = swarms[i].Position.X - (float)(r.Next(0, 2) > 0 ? r.Next(1, 3) : -r.Next(1, 3));
                                    float newY = swarms[i].Position.Y - (float)(r.Next(0, 2) > 0 ? r.Next(1, 3) : -r.Next(1, 3));

                                    swarms[i].Position = new PointF(
                                        newX,
                                        newY);
                                }
                            }

                            if (swarms[i].Position.X <= 0)
                                swarms[i].Position = new PointF(0, swarms[i].Position.Y);
                            else if (swarms[i].Position.X > this.Width)
                                swarms[i].Position = new PointF((float)this.Width, swarms[i].Position.Y);

                            if (swarms[i].Position.Y <= 0)
                                swarms[i].Position = new PointF(swarms[i].Position.X, 0);
                            else if (swarms[i].Position.Y > this.Height)
                                swarms[i].Position = new PointF(swarms[i].Position.X, (float)this.Height);
                        }

                        // Check for collisions
                        // TODO: use a circle instead of rect
                        // Although.. this inprecise method adds a flavour of randomism to the game which fits quite nicely
                        Rectangle firstSwarmRect = new Rectangle((int)(swarms[i].Position.X - swarms[i].Radius), (int)(swarms[i].Position.Y - swarms[i].Radius), (int)(swarms[i].Radius * 2), (int)(swarms[i].Radius * 2));
                        Rectangle secondSwarmRect = new Rectangle((int)(swarms[j].Position.X - swarms[j].Radius), (int)(swarms[j].Position.Y - swarms[j].Radius), (int)(swarms[j].Radius * 2), (int)(swarms[j].Radius * 2));

                        if (firstSwarmRect.IntersectsWith(secondSwarmRect))
                        {
                            // do fight, no events needed as fights should be generic for all swarms (ie. no special implementation for Players' Swarm)

                            // BUG: if a swarm pursues a bigger swarm it just dissappears with no "traveling to the target"
                            // FIXED: smaller swarms will now never pursue a bigger one, cheesy but whatever :P

                            if (swarms[i].Members.Count >= swarms[j].Members.Count)
                            {
                                // some members will go lost once in a while, but hey, that's war right?
                                int add = r.Next(0, swarms[j].Members.Count / 2);

                                if (add <= 3 && swarms[j].Members.Count <= 3)
                                    add = swarms[j].Members.Count;

                                swarms[i].AddMembers(add);

                                // remove 1 extra, to limit the sizes further
                                swarms[j].RemoveMembers(add);

                                if (swarms[i].PlayerControlled)
                                {
                                    if (swarms[i].Members.Count >= highestSwarm)
                                    {
                                        highestSwarm += add;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (playerSwarm.Members.Count <= 1)
            {
                //scores.Add(playerName, highestSwarm);
                lost = true;
            }
        }

        protected override void Render()
        {
            try
            {
                this.Device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, Color.CornflowerBlue, 1, 0);
                this.Device.BeginScene();

                // render our gradient background quad
                this.Device.VertexFormat = CustomVertex.TransformedColored.Format;
                this.Device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, 6, 2, backgroundIndices, true, backgroundVertices);

                if (lost)
                {
                    if (paused)
                        paused = !paused;

                    this.font.DrawText(
                        null,
                        "You lost.",
                        this.ClientRectangle,
                        DrawTextFormat.Center | DrawTextFormat.VerticalCenter,
                        Color.White);
                }

                if (paused)
                {
                    this.font.DrawText(
                        null,
                        "Paused - Press SPACE to Begin/Continue.",
                        this.ClientRectangle,
                        DrawTextFormat.Center | DrawTextFormat.VerticalCenter,
                        Color.White);
                }

                this.font.DrawText(
                    null, 
                    String.Format("Your swarm size: {0} - Swarms currently present: {1}", playerSwarm.Members.Count, swarms.Count), 
                    this.ClientRectangle, 
                    DrawTextFormat.Top | DrawTextFormat.Left, 
                    Color.White);

                this.font.DrawText(
                    null,
                    String.Format("Highest swarm: {0}", highestSwarm),
                    this.ClientRectangle,
                    DrawTextFormat.Top | DrawTextFormat.Right,
                    Color.White);

                // render
                sprite.Begin(SpriteFlags.AlphaBlend);

                foreach (Swarm swarm in swarms)
                {
                    foreach (SwarmMember member in swarm.Members)
                    {
                        // draw it
                        sprite.Draw2D(
                            texture,
                            new PointF(8, 8),
                            member.rotationAngle,
                            new PointF(member.X, member.Y),
                            swarm.Color);
                    }
                }

                sprite.End();
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Something bad: {0}", e));
            }
            finally
            {
                this.Device.EndScene();
                this.Device.Present();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseDown = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseDown = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                playerPosition.X = (float)e.X;
                playerPosition.Y = (float)e.Y;

                playerSwarm.Position = playerPosition;
            }
        }

        protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            // Handle the escape key for quiting
            if (e.KeyCode == Keys.Escape)
            {
                // Close the form and return
                this.Close();
                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                paused = !paused;

                if (paused)
                    timer.Stop();
                else
                    timer.Start();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            scores.Add(playerName, highestSwarm);
            scores.Save();
        }
    }
}