using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Oodles
{
    public class Swarm
    {
        private List<SwarmMember> members = new List<SwarmMember>();
        private Random r = new Random();

        private PointF position;

        private Color color;

        private bool playerControlled = false;

        private float thrust = 2.5f;

        private float radius = 0;

        public Swarm(int size, PointF position, Color color)
        {
            this.position = position;
            this.color = color;

            AddMembers(size);
        }

        public void Update()
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].LifePercentage < 0.1f)
                {
                    members.RemoveAt(i);
                    break;
                }

                members[i].Update(position);

                radius = position.X - members[i].X;
            }
        }

        public void AddMembers(int amount)
        {
            thrust += 0.0025f;

            for (int i = 0; i < amount; i++)
            {
                SwarmMember member = new SwarmMember();
                member.X = position.X + r.Next(0, 500);
                member.Y = position.Y + r.Next(0, 500);

                float angle = (float)(r.NextDouble() * (Math.PI * 2));

                member.VelocityX = (float)(Math.Sin(angle) * 1.5f);
                member.VelocityY = (float)(Math.Cos(angle) * 1.5f);

                member.thrust = thrust;
                member.LifePercentage = r.Next(0, 4); // difficulty really
                members.Add(member);
            }
        }

        public void RemoveMembers(int amount)
        {
            thrust -= 0.0002f;
            members.RemoveRange(0, amount);
        }

        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        public PointF Position
        {
            get { return position; }
            set { position = value; }
        }

        public List<SwarmMember> Members
        {
            get { return members; }
            set { members = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public bool PlayerControlled
        {
            get { return playerControlled; }
            set { playerControlled = value; }
        }
    }
}
