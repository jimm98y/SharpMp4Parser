﻿using SharpMp4Parser.IsoParser.Tools;
using SharpMp4Parser.Java;
using System;

namespace SharpMp4Parser.IsoParser.Support
{
    /**
     * Transformation Matrix as used in <code>Track-</code> and <code>MovieHeaderBox</code>.
     */
    public class Matrix
    {
        public static readonly Matrix ROTATE_0 = new Matrix(1, 0, 0, 1, 0, 0, 1, 0, 0);
        public static readonly Matrix ROTATE_90 = new Matrix(0, 1, -1, 0, 0, 0, 1, 0, 0);
        public static readonly Matrix ROTATE_180 = new Matrix(-1, 0, 0, -1, 0, 0, 1, 0, 0);
        public static readonly Matrix ROTATE_270 = new Matrix(0, -1, 1, 0, 0, 0, 1, 0, 0);
        double u, v, w;
        double a, b, c, d, tx, ty;

        public Matrix(double a, double b, double c, double d, double u, double v, double w, double tx, double ty)
        {
            this.u = u;
            this.v = v;
            this.w = w;
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.tx = tx;
            this.ty = ty;
        }

        public static Matrix fromFileOrder(double a, double b, double u, double c, double d, double v, double tx, double ty, double w)
        {
            return new Matrix(a, b, c, d, u, v, w, tx, ty);
        }

        public static Matrix fromByteBuffer(ByteBuffer byteBuffer)
        {
            return fromFileOrder(
                    IsoTypeReader.readFixedPoint1616(byteBuffer),
                    IsoTypeReader.readFixedPoint1616(byteBuffer),
                    IsoTypeReader.readFixedPoint0230(byteBuffer),
                    IsoTypeReader.readFixedPoint1616(byteBuffer),
                    IsoTypeReader.readFixedPoint1616(byteBuffer),
                    IsoTypeReader.readFixedPoint0230(byteBuffer),
                    IsoTypeReader.readFixedPoint1616(byteBuffer),
                    IsoTypeReader.readFixedPoint1616(byteBuffer),
                    IsoTypeReader.readFixedPoint0230(byteBuffer)
            );
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Matrix matrix = (Matrix)o;

            if (matrix.a.CompareTo(a) != 0) return false;
            if (matrix.b.CompareTo(b) != 0) return false;
            if (matrix.c.CompareTo(c) != 0) return false;
            if (matrix.d.CompareTo(d) != 0) return false;
            if (matrix.tx.CompareTo(tx) != 0) return false;
            if (matrix.ty.CompareTo(ty) != 0) return false;
            if (matrix.u.CompareTo(u) != 0) return false;
            if (matrix.v.CompareTo(v) != 0) return false;
            if (matrix.w.CompareTo(w) != 0) return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result;
            long temp;
            temp = BitConverter.DoubleToInt64Bits(u);
            result = (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(v);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(w);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(a);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(b);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(c);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(d);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(tx);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(ty);
            result = 31 * result + (int)(temp ^ (long)((ulong)temp >> 32));
            return result;
        }

        public override string ToString()
        {
            if (Equals(ROTATE_0))
            {
                return "Rotate 0°";
            }
            if (Equals(ROTATE_90))
            {
                return "Rotate 90°";
            }
            if (Equals(ROTATE_180))
            {
                return "Rotate 180°";
            }
            if (Equals(ROTATE_270))
            {
                return "Rotate 270°";
            }
            return "Matrix{" +
                    "u=" + u +
                    ", v=" + v +
                    ", w=" + w +
                    ", a=" + a +
                    ", b=" + b +
                    ", c=" + c +
                    ", d=" + d +
                    ", tx=" + tx +
                    ", ty=" + ty +
                    '}';
        }

        public void getContent(ByteBuffer byteBuffer)
        {
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, a);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, b);
            IsoTypeWriter.writeFixedPoint0230(byteBuffer, u);

            IsoTypeWriter.writeFixedPoint1616(byteBuffer, c);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, d);
            IsoTypeWriter.writeFixedPoint0230(byteBuffer, v);

            IsoTypeWriter.writeFixedPoint1616(byteBuffer, tx);
            IsoTypeWriter.writeFixedPoint1616(byteBuffer, ty);
            IsoTypeWriter.writeFixedPoint0230(byteBuffer, w);
        }
    }
}
