using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Ensure
{
    public partial class EnsureThat
    {
        public void IsNull<T>(T value)
        {
            if (!Ensure.IsActive)
            {
                return;
            }

            if (value != null)
            {
                throw new ArgumentNullException(paramName, ExceptionMessages.Common_IsNull_Failed);
            }
        }

        public void IsNotNull<T>( T value)
        {
            if (!Ensure.IsActive)
            {
                return;
            }

            if (value == null)
            {
                throw new ArgumentNullException(paramName, ExceptionMessages.Common_IsNotNull_Failed);
            }
        }
    }
}
