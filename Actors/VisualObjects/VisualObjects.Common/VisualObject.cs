// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.Common
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public class VisualObject
    {
        private const int HistoryLength = 7;

        public VisualObject(string name, Speed speed, Coordinate location, Color color, Color historyColor, double rotation = 0)
        {
            this.Name = name;
            this.Speed = speed;
            this.CurrentLocation = location;
            this.CurrentColor = color;
            this.HistoryColor = historyColor;
            this.Rotation = rotation;
            this.LocationHistory = new List<Coordinate>();
            this.HistoryStartIndex = -1;
        }

        public VisualObject(VisualObject other)
        {
            this.Name = other.Name;
            this.Speed = new Speed(other.Speed);

            this.CurrentLocation = new Coordinate(other.CurrentLocation);
            this.LocationHistory = new List<Coordinate>(other.LocationHistory.Count);
            foreach (Coordinate c in other.LocationHistory)
            {
                this.LocationHistory.Add(new Coordinate(c));
            }

            this.CurrentColor = new Color(other.CurrentColor);
            this.HistoryColor = new Color(other.HistoryColor);

            this.Rotation = other.Rotation;
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public Speed Speed { get; private set; }

        [DataMember(Name = "current")]
        public Coordinate CurrentLocation { get; set; }

        [DataMember]
        public Color CurrentColor { get; set; }

        [DataMember]
        public Color HistoryColor { get; set; }

        [DataMember]
        public int HistoryStartIndex { get; set; }

        [DataMember(Name = "history")]
        public List<Coordinate> LocationHistory { get; private set; }

        [DataMember]
        public double Rotation { get; set; }

        public static VisualObject CreateRandom(string name, Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random(name.GetHashCode());
            }

            return new VisualObject(
                name,
                Speed.CreateRandom(rand),
                Coordinate.CreateRandom(rand),
                Color.CreateRandom(Color.CurrentColorsPalette, rand),
                Color.CreateRandom(Color.HistoryColorsPalette, rand));
        }

        public void Move(bool rotate)
        {
            if (this.LocationHistory.Count < HistoryLength)
            {
                this.HistoryStartIndex = (this.HistoryStartIndex + 1);
                this.LocationHistory.Add(new Coordinate(this.CurrentLocation));
            }
            else
            {
                this.HistoryStartIndex = (this.HistoryStartIndex + 1)%HistoryLength;
                this.LocationHistory[this.HistoryStartIndex] = new Coordinate(this.CurrentLocation);
            }

            double xSpeed = this.Speed.XSpeed;
            double ySpeed = this.Speed.YSpeed;
            double zSpeed = this.Speed.ZSpeed;

            double x = this.CurrentLocation.X + xSpeed;
            double y = this.CurrentLocation.Y + ySpeed;
            double z = this.CurrentLocation.Z + zSpeed;

            this.CurrentLocation = new Coordinate(this.CurrentLocation.X + xSpeed, this.CurrentLocation.Y + ySpeed, this.CurrentLocation.Z + zSpeed);

            // trim to edges
            this.Speed = new Speed(
                CheckForEdge(x, xSpeed),
                CheckForEdge(y, ySpeed),
                CheckForEdge(z, zSpeed));

            if (rotate)
            {
                this.Rotation = 5;
            }
            else
            {
                this.Rotation = 0;
            }
        }

        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            this.ToJson(sb);

            return sb.ToString();
        }

        public void ToJson(StringBuilder builder)
        {
            builder.Append("{");

            {
                builder.Append("\"current\":");
                this.CurrentLocation.ToJson(builder);
            }

            {
                builder.Append(", \"history\":");
                builder.Append("[");
                int currentIndex = this.HistoryStartIndex;
                if (currentIndex != -1)
                {
                    bool first = true;
                    do
                    {
                        currentIndex++;
                        if (currentIndex == this.LocationHistory.Count)
                        {
                            currentIndex = 0;
                        }

                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            builder.Append(", ");
                        }

                        this.LocationHistory[currentIndex].ToJson(builder);
                    } while (currentIndex != this.HistoryStartIndex);
                }
                builder.Append("]");
            }

            {
                builder.Append(", \"currentColor\":");
                this.CurrentColor.ToJson(builder);
            }

            {
                builder.Append(", \"historyColor\":");
                this.HistoryColor.ToJson(builder);
            }

            {
                builder.Append(", \"rotation\":");
                builder.Append(this.Rotation);
            }

            builder.Append("}");
        }

        private static double CheckForEdge(double point, double speed)
        {
            if (point < -1.0 || point > 1.0)
            {
                return speed*-1.0;
            }

            return speed;
        }
    }
}
