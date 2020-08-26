using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICopyFrom<T>
{
    void CopyFrom(T reference);

}