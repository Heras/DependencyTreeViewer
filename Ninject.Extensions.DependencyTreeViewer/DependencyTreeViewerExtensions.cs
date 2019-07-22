namespace Ninject.Extensions.DependencyTreeViewer
{
    using System.Diagnostics;
    using System.Linq;

    using Ninject.Parameters;
    using Ninject.Syntax;

    public static class DependencyTreeViewerExtensions
    {
        public static void GenerateDependencyGraphFor(this IKernel kernel, System.Type contract, int depth = 0)
        {
            Debug.WriteLine($"{new string('\t', depth)}{contract.Name}");

            typeof(ResolutionExtensions)
                .GetMethod("TryGet", new[] { typeof(IResolutionRoot), typeof(IParameter[]) })
                .MakeGenericMethod(contract)
                .Invoke(null, new object[] { kernel, new IParameter[] { } })
                ?
                .GetType()
                .GetConstructors()
                .ToList()
                .ForEach(ctor =>
                {
                    Debug.WriteLine($"{new string('\t', depth)}{ctor.DeclaringType.Name}");

                    ctor.GetParameters()
                        .ToList()
                        .ForEach(p => kernel.GenerateDependencyGraphFor(p.ParameterType, depth + 1));
                });
        }
    }
}
