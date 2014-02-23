using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Oodles
{
    public class SwarmMember
    {
        public float thrust = 2.5f;
        private float angleAcceleration = 0.6f; // the smaller the swarm, the higher the acceleration (makes it look more compact)
        private float angleVelocityDamp = 0.9f;
        private float velocityDamp = 0.8f;

        private float xPos = 0.0f;
        private float yPos = 0.0f;

        private float xVel = 0.0f;
        private float yVel = 0.0f;

        public float rotationAngle = 0.0f;
        private float angleVelocity = 0.0f;

        private float lifePercentage = 1.0f;

        public void Update(PointF target)
        {
            // dx, dy (direction towards the target)
            float dX = target.X - xPos;
            float dY = target.Y - yPos;

            // calculate angle in radians to target
            double angle = Math.Atan2(dY, dX);

            double angDiff = angle - (rotationAngle / (180 / Math.PI));

            // find the shortest direction to rotate
            if (angDiff > Math.PI)
                angDiff -= Math.PI * 2;

            if (angDiff < -Math.PI)
                angDiff += Math.PI * 2;

            // limit rotation (angDiff) speed to some max speed (0.0029 radians)
            if (Math.Abs(angDiff) > angleAcceleration)
                angDiff *= angleAcceleration / Math.Abs(angDiff);

            // add angular velocity to rotation and damp angular velocity:
            angleVelocity += (float)angDiff;
            rotationAngle += (float)((angleVelocity *= angleVelocityDamp) * (180 / Math.PI));

            // increace velocity in the direction headed for:
            double rads = rotationAngle / (180 / Math.PI);
            xVel += ((float)Math.Cos(rads) * thrust);
            yVel += ((float)Math.Sin(rads) * thrust);

            // add velocity to position and damp velocities
            xPos += (xVel *= velocityDamp);
            yPos += (yVel *= velocityDamp);

            // shorten life of this member
            lifePercentage -= 0.0005f;
        }

        public float X
        {
            get { return xPos; }
            set { xPos = value; }
        }

        public float Y
        {
            get { return yPos; }
            set { yPos = value; }
        }

        public float VelocityX
        {
            get { return xVel; }
            set { xVel = value; }
        }

        public float VelocityY
        {
            get { return yVel; }
            set { yVel = value; }
        }

        public float LifePercentage
        {
            get { return lifePercentage; }
            set { lifePercentage = value; }
        }
    }
}
