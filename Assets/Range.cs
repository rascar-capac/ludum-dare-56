using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public abstract class Range<T>
{
    [SerializeField] protected T _MinimumValue;
    [SerializeField] protected T _MaximumValue;

    public T MinimumValue
    {
        get { return _MinimumValue; }
        protected set { _MinimumValue = value; }
    }

    public T MaximumValue
    {
        get { return _MaximumValue; }
        protected set { _MaximumValue = value; }
    }

    public abstract T Amplitude{  get; }
    public abstract T Center{ get; }

    public abstract T GetRandomValue();
    public abstract bool Contains( T value_to_check );
}

[System.Serializable]
public class IntegerRange : Range<int>
{
    public override int Amplitude{ get{ return _MaximumValue - _MinimumValue; } }
    public override int Center { get { return ( _MaximumValue + _MinimumValue ) / 2; } }

    public IntegerRange(
        int minimum_value,
        int maximum_value
        )
    {
        MinimumValue = minimum_value;
        MaximumValue = maximum_value;
    }

    public override int GetRandomValue()
    {
        return UnityEngine.Random.Range( MinimumValue, MaximumValue );
    }

    public override bool Contains(
        int value_to_check
        )
    {
        return MinimumValue <= value_to_check
            && value_to_check < MaximumValue;
    }

    public int Clamp(
        int input
        )
    {
        return Mathf.Clamp(input, MinimumValue, MaximumValue);
    }
}

[System.Serializable]
public class FloatRange : Range<float>
{
    public override float Amplitude { get { return _MaximumValue - _MinimumValue; } }
    public override float Center { get { return ( _MaximumValue + _MinimumValue ) * 0.5f; } }

    public FloatRange(
        float minimum_value,
        float maximum_value
        )
    {
        MinimumValue = minimum_value;
        MaximumValue = maximum_value;
    }

    public override float GetRandomValue()
    {
        return UnityEngine.Random.Range( MinimumValue, MaximumValue );
    }

    public override bool Contains(
        float value_to_check
        )
    {
        return MinimumValue <= value_to_check
            && value_to_check <= MaximumValue;
    }

    public float RemapFrom(
        float input,
        float minimum_value = 0,
        float maximum_value = 1,
        bool must_clamp = false
        )
    {
        float output = math.remap( minimum_value, maximum_value, _MinimumValue, _MaximumValue, input );

        if( must_clamp )
        {
            output = Mathf.Clamp( output, _MinimumValue, _MaximumValue );
        }

        return output;
    }

    public float RemapTo(
        float input,
        float minimum_value = 0,
        float maximum_value = 1,
        bool must_clamp = false
        )
    {
        float output = math.remap( _MinimumValue, _MaximumValue, minimum_value, maximum_value, input );

        if( must_clamp )
        {
            output = Mathf.Clamp( output, minimum_value, maximum_value );
        }

        return output;
    }

    public float Clamp(
        float input
        )
    {
        return Mathf.Clamp(input, MinimumValue, MaximumValue);
    }
}

[System.Serializable]
public class Vector3Range : Range<Vector3>
{
    public override Vector3 Amplitude { get { return _MaximumValue - _MinimumValue; } }
    public override Vector3 Center { get { return ( _MaximumValue + _MinimumValue ) * 0.5f; } }

    public Vector3Range(
        Vector3 minimum_value,
        Vector3 maximum_value
        )
    {
        MinimumValue = minimum_value;
        MaximumValue = maximum_value;
    }

    public override Vector3 GetRandomValue()
    {
        return new Vector3( UnityEngine.Random.Range( MinimumValue.x, MaximumValue.x ), UnityEngine.Random.Range( MinimumValue.y, MaximumValue.y ), UnityEngine.Random.Range( MinimumValue.z, MaximumValue.z ) );
    }

    public override bool Contains(
        Vector3 value_to_check
        )
    {
        return MinimumValue.x <= value_to_check.x
            && value_to_check.x <= MaximumValue.x
            && MinimumValue.y <= value_to_check.y
            && value_to_check.y <= MaximumValue.y
            && MinimumValue.z <= value_to_check.z
            && value_to_check.z <= MaximumValue.z;
    }
}
