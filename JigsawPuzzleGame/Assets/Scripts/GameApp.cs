using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameApp : Patterns.Singleton<GameApp>
{
  public bool TileMovementEnabled { get; set; } = false;
  public double SecondsSinceStart { get; set; } = 0;
  public int TotalTilesInCorrectPosition { get; set; } = 0;

  [SerializeField]
  List<string> jigsawImageNames = new List<string>();
   [SerializeField]List<Vector2> ImageCuttingDimensions = new List<Vector2>();

  int imageIndex = 0;

  public string GetJigsawImageName(int selectedLevel)
  {
    //string imageName = jigsawImageNames[imageIndex];
     string imageName = jigsawImageNames[selectedLevel-1];
    if(imageIndex == jigsawImageNames.Count)
    {
      imageIndex = 0;
    }
    return imageName;
  }


  public Vector2 GetJigsawImageDimension(int selectedLevel)
  {
  
    return ImageCuttingDimensions[selectedLevel-1];
  }
}
