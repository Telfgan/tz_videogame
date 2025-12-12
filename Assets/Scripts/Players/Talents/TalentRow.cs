using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TalentRow
{
    public List<Talent> Talents;

    public bool isOpen;

    //public List<Talent> Talents => _talents;
    //public bool isOpen => _isOpen;

}
