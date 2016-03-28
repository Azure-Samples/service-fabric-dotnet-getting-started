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
    public sealed class Color
    {
        public static double[][] CurrentColorsPalette =
        {
            new[] {0.0, 0.0, 1.0, 0.0},
            new[] {0.0, 1.0, 0.0, 0.0},
            new[] {1.0, 0.0, 0.0, 0.0}
        };

        public static double[][] HistoryColorsPalette =
        {
            new[] {1.0, 0.0, 0.0, 0.0},
            new[] {1.0, 1.0, 0.0, 0.0},
            new[] {1.0, 1.0, 1.0, 0.0}
        };

        public Color(double r, double g, double b, double a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Color(Color other)
        {
            this.R = other.R;
            this.G = other.G;
            this.B = other.B;
            this.A = other.A;
        }

        [DataMember]
        public double R { get; private set; }

        [DataMember]
        public double G { get; private set; }

        [DataMember]
        public double B { get; private set; }

        [DataMember]
        public double A { get; private set; }

        public static Color CreateRandom(double[][] colorPalette, Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random((int) DateTime.Now.Ticks);
            }

            int colorIndex = rand.Next(colorPalette.GetLength(0));

            return new Color(
                r: colorPalette[colorIndex][0] + rand.NextDouble(),
                g: colorPalette[colorIndex][1] + rand.NextDouble(),
                b: colorPalette[colorIndex][2] + rand.NextDouble(),
                a: colorPalette[colorIndex][3] + rand.NextDouble());
        }

        public void ToJson(StringBuilder builder)
        {
            builder.AppendFormat(
                "{{ \"r\":{0}, \"g\":{1}, \"b\":{2}, \"a\":{3} }}",
                this.R.ToString(NumberFormatInfo.InvariantInfo),
                this.G.ToString(NumberFormatInfo.InvariantInfo),
                this.B.ToString(NumberFormatInfo.InvariantInfo),
                this.A.ToString(NumberFormatInfo.InvariantInfo));
        }
    }
}