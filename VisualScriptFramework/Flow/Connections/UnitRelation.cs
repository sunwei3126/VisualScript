using System;
using System.Collections.Generic;
using System.Linq;
using VisualScript.Core.Ensure;
using VisualScript.Flow.Ports;

namespace VisualScript.Flow.Connections
{
    public sealed class UnitRelation : IUnitRelation
    {
        public UnitRelation(IUnitPort source, IUnitPort destination)
        {
            Ensure.That(nameof(source)).IsNotNull(source);
            Ensure.That(nameof(destination)).IsNotNull(destination);

            if (source.Unit != destination.Unit)
            {
                throw new NotSupportedException("Cannot create relations across nodes.");
            }

            this.Source = source;
            this.Destination = destination;
        }

        public IUnitPort Source { get; }

        public IUnitPort Destination { get; }
    }
}
