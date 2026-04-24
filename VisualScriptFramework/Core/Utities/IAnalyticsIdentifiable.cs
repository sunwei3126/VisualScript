using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTLogic.Core.Utities
{
    public interface IAnalyticsIdentifiable
    {
        AnalyticsIdentifier GetAnalyticsIdentifier();
    }

    public class AnalyticsIdentifier
    {
        public string Identifier;
        public string Namespace;
        public int Hashcode;
    }
}
