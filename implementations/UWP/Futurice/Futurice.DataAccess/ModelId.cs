using System;

namespace Futurice.DataAccess
{

    /// <summary>
    /// Identifies a model of a specific type.
    /// </summary>
    public abstract class ModelIdentifier : IEquatable<ModelIdentifier>
    {
        public int Completness { get; }

        public virtual bool Equals(ModelIdentifier other)
        {
            return this == other;
        }
    }

    public class ModelIdentifier<T> : ModelIdentifier
    {
        public override bool Equals(ModelIdentifier other)
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

        public override bool Equals(ModelIdentifier other)
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