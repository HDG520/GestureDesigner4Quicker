#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
using static System.Math;

namespace GestureDesigner.Design;

internal record MathPoint
{
    public static readonly MathPoint Zero = new(x:0, y:0);
    public MathPoint(double x, double y)
    {
        X = Round(x, Precision);
        Y = Round(y, Precision);
    }
    public MathPoint(MathAngle angle, double radius) : this(angle, radius, Zero) {}
    public MathPoint(MathAngle angle, double radius, MathPoint o)
    {
        X = Round(Cos(angle.Radians) * radius + o.X, Precision);
        Y = Round(Sin(angle.Radians) * radius + o.Y, Precision);
    }

    internal static int Precision { private get; set; } = 3;
    public double X { get; }
    public double Y { get; }
}

public record MathAngle
{
    public const double RadiansRate = PI / 180;
    internal static HorizontalDirection HorizontalDirection { private get; set; } = HorizontalDirection.Right;
    
    public static readonly MathAngle Zero = 0;
    
    private readonly double _angle;
    public double Angle => (_angle % 360 + 360) % 360;

    public double Radians => Angle * RadiansRate;
    
    private MathAngle(double angle)
    {
        if (HorizontalDirection == HorizontalDirection.Left) angle += 180;
        _angle = angle;
    }
    
    #region 运算符重载
    
    public static MathAngle operator +(MathAngle a, MathAngle b) => new(a._angle + b._angle);
    public static MathAngle operator -(MathAngle a, MathAngle b) => new(a._angle - b._angle);
    public static MathAngle operator -(MathAngle a) => new(-a._angle);
    public static bool operator >(MathAngle a, MathAngle b) => a._angle > b._angle;
    public static bool operator <(MathAngle a, MathAngle b) => a._angle < b._angle;
    
    public static MathAngle operator *(MathAngle a, double b) => new(a._angle * b);
    public static MathAngle operator *(double a, MathAngle b) => b * a;
    public static MathAngle operator *(MathAngle a, int b) => new(a._angle * b);
    public static MathAngle operator *(int a, MathAngle b) => b * a;

    public static MathAngle operator /(MathAngle a, double b) => new(a._angle / b);
    public static MathAngle operator /(double a, MathAngle b) => b/a;
    public static MathAngle operator /(MathAngle a, int b) => new(a._angle / b);
    public static MathAngle operator /(int a, MathAngle b) => b/a;
    
    #endregion
    
    #region 隐式转换
    
    public static implicit operator MathAngle(double angle) => new(angle);
    public static implicit operator double(MathAngle angle) => angle.Angle;
    
    #endregion
}