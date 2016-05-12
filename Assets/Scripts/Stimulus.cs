using UnityEngine;
using System.Collections;

public enum StimType
{
    NONE,
    Arrow
}
public class Stimulus  {

    public StimType stimType;
    public float strength;

    public Stimulus (StimType _stimType, float _strength)
    {
        stimType = _stimType;
        strength = _strength;
    }
}
