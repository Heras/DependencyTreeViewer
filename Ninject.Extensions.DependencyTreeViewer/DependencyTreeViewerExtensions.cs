namespace Ninject.Extensions.DependencyTreeViewer
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Ninject.Parameters;
    using Ninject.Syntax;

    public static class DependencyTreeViewerExtensions
    {
        [Conditional("DEBUG")]
        public static void GenerateDependencyGraphFor<T>(this IKernel kernel, int depth = 0)
        {
            typeof(ResolutionExtensions)
                .DebugWrite(t => Environment.NewLine + depth.Tab() + typeof(T).Name)
                .GetMethod("TryGet", new[] { typeof(IResolutionRoot), typeof(IParameter[]) })
                .MakeGenericMethod(typeof(T))
                .Invoke(null, new object[] { kernel, new IParameter[] { } })
                ?
                .GetType()
                .DebugWrite(t => ": " + t.Name)
                .GetConstructors()
                .Select(c => c.GetParameters())
                .OrderByDescending(c => c.Count())
                .First()
                .DebugWrite(c => c
                    .Select(p => p.ParameterType.Name)
                    .DefaultIfEmpty("")
                    .Aggregate((a, b) => a + ", " + b)
                    .Wrap("(", ")"))
                .ToList()
                .ForEach(p =>
                    typeof(DependencyTreeViewerExtensions)
                        .GetMethod("GenerateDependencyGraphFor", new[] { typeof(IKernel), typeof(int) })
                        .MakeGenericMethod(p.ParameterType)
                        .Invoke(null, new object[] { kernel, depth + 1 }));
        }
        public static string Wrap(this string s, string before, string after) => before + s + after;

        public static string Tab(this int repeat) => new string('\t', repeat);

        public static T DebugWrite<T>(this T @object, Func<T, string> function)
        {
            var result = function.Invoke(@object);
            Debug.Write(result);
            return @object;
        }
    }
}
