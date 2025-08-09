using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GestureDesigner.Design;

/// <summary>
/// 手势绘制的配置选项
/// </summary>
public record Options
{
    /// <summary>默认配置（向右，向上）</summary>
    public static readonly Options Default = new();

    /// <summary>Windows 坐标系配置（向下为正方向）</summary>
    public static readonly Options Windows = new()
    {
        VerticalDirection = VerticalDirection.Down
    };
    /// <summary>X轴正方向（默认向右）</summary>
    public HorizontalDirection HorizontalDirection { get; set; } = HorizontalDirection.Right;
    /// <summary>Y轴正方向（默认向上）</summary>
    public VerticalDirection VerticalDirection { get; private set; } = VerticalDirection.Up;
    /// <summary>计算精度（小数位数，默认4位）</summary>
    public int Precision { get; set; } = 4;
}

/// <summary>
/// 手势绘制器，用于生成基于数学坐标的几何路径（支持直线、圆弧等）
/// </summary>
public class GestureDrawer
{
    private readonly List<MathPoint> _internalPath = [];
    private readonly Options _options = Options.Default;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">绘制配置（默认使用 <see cref="Options.Default"/>）</param>
    public GestureDrawer(Options? options = null)
    {
        _options = options ?? _options;
        MathAngle.HorizontalDirection = _options.HorizontalDirection;
        MathPoint.Precision = _options.Precision;
        _internalPath.Add(_location);
    }

    private MathPoint _location = MathPoint.Zero;

    private MathAngle _direction = MathAngle.Zero;

    internal IReadOnlyList<Point> DrawingPath => ConvertToPath(_internalPath);

    /// <summary>
    /// 设置当前方向角度（根据配置自动转换单位）
    /// </summary>
    /// <param name="angle">角度</param>
    private void SetDirection(MathAngle angle)
    {
        _direction = angle;
    }

    /// <summary>
    /// 向前绘制指定长度的直线（沿当前方向）
    /// </summary>
    /// <param name="length">长度</param>
    public GestureDrawer Forward(double length)
    {
        var endPoint = new MathPoint(_direction, length, _location);
        _internalPath.AddRange([_location, endPoint]);
        _location = endPoint;
        return this;
    }

    /// <summary>
    /// 绘制指定长度和角度的直线
    /// </summary>
    /// <param name="length">长度</param>
    /// <param name="angle">角度</param>
    public GestureDrawer DrawLine(double length, MathAngle angle)
    {
        SetDirection(angle);
        return Forward(length);
    }

    /// <summary>
    /// 旋转当前方向（叠加角度）
    /// </summary>
    /// <param name="angle">旋转角度</param>
    public GestureDrawer Rotate(MathAngle angle)
    {
        SetDirection(_direction + angle);
        return this;
    }

    /// <summary>
    /// 绘制圆弧（自动偏移坐标，相对当前角度）
    /// </summary>
    /// <param name="radius">半径</param>
    /// <param name="relativeStartAngle">相对当前角度的偏移量</param>
    /// <param name="sweepAngle">扫过角度（可为负，表示反方向旋转）</param>
    public GestureDrawer DrawArcRelative(double radius, MathAngle relativeStartAngle, MathAngle sweepAngle)
    {
        SetDirection(_direction + relativeStartAngle);
        return DrawArc(radius, _direction, sweepAngle);
    }
    
    /// <summary>
    /// 绘制圆弧（自动偏移坐标）
    /// </summary>
    /// <param name="radius">半径</param>
    /// <param name="startAngle">起始角度</param>
    /// <param name="sweepAngle">扫过角度（可为负，表示反方向旋转）</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public GestureDrawer DrawArc(double radius, MathAngle startAngle, MathAngle sweepAngle)
    {
        MathPoint firstPoint = new (startAngle, radius);
        return DrawArc(-firstPoint.X, -firstPoint.Y, radius, startAngle, sweepAngle);
    }
    
    private GestureDrawer DrawArc(double relativeX, double relativeY, double radius, MathAngle startAngle,
        MathAngle sweepAngle)
    {
        return DrawArcAbsolutely(_location.X + relativeX,
            _location.Y + relativeY,
            radius, startAngle, sweepAngle);
    }
    
    private GestureDrawer DrawArcAbsolutely(double centerX, double centerY, double radius, MathAngle startAngle,
        MathAngle sweepAngle)
    {
        if (sweepAngle == MathAngle.Zero) return this;
        
        var internalCenter = new MathPoint(centerX, centerY);
        var internalEndAngle = startAngle + sweepAngle;
        // var steps = Max(10, (int)sweepAngle.Angle);
        const int steps = 180;

        for (var i = 0; i <= steps; i++)
        {
            var currentInternalAngle = startAngle + i * (sweepAngle / steps);
            var pointOnArc = new MathPoint(currentInternalAngle, radius, internalCenter);
            _internalPath.Add(pointOnArc);
        }

        _location = new MathPoint(internalEndAngle, radius, internalCenter);
        _direction = internalEndAngle + (sweepAngle > 0 ? 90 : -90);

        return this;
    }

    internal PathGeometry ToPathGeometry()
    {
        var path = DrawingPath;
        if (path.Count == 0)
            return new PathGeometry();

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = path[0] };

        for (var i = 1; i < path.Count; i++)
            pathFigure.Segments.Add(new LineSegment(path[i], true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    private IReadOnlyList<Point> ConvertToPath(List<MathPoint> internalPath)
    {
        var xSign = _options.HorizontalDirection == HorizontalDirection.Right ? 1 : -1;
        var ySign = _options.VerticalDirection == VerticalDirection.Down ? 1 : -1;

        return internalPath.Select(p => new Point(p.X * xSign, p.Y * ySign)).ToList().AsReadOnly();
    }
}