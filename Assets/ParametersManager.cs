using System.Collections.Generic;

public class ParametersManager : Singleton<ParametersManager>
{
    public Dictionary<ParameterData, float> CurrentParameters;
}
