using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualScript.Core.Collections
{
    public interface INotifiedCollectionItem
    {
        void BeforeAdd();

        void AfterAdd();

        void BeforeRemove();

        void AfterRemove();
    }
}
