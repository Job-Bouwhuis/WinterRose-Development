using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.SourceGeneration
{
    /// <summary>
    /// An interface that defines a source generator.
    /// </summary>
    public interface ICodeGenerator
    {
        void Initialize(SourceContext context);
        void Generate(SourceContext context);

    }
}
