using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using GestureDesigner.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RoslynPad.Roslyn;

namespace GestureDesigner;

internal class MainWindowViewModel(RoslynHost host)
{
    private RoslynHost Host { get; } = host;
    
    internal PathGeometry Geometry { get; private set; } = CreateWaitingPath();

    internal GestureDrawer? Drawer { get; private set; }

    #region DefaultPath

    private static PathGeometry CreateWaitingPath()
    {
        var gd = new GestureDrawer()
            .DrawLine(4, 90)
            .DrawLine(1, 0)
            .DrawArcRelative(1, 90, -180)
            .Forward(1);

        return gd.ToPathGeometry();
    }

    #endregion

    #region Script Execution

    private Script? _script;
    private object? Result { get; set; }

    internal async Task<bool> TrySubmitAsync(string text)
    {
        _script = CSharpScript.Create(text, ScriptOptions.Default
            .WithReferences(Host.DefaultReferences)
            .WithImports(Host.DefaultImports));
        
        var diagnostics = _script.Compile();
        if (diagnostics.Any(t => t.Severity == DiagnosticSeverity.Error)) return false;
        

        await ExecuteAsync().ConfigureAwait(true);

        return true;
    }

    private async Task ExecuteAsync()
    {
        var script = _script;
        if (script == null) return;

        try
        {
            var output = await script.RunAsync().ConfigureAwait(true);

            if (output.Exception == null)
            {
                Result = output.ReturnValue;
                Drawer = Result as GestureDrawer;
                Geometry = Drawer?.ToPathGeometry() ?? Geometry;
            }
        }
        catch
        {
            //ignore
        }
    }

    #endregion
}