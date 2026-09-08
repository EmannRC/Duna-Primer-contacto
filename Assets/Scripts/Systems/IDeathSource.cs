using UnityEngine;
using System;

public interface IDeathSource
{
    event Action OnDeath;
}
