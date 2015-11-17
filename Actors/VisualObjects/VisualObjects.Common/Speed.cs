// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace VisualObjects.Common
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class Speed
    {
        public Speed(Speed other)
        {
            this.XSpeed = other.XSpeed;
            this.YSpeed = other.YSpeed;
            this.ZSpeed = other.ZSpeed;
        }

        public Speed(double x, double y, double z)
        {
            this.XSpeed = x;
            this.YSpeed = y;
            this.ZSpeed = z;
        }

        [DataMember]
        public double XSpeed { get; private set; }

        [DataMember]
        public double YSpeed { get; private set; }

        [DataMember]
        public double ZSpeed { get; private set; }

        public static Speed CreateRandom(Random rand = null)
        {
            if (rand == null)
            {
                rand = new Random((int) DateTime.Now.Ticks);
            }

            return new Speed(rand.NextDouble()*0.03, rand.NextDouble()*0.03, rand.NextDouble()*0.03);
        }
    }
}