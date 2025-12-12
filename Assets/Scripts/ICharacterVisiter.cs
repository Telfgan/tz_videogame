using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterVisiter
{
    public void Visit(Character character);
    public void Visit(MinionComponent minion);
}
