using System;
using System.Collections.Generic;
using System.Linq;
using IoTLogic.Core.Ensure;
using IoTLogic.Flow.Ports;

namespace IoTLogic.Flow.Connections
{
    public sealed class UnitRelation : IUnitRelation
    {
        public UnitRelation(IUnitPort source, IUnitPort destination)
        {
            Ensure.That(nameof(source)).IsNotNull(source);
            Ensure.That(nameof(destination)).IsNotNull(destination);

            if (source.LogicNode != destination.LogicNode)
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
