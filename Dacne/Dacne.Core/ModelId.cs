using System;

namespace Dacne.Core
{

    /// <summary>
    /// Identifies a model of a specific type.
    /// </summary>
    public abstract class ModelIdentifierBase : IEquatable<ModelIdentifierBase>
    {
        public int Completness { get; }

        public virtual bool Equals(ModelIdentifierBase other)
        {
            return this == other;
        }
    }

    public class ModelIdentifier<T> : ModelIdentifierBase
    {
        public override bool Equals(ModelIdentifierBase other)
        {
            if (other == null)
            {
                return false;
            }

            return base.Equals(other) || typeof(ModelIdentifier<T>) == other.GetType();
        }
    }

    public class KeyedModelIdentifier<T, TKey> : ModelIdentifier<T>
    {
        public KeyedModelIdentifier(TKey key)
        {
            Key = key;
        }

        public TKey Key { get; private set; }

        public override bool Equals(ModelIdentifierBase other)
        {
            return this == other || (other as KeyedModelIdentifier<T, TKey>)?.Key.Equals(Key) == true;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }

    public class SimpleModelIdentifier<T> : KeyedModelIdentifier<T, string>
    {
        public SimpleModelIdentifier(string key) : base(key) { }
    }
}