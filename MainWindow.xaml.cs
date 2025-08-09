using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GestureDesigner.Design;
using HandyControl.Controls;
using RoslynPad.Editor;
using RoslynPad.Roslyn;

namespace GestureDesigner;

/// <summary>
/// 主窗口
/// </summary>
public partial class MainWindow
{
    private readonly IEnumerable<Assembly>? _additionalAssemblies =
    [
        Assembly.Load("RoslynPad.Roslyn.Windows"),
        Assembly.Load("RoslynPad.Editor.Windows"),
        Assembly.Load("GestureDesigner")
    ];

    private readonly ImmutableArray<string>? _disabledDiagnostics = null;

    private readonly RoslynHostReferences? _references =
        RoslynHostReferences.NamespaceDefault.With(assemblyReferences:
        [
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            typeof(GestureDrawer).Assembly,
            typeof(Regex).Assembly
        ]);

    private AnimationPath? _path;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        Host = new RoslynHost(_additionalAssemblies, _references, _disabledDiagnostics);
        ViewModel = new MainWindowViewModel(Host);
        DataContext = ViewModel;
    }

    private MainWindowViewModel ViewModel { get; }
    private RoslynHost Host { get; }
    private SolidColorBrush StrokeBursh { get; } = (SolidColorBrush) new BrushConverter().ConvertFrom("#1E90FF")!;

    private void CodeEditor_OnLoaded(object sender, RoutedEventArgs e)
    {
        CodeEditor.Initialize(Host, new ClassificationHighlightColors(), Directory.GetCurrentDirectory(), string.Empty);
        CodeEditor.Text =
            """
            using GestureDesigner.Design;
            using static System.Math;
            
            return new GestureDrawer()
                
                ;
            
            /* 例：手势-P
            return new GestureDrawer()
                .DrawLine(4, 90)
                .DrawLine(1, 0)
                .DrawArcRelative(1, 90, -180)
                .Forward(1);
            */
            """;
        CodeEditor.Focus();
        CodeEditor.CaretOffset = 92;
        AddOrUpdateAnimationPath();
    }

    private async void CodeEditor_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F5 when await ViewModel.TrySubmitAsync(CodeEditor.Text).ConfigureAwait(true):
                AddOrUpdateAnimationPath();
                break;
            case Key.F4 when ViewModel.Drawer != null:
                var json = "[" +
                                    string.Join(",", ViewModel.Drawer.DrawingPath.Select(p => $"\"{p.X},{p.Y}\""))
                                + "]";
                const string path = "temp.json";
                File.WriteAllText(path, json);
                break;
        }
    }

    private void AddOrUpdateAnimationPath()
    {
        if (_path != null) MainGrid.Children.Remove(_path);
        _path = null;

        _path = new AnimationPath
        {
            Data = ViewModel.Geometry,
            Stroke = StrokeBursh,
            Stretch = Stretch.Uniform,
            StrokeThickness = 2,
            Margin = new Thickness(100)
        };
        Grid.SetColumn(_path, 1);
        MainGrid.Children.Add(_path);
    }
}