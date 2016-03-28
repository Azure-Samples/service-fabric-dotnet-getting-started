// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.Common
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    public sealed class Coordinate
    {
        public Coordinate(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Coordinate(Coordinate other)
        {
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
        }

        [DataMember]
        public double X { get; private set; }

        [DataMember]
        public double Y { get; private set; }

        [DataMember]
        public double Z { get; private set; }

        public static Coordinate CreateRandom(Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random((int) DateTime.Now.Ticks);
            }

            return new Coordinate(rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
        }

        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            this.ToJson(sb);

            return sb.ToString();
        }

        public void ToJson(StringBuilder builder)
        {
            builder.AppendFormat(
                "{{ \"x\":{0}, \"y\":{1}, \"z\":{2} }}",
                this.X.ToString(NumberFormatInfo.InvariantInfo),
                this.Y.ToString(NumberFormatInfo.InvariantInfo),
                this.Z.ToString(NumberFormatInfo.InvariantInfo));
        }
    }
}