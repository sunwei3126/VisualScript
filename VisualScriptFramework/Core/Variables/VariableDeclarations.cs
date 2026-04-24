using System;
using System.Collections;
using System.Collections.Generic;
using VisualScript.Core.Reflection;

namespace VisualScript.Core.Variables
{
    //[SerializationVersion("A")]
    public sealed class VariableDeclarations : IEnumerable<VariableDeclaration>, ISpecifiesCloner
    {
        public VariableDeclarations()
        {
            collection = new VariableDeclarationCollection();
        }

        public VariableKind Kind;

        //[Serialize, InspectorWide(true)]
        private VariableDeclarationCollection collection;

        internal Action OnVariableChanged;

        public object this[string variable]
        {
            get => Get(variable);
            set => Set(variable, value);
        }

        public void Set(string variable, object value)
        {
            if (string.IsNullOrEmpty(variable))
            {
                // Do nothing: safer for e.g. default event configurations,
                // where we don't specify a variable name. If we specified a default,
                // we might override a user value inadvertently.
                return;
            }

            if (collection.TryGetValue(variable, out var declaration))
            {
                if (declaration.Value != value)
                {
                    declaration.Value = value;
                    OnVariableChanged?.Invoke();
                }
            }
            else
            {
                collection.Add(new VariableDeclaration(variable, value));
                OnVariableChanged?.Invoke();
            }
        }

        public object Get(string variable)
        {
            if (string.IsNullOrEmpty(variable))
            {
                throw new ArgumentException("No variable name specified.", nameof(variable));
            }

            if (collection.TryGetValue(variable, out var declaration))
            {
                return declaration.Value;
            }

            throw new InvalidOperationException($"Variable not found: '{variable}'.");
        }

        public T Get<T>(string variable)
        {
            return (T)Get(variable, typeof(T));
        }

        public object Get(string variable, Type expectedType)
        {
            return ConversionUtility.Convert(Get(variable), expectedType);
        }

        public void Clear()
        {
            collection.Clear();
        }

        public bool IsDefined(string variable)
        {
            if (string.IsNullOrEmpty(variable))
            {
                throw new ArgumentException("No variable name specified.", nameof(variable));
            }

            return collection.Contains(variable);
        }

        public VariableDeclaration GetDeclaration(string variable)
        {
            if (collection.TryGetValue(variable, out var declaration))
            {
                return declaration;
            }

            throw new InvalidOperationException($"Variable not found: '{variable}'.");
        }

        public IEnumerator<VariableDeclaration> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)collection).GetEnumerator();
        }

        ICloner ISpecifiesCloner.cloner => VariableDeclarationsCloner.instance;
    }
}
