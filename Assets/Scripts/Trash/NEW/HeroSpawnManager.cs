using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HeroSpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnArea> _spawnAreas;

    public Vector3 GetRandomPoint(int index)
    {
        return _spawnAreas[index].GetRandomPoint();
    }

    public Vector3 GetPoint(int index)
    {
        return _spawnAreas[index].GetPoint();
    }

    public Quaternion GetRotate(int index)
    {
        return _spawnAreas[index].GetRotate();
    }
}

[System.Serializable]
public class SpawnArea
{
    public Transform Point;
    public float Radius = 2f;

    public Vector3 GetRandomPoint()
    {
        return new Vector3(Point.position.x + Random.Range(-Radius, Radius), Point.position.y, Point.position.z + Random.Range(-Radius, Radius));
    }

    public Vector3 GetPoint()
    {
        return Point.position;
    }

    public Quaternion GetRotate()
    {
        return Point.rotation;
    }
}