using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualScript.Core.Collections;

namespace VisualScript.Core.Variables
{
    //[SerializationVersion("A")]
    public sealed class VariableDeclarationCollection : KeyedCollection<string, VariableDeclaration>, IKeyedCollection<string, VariableDeclaration>
    {
        protected override string GetKeyForItem(VariableDeclaration item)
        {
            return item.Name;
        }

        public void EditorRename(VariableDeclaration item, string newName)
        {
            ChangeItemKey(item, newName);
        }

        public bool TryGetValue(string key, out VariableDeclaration value)
        {
            if (Dictionary == null)
            {
                value = default;
                return false;
            }

            return Dictionary.TryGetValue(key, out value);
        }
    }
}
