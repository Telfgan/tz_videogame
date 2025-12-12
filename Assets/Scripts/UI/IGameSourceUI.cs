using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameSourceUI
{
    public void SetSource(int teamIndex, int source);
    public void ShowWinner(int teamIndex);
}
