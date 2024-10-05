using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionary
{
}

[Serializable]
public class SerializableDictionary<TKey, TValue> :
    SerializableDictionary,
    IDictionary<TKey, TValue>,
    IReadOnlyDictionary<TKey, TValue>,
    ISerializationCallbackReceiver
{
    // -- TYPES

    [Serializable]
    public struct SerializableKeyValuePair
    {
        public TKey Key;
        public TValue Value;

        public SerializableKeyValuePair(
            TKey key,
            TValue value
            )
        {
            Key = key;
            Value = value;
        }

        public void SetValue(
            TValue value
            )
        {
            Value = value;
        }

        public readonly KeyValuePair<TKey, TValue> ToKeyValuePair()
        {
            return new KeyValuePair<TKey, TValue>( Key, Value );
        }
    }

    // -- FIELDS

    [SerializeField]
    private List<SerializableKeyValuePair> KeyValueList = new List<SerializableKeyValuePair>();

    private Lazy<Dictionary<TKey, uint>> _KeyPositions = null;
    [NonSerialized] private int _version = 0;

    private KeyCollection _keys = null;
    private ValueCollection _values = null;

    // -- PROPERTIES

    private Dictionary<TKey, uint> KeyPositions => _KeyPositions.Value;
    public KeyCollection Keys => _keys ??= new KeyCollection( this );
    public ValueCollection Values => _values ??= new ValueCollection( this );
    public int Count => KeyValueList.Count;
    public bool IsReadOnly => false;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    public TValue this[ TKey key ]
    {
        get
        {
            return KeyValueList[ ( int )KeyPositions[ key ] ].Value;
        }
        set
        {
            if( KeyPositions.TryGetValue( key, out uint index ) )
            {
                var keyValuePair = KeyValueList[ ( int )index ];
                keyValuePair.SetValue( value );
                KeyValueList[ ( int )index ] = keyValuePair;
            }
            else
            {
                KeyPositions[ key ] = ( uint )KeyValueList.Count;
                KeyValueList.Add( new SerializableKeyValuePair( key, value ) );
            }

            _version++;
        }
    }

    // -- METHODS

    public SerializableDictionary()
    {
        _KeyPositions = new Lazy<Dictionary<TKey, uint>>( MakeKeyPositions );
    }

    public SerializableDictionary( IDictionary<TKey, TValue> dictionary )
    {
        _KeyPositions = new Lazy<Dictionary<TKey, uint>>( MakeKeyPositions );

        if( dictionary == null )
        {
            throw new ArgumentException( "The passed dictionary is null." );
        }

        foreach( KeyValuePair<TKey, TValue> pair in dictionary )
        {
            Add( pair.Key, pair.Value );
        }
    }

    private Dictionary<TKey, uint> MakeKeyPositions()
    {
        int entry_count = KeyValueList.Count;

        Dictionary<TKey, uint> result = new Dictionary<TKey, uint>( entry_count );

        for( int entry_index = 0; entry_index < entry_count; ++entry_index )
        {
            result[ KeyValueList[ entry_index ].Key ] = ( uint )entry_index;
        }

        return result;
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        // After deserialization, the key positions might be changed
        _KeyPositions = new Lazy<Dictionary<TKey, uint>>( MakeKeyPositions );
    }

    public bool ContainsValue(
        TValue value
        )
    {
        if( value == null )
        {
            return KeyValueList.Exists( key_value_pair => key_value_pair.Value == null );
        }

        return KeyValueList.Exists( key_value_pair => EqualityComparer<TValue>.Default.Equals( key_value_pair.Value, value ) );
    }

    public void Add(
        TKey key,
        TValue value
        )
    {
        if( KeyPositions.ContainsKey( key ) )
        {
            throw new ArgumentException( "An element with the same key already exists in the dictionary." );
        }
        else
        {
            KeyPositions[ key ] = ( uint )KeyValueList.Count;

            KeyValueList.Add( new SerializableKeyValuePair( key, value ) );
            _version++;
        }
    }

    public bool ContainsKey(
        TKey key
        )
    {
        return KeyPositions.ContainsKey( key );
    }

    public bool Remove(
        TKey key
        )
    {
        if( KeyPositions.TryGetValue( key, out uint index ) )
        {
            Dictionary<TKey, uint> key_positions = KeyPositions;

            key_positions.Remove( key );

            KeyValueList.RemoveAt( ( int )index );

            int entry_count = KeyValueList.Count;

            for( uint entry_index = index; entry_index < entry_count; entry_index++ )
            {
                key_positions[ KeyValueList[ ( int )entry_index ].Key ] = entry_index;
            }

            _version++;

            return true;
        }

        return false;
    }

    public bool TryGetValue(
        TKey key,
        out TValue value
        )
    {
        if( KeyPositions.TryGetValue( key, out uint index ) )
        {
            value = KeyValueList[ ( int )index ].Value;

            return true;
        }

        value = default;

        return false;
    }

    public void Add(
        KeyValuePair<TKey, TValue> key_value_pair
        )
    {
        Add( key_value_pair.Key, key_value_pair.Value );
        _version++;
    }

    public bool Contains(
        KeyValuePair<TKey, TValue> key_value_pair
        )
    {
        return KeyPositions.ContainsKey( key_value_pair.Key );
    }

    public bool Remove(
        KeyValuePair<TKey, TValue> key_value_pair
        )
    {
        return Remove( key_value_pair.Key );
    }

    public void Clear()
    {
        KeyValueList.Clear();
        KeyPositions.Clear();
        _version++;
    }

    public void CopyTo(
        KeyValuePair<TKey, TValue>[] array,
        int array_index
        )
    {
        int keys_count = KeyValueList.Count;

        if( array.Length - array_index < keys_count )
        {
            throw new ArgumentException( "arrayIndex" );
        }

        for( int i = 0; i < keys_count; ++i, ++array_index )
        {
            SerializableKeyValuePair entry = KeyValueList[ i ];

            array[ array_index ] = new KeyValuePair<TKey, TValue>( entry.Key, entry.Value );
        }
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator( this );
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // -- TYPES

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
    {
        // -- FIELDS

        private readonly SerializableDictionary<TKey, TValue> _dictionary;
        private readonly int _version;

        private int _currentIndex;
        private KeyValuePair<TKey, TValue> _current;

        // -- PROPERTIES

        public readonly KeyValuePair<TKey, TValue> Current => _current;

        readonly DictionaryEntry IDictionaryEnumerator.Entry
        {
            get
            {
                if( !IsValidIndex( _currentIndex ) )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_CANNOT_HAPPEN );
                }

                return new DictionaryEntry( _current.Key, _current.Value );
            }
        }

        readonly object IDictionaryEnumerator.Key
        {
            get
            {
                if( !IsValidIndex( _currentIndex ) )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_CANNOT_HAPPEN );
                }

                return _current.Key;
            }
        }

        readonly object IDictionaryEnumerator.Value
        {
            get
            {
                if( !IsValidIndex( _currentIndex ) )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_CANNOT_HAPPEN );
                }

                return _current.Value;
            }
        }

        readonly object IEnumerator.Current
        {
            get
            {
                if( !IsValidIndex( _currentIndex ) )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_CANNOT_HAPPEN );
                }

                return new KeyValuePair<TKey, TValue>( _current.Key, _current.Value );
            }
        }

        // -- CONSTRUCTORS

        internal Enumerator(
            SerializableDictionary<TKey, TValue> dictionary
            )
        {
            _dictionary = dictionary;
            _version = dictionary._version;
            _currentIndex = 0;
            _current = new KeyValuePair<TKey, TValue>();
        }

        // -- METHODS

        public bool MoveNext()
        {
            if( _version != _dictionary._version )
            {
                throw new InvalidOperationException( "Collection was modified; enumeration operation may not execute" );
            }

            while( _currentIndex < _dictionary.KeyValueList.Count )
            {
                _current = _dictionary.KeyValueList[ _currentIndex ].ToKeyValuePair();
                _currentIndex++;

                return true;
            }

            _currentIndex = _dictionary.Count + 1;
            _current = new KeyValuePair<TKey, TValue>();

            return false;
        }

        public readonly void Dispose()
        {
        }

        void IEnumerator.Reset()
        {
            if( _version != _dictionary._version )
            {
                throw new InvalidOperationException( ThrowUtils.ENUMERATION_COLLECTION_MODIFIED );
            }

            _currentIndex = 0;
            _current = new KeyValuePair<TKey, TValue>();
        }

        private readonly bool IsValidIndex(
            int index
            )
        {
            return index >= 0
                || index < _dictionary.Count;
        }
    }

    public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
    {
        // -- FIELDS

        private readonly SerializableDictionary<TKey, TValue> _dictionary = null;

        // -- PROPERTIES

        public int Count => _dictionary.Count;

        bool ICollection<TKey>.IsReadOnly => true;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => ( ( ICollection )_dictionary ).SyncRoot;

        // -- CONSTRUCTORS

        public KeyCollection(
            SerializableDictionary<TKey, TValue> dictionary
            )
        {
            ThrowUtils.ThrowIfArgumentNull( nameof( dictionary ), dictionary );

            _dictionary = dictionary;
        }

        // -- METHODS

        public Enumerator GetEnumerator()
        {
            return new Enumerator( _dictionary );
        }

        public void CopyTo(
            TKey[] array,
            int index
            )
        {
            ThrowUtils.ThrowIfArgumentNull( nameof( array ), array );
            ThrowUtils.ThrowIfArgumentOutOfRange( nameof( index ), index, array.Length );

            if( array.Length - index < _dictionary.Count )
            {
                throw new ArgumentException( "Array is too small for copy" );
            }

            int count = _dictionary.Count;

            for( int i = 0; i < count; i++ )
            {
                array[ index++ ] = _dictionary.KeyValueList[ i ].Key;
            }
        }

        public bool Contains(
            TKey item
            )
        {
            return _dictionary.ContainsKey( item );
        }

        void ICollection<TKey>.Add(
            TKey item
            )
        {
            throw new NotSupportedException();
        }

        void ICollection<TKey>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<TKey>.Remove(
            TKey item
            )
        {
            throw new NotSupportedException();
        }

        IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
        {
            return new Enumerator( _dictionary );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator( _dictionary );
        }

        void ICollection.CopyTo(
            Array array,
            int index
            )
        {
            ThrowUtils.ThrowIfArgumentNull( nameof( array ), array );

            if( array.Rank != 1 )
            {
                throw new ArgumentException( "Array must not be multi-dimensionnal." );
            }

            ThrowUtils.ThrowIfArgumentOutOfRange( nameof( index ), index, array.Length );

            if( array.Length - index < _dictionary.Count )
            {
                throw new ArgumentException( "Array is too small for copy" );
            }

            if( array is TKey[] keys )
            {
                CopyTo( keys, index );
            }
            else
            {
                if( array is not object[] objects )
                {
                    throw new ArgumentException( "Invalid array type" );
                }

                int count = _dictionary.Count;

                try
                {

                    for( int i = 0; i < count; i++ )
                    {
                        objects[ index++ ] = _dictionary.KeyValueList[ i ].Key;
                    }
                }
                catch( ArrayTypeMismatchException )
                {
                    throw new ArgumentException( "Invalid array type" );
                }
            }
        }

        [Serializable]
        public struct Enumerator : IEnumerator<TKey>, IEnumerator
        {
            // -- FIELDS

            private readonly SerializableDictionary<TKey, TValue> _dictionary;
            private readonly int _version;

            private int _currentIndex;
            private TKey _currentKey;

            // -- PROPERTIES

            public readonly TKey Current => _currentKey;

            readonly object IEnumerator.Current
            {
                get
                {
                    if( _currentIndex == 0
                        || _currentIndex == _dictionary.Count + 1
                        )
                    {
                        throw new InvalidOperationException( ThrowUtils.ENUMERATION_CANNOT_HAPPEN );
                    }

                    return _currentKey;
                }
            }

            void IEnumerator.Reset()
            {
                if( _version != _dictionary._version )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_COLLECTION_MODIFIED );
                }

                _currentIndex = 0;
                _currentKey = default;
            }

            // -- CONSTRUCTORS

            internal Enumerator(
                SerializableDictionary<TKey, TValue> dictionary
                )
            {
                _dictionary = dictionary;
                _version = dictionary._version;
                _currentIndex = 0;
                _currentKey = default;
            }

            // -- METHODS

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                if( _version != _dictionary._version )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_COLLECTION_MODIFIED );
                }

                while( _currentIndex < _dictionary.KeyValueList.Count )
                {
                    _currentKey = _dictionary.KeyValueList[ _currentIndex ].Key;
                    _currentIndex++;

                    return true;
                }

                _currentIndex = _dictionary.Count + 1;
                _currentKey = default;

                return false;
            }
        }
    }

    public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
    {
        // -- FIELDS

        private readonly SerializableDictionary<TKey, TValue> _dictionary = null;

        // -- PROPERTIES

        public int Count => _dictionary.Count;

        bool ICollection<TValue>.IsReadOnly => true;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => ( ( ICollection )_dictionary ).SyncRoot;

        // -- CONSTRUCTORS

        public ValueCollection(
            SerializableDictionary<TKey, TValue> dictionary
            )
        {
            ThrowUtils.ThrowIfArgumentNull( nameof( dictionary ), dictionary );

            _dictionary = dictionary;
        }

        // -- METHODS

        public Enumerator GetEnumerator()
        {
            return new Enumerator( _dictionary );
        }

        public void CopyTo(
            TValue[] array,
            int index
            )
        {
            ThrowUtils.ThrowIfArgumentNull( nameof( array ), array );
            ThrowUtils.ThrowIfArgumentOutOfRange( nameof( index ), index, array.Length );

            if( array.Length - index < _dictionary.Count )
            {
                throw new ArgumentException( "Array is too small for copy" );
            }

            int count = _dictionary.Count;

            for( int i = 0; i < count; i++ )
            {
                array[ index++ ] = _dictionary.KeyValueList[ i ].Value;
            }
        }

        public bool Contains(
            TValue item
            )
        {
            return _dictionary.ContainsValue( item );
        }

        void ICollection<TValue>.Add(
            TValue item
            )
        {
            throw new NotSupportedException();
        }

        void ICollection<TValue>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<TValue>.Remove(
            TValue item
            )
        {
            throw new NotSupportedException();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return new Enumerator( _dictionary );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator( _dictionary );
        }

        void ICollection.CopyTo(
            Array array,
            int index
            )
        {
            ThrowUtils.ThrowIfArgumentNull( nameof( array ), array );

            if( array.Rank != 1 )
            {
                throw new ArgumentException( "Array must not be multi-dimensionnal." );
            }

            ThrowUtils.ThrowIfArgumentOutOfRange( nameof( index ), index, array.Length );

            if( array.Length - index < _dictionary.Count )
            {
                throw new ArgumentException( "Array is too small for copy" );
            }

            if( array is TValue[] values )
            {
                CopyTo( values, index );
            }
            else
            {
                if( array is not object[] objects )
                {
                    throw new ArgumentException( "Invalid array type" );
                }

                int count = _dictionary.Count;

                try
                {
                    for( int i = 0; i < count; i++ )
                    {
                        objects[ index++ ] = _dictionary.KeyValueList[ i ].Key;
                    }
                }
                catch( ArrayTypeMismatchException )
                {
                    throw new ArgumentException( "Invalid array type" );
                }
            }
        }

        [Serializable]
        public struct Enumerator : IEnumerator<TValue>, IEnumerator
        {
            // -- FIELDS

            private readonly SerializableDictionary<TKey, TValue> _dictionary;
            private readonly int _version;

            private int _currentIndex;
            private TValue _currentKey;

            // -- PROPERTIES

            public readonly TValue Current => _currentKey;

            readonly object IEnumerator.Current
            {
                get
                {
                    if( _currentIndex == 0
                        || _currentIndex == _dictionary.Count + 1
                        )
                    {
                        throw new InvalidOperationException( ThrowUtils.ENUMERATION_CANNOT_HAPPEN );
                    }

                    return _currentKey;
                }
            }

            void IEnumerator.Reset()
            {
                if( _version != _dictionary._version )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_COLLECTION_MODIFIED );
                }

                _currentIndex = 0;
                _currentKey = default;
            }

            // -- CONSTRUCTORS

            internal Enumerator(
                SerializableDictionary<TKey, TValue> dictionary
                )
            {
                _dictionary = dictionary;
                _version = dictionary._version;
                _currentIndex = 0;
                _currentKey = default;
            }

            // -- METHODS

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                if( _version != _dictionary._version )
                {
                    throw new InvalidOperationException( ThrowUtils.ENUMERATION_COLLECTION_MODIFIED );
                }

                while( _currentIndex < _dictionary.KeyValueList.Count )
                {
                    _currentKey = _dictionary.KeyValueList[ _currentIndex ].Value;
                    _currentIndex++;

                    return true;
                }

                _currentIndex = _dictionary.Count + 1;
                _currentKey = default;

                return false;
            }
        }
    }
}
