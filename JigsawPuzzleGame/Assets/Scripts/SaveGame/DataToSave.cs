using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PuzzleSaveData
{
    public string imagePath;
    public double Time;
    public int LevelNumber;
    public List<PuzzlePieceData> pieces = new List<PuzzlePieceData>();
}

[System.Serializable]
public class PuzzlePieceData
{
    public string name;
    public SerializableTransform transform;
    public bool isMoveable;
}
[System.Serializable]
public class SerializableTransform
{
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public float scaleX, scaleY, scaleZ;

    // Default constructor (needed for deserialization)
    public SerializableTransform() { }

    public SerializableTransform(Transform transform)
    {
        posX = transform.position.x;
        posY = transform.position.y;
        posZ = transform.position.z;

        rotX = transform.rotation.x;
        rotY = transform.rotation.y;
        rotZ = transform.rotation.z;
        rotW = transform.rotation.w;

        scaleX = transform.localScale.x;
        scaleY = transform.localScale.y;
        scaleZ = transform.localScale.z;
    }

    public void ApplyToTransform(Transform transform)
    {
        transform.position = new Vector3(posX, posY, posZ);
        transform.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }
}
