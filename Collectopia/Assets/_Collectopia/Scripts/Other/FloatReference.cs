using System;

[Serializable]
public class FloatReference
{
    public bool useConstant;
    public float constantValue;
    public FloatVariableSO variable; 
    public float Value
    {
        get { return useConstant ? constantValue : variable.value; }
    }
}
