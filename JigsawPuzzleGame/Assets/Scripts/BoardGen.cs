using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardGen : MonoBehaviour
{

  PuzzleSaveData getSavedData;
  private string imageFilename;
  Sprite mBaseSpriteOpaque;
  Sprite mBaseSpriteTransparent;

  GameObject mGameObjectOpaque;
  GameObject mGameObjectTransparent;


  public float ghostTransparency = 0.1f;

  // Jigsaw tiles creation.
  public int numTileX { get; private set; }
  public int numTileY { get; private set; }

  Tile[,] mTiles = null;
  GameObject[,] mTileGameObjects= null;

  public Transform parentForTiles = null;

  // Access to the menu.
  public Menu menu = null;
  private List<Rect> regions = new List<Rect>();
  private List<Coroutine> activeCoroutines = new List<Coroutine>();

  [Header("Rotate Tile")]
  public Transform selectedTile;



  Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = new RenderTexture(newWidth, newHeight, 32);
        RenderTexture.active = rt;

        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        rt.Release();

        return result;
    }
  



  Sprite LoadBaseTexture()
  {
     Texture2D tex ;
    if(PlayerPrefs.GetInt("LoadSavedGame")==1){
      menu.LoadingScreen.SetActive(true);
      LoadSavedGame();
       tex = SpriteUtils.LoadTexture(getSavedData.imagePath);

    }
    else{

      tex = SpriteUtils.LoadTexture(imageFilename);
    }

    int selectedLevel =PlayerPrefs.GetInt("SelectedLevel");
     switch (selectedLevel)
        {
            case 1:
                 tex = Resize(tex, 1100,600);
                break;
            case 2:
                 tex = Resize(tex, 1400,800);
                break;
            case 3:
                 tex = Resize(tex, 1800,1000);
                break;
            case 4:
                 tex = Resize(tex, 1300,1300);
                break;
            case 5:
                 tex = Resize(tex, 2000,2000);
                break;
            default:
                 tex = Resize(tex, 600,600);
                break;
        }
    
   
    if (!tex.isReadable)
    {
      Debug.Log("Error: Texture is not readable");
      return null;
    }

    if (tex.width % Tile.tileSize != 0 || tex.height % Tile.tileSize != 0)
    {
      Debug.Log("Error: Image must be of size that is multiple of <" + Tile.tileSize + ">");
      return null;
    }

    // Add padding to the image.
    Texture2D newTex = new Texture2D(
        tex.width + Tile.padding * 2,
        tex.height + Tile.padding * 2,
        TextureFormat.ARGB32,
        false);

    // Set the default colour as white
    for (int x = 0; x < newTex.width; ++x)
    {
      for (int y = 0; y < newTex.height; ++y)
      {
        newTex.SetPixel(x, y, Color.white);
      }
    }

    // Copy the colours.
    for (int x = 0; x < tex.width; ++x)
    {
      for (int y = 0; y < tex.height; ++y)
      {
        Color color = tex.GetPixel(x, y);
        color.a = 1.0f;
        newTex.SetPixel(x + Tile.padding, y + Tile.padding, color);
      }
    }
    newTex.Apply();

    Sprite sprite = SpriteUtils.CreateSpriteFromTexture2D(
        newTex,
        0,
        0,
        newTex.width,
        newTex.height);
    return sprite;
  }




  // Start is called before the first frame update
  void Start()
  {
    imageFilename = GameApp.Instance.GetJigsawImageName(PlayerPrefs.GetInt("SelectedLevel"));

    mBaseSpriteOpaque = LoadBaseTexture();
    mGameObjectOpaque = new GameObject();
    mGameObjectOpaque.name = imageFilename + "_Opaque";
    mGameObjectOpaque.AddComponent<SpriteRenderer>().sprite = mBaseSpriteOpaque;
    mGameObjectOpaque.GetComponent<SpriteRenderer>().sortingLayerName = "Opaque";

    mBaseSpriteTransparent = CreateTransparentView(mBaseSpriteOpaque.texture);
    mGameObjectTransparent = new GameObject();
    mGameObjectTransparent.name = imageFilename + "_Transparent";
    mGameObjectTransparent.AddComponent<SpriteRenderer>().sprite = mBaseSpriteTransparent;
    mGameObjectTransparent.GetComponent<SpriteRenderer>().sortingLayerName = "Transparent";

    mGameObjectOpaque.gameObject.SetActive(false);

    SetCameraPosition();
  
    // Create the Jigsaw tiles.
    //CreateJigsawTiles();
    StartCoroutine(Coroutine_CreateJigsawTiles());
  }

  Sprite CreateTransparentView(Texture2D tex)
  {
    Texture2D newTex = new Texture2D(
      tex.width,
      tex.height, 
      TextureFormat.ARGB32, 
      false);

    for(int x = 0; x < newTex.width; x++)
    {
      for(int y = 0; y < newTex.height; y++)
      {
        Color c = tex.GetPixel(x, y);
        if(x > Tile.padding && 
           x < (newTex.width - Tile.padding) &&
           y > Tile.padding && 
           y < (newTex.height - Tile.padding))
        {
          c.a = ghostTransparency;
        }
        newTex.SetPixel(x, y, c);
      }
    }
    newTex.Apply();

    Sprite sprite = SpriteUtils.CreateSpriteFromTexture2D(
      newTex,
      0,
      0,
      newTex.width,
      newTex.height);
    return sprite;
  }

  void SetCameraPosition()
  {
    Camera.main.transform.position = new Vector3(mBaseSpriteOpaque.texture.width / 2,
      mBaseSpriteOpaque.texture.height / 2, -50.0f);
    //Camera.main.orthographicSize = mBaseSpriteOpaque.texture.width / 2;
    int smaller_value = Mathf.Min(mBaseSpriteOpaque.texture.width, mBaseSpriteOpaque.texture.height);
    Camera.main.orthographicSize = smaller_value * 1f;
    
  }

  public static GameObject CreateGameObjectFromTile(Tile tile)
  {
    GameObject obj = new GameObject();

    obj.name = "TileGameObe_" + tile.xIndex.ToString() + "_" + tile.yIndex.ToString();

    obj.transform.position = new Vector3(tile.xIndex * Tile.tileSize, tile.yIndex * Tile.tileSize, 0.0f);

    SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = SpriteUtils.CreateSpriteFromTexture2D(
      tile.finalCut,
      0,
      0,
      Tile.padding * 2 + Tile.tileSize,
      Tile.padding * 2 + Tile.tileSize);

    BoxCollider2D box = obj.AddComponent<BoxCollider2D>();

    TileMovement tileMovement = obj.AddComponent<TileMovement>();
    tileMovement.tile = tile;

    return obj;
  }

  void CreateJigsawTiles()
  {
    Texture2D baseTexture = mBaseSpriteOpaque.texture;
    numTileX = baseTexture.width / Tile.tileSize;
    numTileY = baseTexture.height / Tile.tileSize;

    mTiles = new Tile[numTileX, numTileY];
    mTileGameObjects = new GameObject[numTileX, numTileY];

    for(int i = 0; i < numTileX; i++)
    {
      for(int j = 0; j < numTileY; j++)
      {
        mTiles[i, j] = CreateTile(i, j, baseTexture);
        mTileGameObjects[i, j] = CreateGameObjectFromTile(mTiles[i, j]);
        if(parentForTiles != null)
        {
          mTileGameObjects[i, j].transform.SetParent(parentForTiles);
        }
      }
    }

    // Enable the bottom panel and set the onlcick delegate to the play button.
    //menu.SetEnableBottomPanel(true);
    //menu.btnPlayOnClick = ShuffleTiles;
    ShuffleTiles();
  }

  IEnumerator Coroutine_CreateJigsawTiles()
  {
    Texture2D baseTexture = mBaseSpriteOpaque.texture;
    numTileX = baseTexture.width / Tile.tileSize;
    numTileY = baseTexture.height / Tile.tileSize;

    mTiles = new Tile[numTileX, numTileY];
    mTileGameObjects = new GameObject[numTileX, numTileY];

    for (int i = 0; i < numTileX; i++)
    {
      for (int j = 0; j < numTileY; j++)
      {
        mTiles[i, j] = CreateTile(i, j, baseTexture);
        mTileGameObjects[i, j] = CreateGameObjectFromTile(mTiles[i, j]);
        if (parentForTiles != null)
        {
          mTileGameObjects[i, j].transform.SetParent(parentForTiles);
        }

        yield return null;
      }
    }

    // Enable the bottom panel and set the delegate to button play on click.
    //menu.SetEnableBottomPanel(true);
    ShuffleTiles();
    //menu.btnPlayOnClick = ShuffleTiles;

  }


  Tile CreateTile(int i, int j, Texture2D baseTexture)
  {
    Tile tile = new Tile(baseTexture);
    tile.xIndex = i;
    tile.yIndex = j;

    // Left side tiles.
    if (i == 0)
    {
      tile.SetCurveType(Tile.Direction.LEFT, Tile.PosNegType.NONE);
    }
    else
    {
      // We have to create a tile that has LEFT direction opposite curve type.
      Tile leftTile = mTiles[i - 1, j];
      Tile.PosNegType rightOp = leftTile.GetCurveType(Tile.Direction.RIGHT);
      tile.SetCurveType(Tile.Direction.LEFT, rightOp == Tile.PosNegType.NEG ?
        Tile.PosNegType.POS : Tile.PosNegType.NEG);
    }

    // Bottom side tiles
    if (j == 0)
    {
      tile.SetCurveType(Tile.Direction.DOWN, Tile.PosNegType.NONE);
    }
    else
    {
      Tile downTile = mTiles[i, j - 1];
      Tile.PosNegType upOp = downTile.GetCurveType(Tile.Direction.UP);
      tile.SetCurveType(Tile.Direction.DOWN, upOp == Tile.PosNegType.NEG ?
        Tile.PosNegType.POS : Tile.PosNegType.NEG);
    }

    // Right side tiles.
    if (i == numTileX - 1)
    {
      tile.SetCurveType(Tile.Direction.RIGHT, Tile.PosNegType.NONE);
    }
    else
    {
      float toss = UnityEngine.Random.Range(0f, 1f);
      if(toss < 0.5f)
      {
        tile.SetCurveType(Tile.Direction.RIGHT, Tile.PosNegType.POS);
      }
      else
      {
        tile.SetCurveType(Tile.Direction.RIGHT, Tile.PosNegType.NEG);
      }
    }

    // Up side tile.
    if(j == numTileY - 1)
    {
      tile.SetCurveType(Tile.Direction.UP, Tile.PosNegType.NONE);
    }
    else
    {
      float toss = UnityEngine.Random.Range(0f, 1f);
      if (toss < 0.5f)
      {
        tile.SetCurveType(Tile.Direction.UP, Tile.PosNegType.POS);
      }
      else
      {
        tile.SetCurveType(Tile.Direction.UP, Tile.PosNegType.NEG);
      }
    }

    tile.Apply();
    return tile;
  }


  // Update is called once per frame
  void Update()
  {
    // if(Input.GetKey(KeyCode.R)){
    //   check=true;
    // }


//   if (selectedTile != null)
// {
//     SpriteRenderer spriteRenderer = selectedTile.GetComponent<SpriteRenderer>();
//     Vector3 pivotOffset = spriteRenderer.bounds.extents*2; // Half-size of the sprite
//     float currentRotation = selectedTile.eulerAngles.z; // Get current rotation

//     if (Input.GetKeyDown(KeyCode.Mouse0)) // Rotate Clockwise
//     {
//         currentRotation -= 90;
//         if (currentRotation < 0) currentRotation += 360; // Keep within 0-360 range

//         // Adjust position based on new rotation
//         Vector3 positionOffset = Vector3.zero;
//         if (currentRotation == 270) positionOffset = new Vector3(0, pivotOffset.y, 0); // -90°
//         else if (currentRotation == 180) positionOffset = new Vector3(pivotOffset.x, 0, 0); // -180°
//         else if (currentRotation == 90) positionOffset = new Vector3(0, -pivotOffset.y, 0); // 90°
//         else if (currentRotation == 0) positionOffset = new Vector3(-pivotOffset.x, 0, 0); // 0°

//         selectedTile.position += positionOffset;
//         selectedTile.rotation = Quaternion.Euler(0, 0, currentRotation);
//     }
//     // else if (Input.GetKeyDown(KeyCode.LeftArrow)) // Rotate Counterclockwise
//     // {
//     //     currentRotation += 90;
//     //     if (currentRotation >= 360) currentRotation -= 360; // Keep within 0-360 range

//     //     // Adjust position based on new rotation (inverse of right rotation)
//     //     Vector3 positionOffset = Vector3.zero;
//     //     if (currentRotation == 90) positionOffset = new Vector3(pivotOffset.x,0, 0);
//     //     else if (currentRotation == 180) positionOffset = new Vector3(0, pivotOffset.y, 0);
//     //     else if (currentRotation == 270) positionOffset = new Vector3(-pivotOffset.x, 0, 0);
//     //     else if (currentRotation == 0) positionOffset = new Vector3(0, -pivotOffset.y, 0);

//     //     selectedTile.position += positionOffset;
//     //     selectedTile.rotation = Quaternion.Euler(0, 0, currentRotation);
//     // }
// }



  }

  #region Shuffling related codes
  int CountRandom=0;
  private IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds,bool ShouldRotate)
  {
    float elaspedTime = 0.0f;
    if(ShouldRotate){

    objectToMove.GetComponent<TileMovement>().RandomRotaion();
    CountRandom++;
    }
    Vector3 startingPosition = objectToMove.transform.position;
    while(elaspedTime < seconds)
    {
      objectToMove.transform.position = Vector3.Lerp(
        startingPosition, end, (elaspedTime / seconds));
      elaspedTime += Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }
    objectToMove.transform.position = end;
  }

  void Shuffle(GameObject obj, bool Shouldshuffle)
  {
    if(regions.Count == 0)
    {
      regions.Add(new Rect(-300.0f, -100.0f, 50.0f, numTileY * Tile.tileSize));
      regions.Add(new Rect((numTileX+1) * Tile.tileSize, -100.0f, 50.0f, numTileY * Tile.tileSize));
    }

    int regionIndex = UnityEngine.Random.Range(0, regions.Count);
    float x = UnityEngine.Random.Range(regions[regionIndex].xMin, regions[regionIndex].xMax);
    float y = UnityEngine.Random.Range(regions[regionIndex].yMin, regions[regionIndex].yMax);

    Vector3 pos = new Vector3(x, y, 0.0f);
    Coroutine moveCoroutine = StartCoroutine(Coroutine_MoveOverSeconds(obj, pos, 1.0f,Shouldshuffle));
    activeCoroutines.Add(moveCoroutine);
  }

  IEnumerator Coroutine_Shuffle()
  {
    for(int i = 0; i < numTileX; ++i)
    {
      for(int j = 0; j < numTileY; ++j)
      {
        bool shouldRotate = UnityEngine.Random.value < 0.2f; // 20% chance to rotate
        Shuffle(mTileGameObjects[i, j], shouldRotate);
      
        yield return null;
      }
    }

    foreach(var item in activeCoroutines)
    {
      if(item != null)
      {
        yield return null;
      }
    }
   
    OnFinishedShuffling();
  }

  public void ShuffleTiles()
  {
    StartCoroutine(Coroutine_Shuffle());
  }

  void OnFinishedShuffling()
  {
    activeCoroutines.Clear();

    menu.SetEnableBottomPanel(false);
    StartCoroutine(Coroutine_CallAfterDelay(() =>{
    menu.SetEnableTopPanel(true);  
    
    if(PlayerPrefs.GetInt("LoadSavedGame")==1){
      SetLoadPiecesPositions(getSavedData.pieces); 
    }
    
    }, 1.0f));
    GameApp.Instance.TileMovementEnabled = true;
    Debug.Log(" tOTAL SUGGLE "+CountRandom);
    StartTimer();
    
   // Debug.Log("The Items in teh list : "+Tile.tilesSorting.mSortIndices.Count);
    for(int i = 0; i < numTileX; ++i)
    {
      for(int j = 0; j < numTileY; ++j)
      {
        TileMovement tm = mTileGameObjects[i, j].GetComponent<TileMovement>();
        tm.onTileInPlace += OnTileInPlace;
        SpriteRenderer spriteRenderer = tm.gameObject.GetComponent<SpriteRenderer>();
       // Debug.Log(tm.gameObject.name+ "   "+ spriteRenderer);

        Tile.tilesSorting.BringToTop(spriteRenderer);
      }
    }

    menu.SetTotalTiles(numTileX * numTileY);
   
  }

  IEnumerator Coroutine_CallAfterDelay(System.Action function, float delay)
  {
    yield return new WaitForSeconds(delay);
    function();
  }


  public void StartTimer()
  {
    StartCoroutine(Coroutine_Timer());
  }

  IEnumerator Coroutine_Timer()
  {
    while(true)
    {
      yield return new WaitForSeconds(1.0f);
      GameApp.Instance.SecondsSinceStart += 1;

      menu.SetTimeInSeconds(GameApp.Instance.SecondsSinceStart);
    }
  }

  public void StopTimer()
  {
    StopCoroutine(Coroutine_Timer());
  }

  #endregion

  public void ShowOpaqueImage()
  {
    mGameObjectOpaque.SetActive(true);
  }

  public void HideOpaqueImage()
  {
    mGameObjectOpaque.SetActive(false);
  }

  bool  check=false;
  void OnTileInPlace(TileMovement tm)
  {
    GameApp.Instance.TotalTilesInCorrectPosition += 1;

    tm.enabled = false;
    Destroy(tm);

    SpriteRenderer spriteRenderer = tm.gameObject.GetComponent<SpriteRenderer>();
    Tile.tilesSorting.Remove(spriteRenderer);

    if (GameApp.Instance.TotalTilesInCorrectPosition == mTileGameObjects.Length || check)
    {
      //Debug.Log("Game completed. We will implement an end screen later");
      menu.SetEnableTopPanel(false);
      menu.SetEnableGameCompletionPanel(true);

      // Reset the values.
      GameApp.Instance.SecondsSinceStart = 0;
      GameApp.Instance.TotalTilesInCorrectPosition = 0;

      if(PlayerPrefs.GetInt("LoadSavedGame", 0)==1){
        PlayerPrefs.SetInt("gamesaved",0);
      }

      if(PlayerPrefs.GetInt("SelectedLevel")==PlayerPrefs.GetInt("UnlockedLevels") && PlayerPrefs.GetInt("UnlockedLevels")!=5 ){
        PlayerPrefs.SetInt("UnlockedLevels", PlayerPrefs.GetInt("UnlockedLevels")%5+1);
        PlayerPrefs.SetInt("SelectedLevel", PlayerPrefs.GetInt("UnlockedLevels"));

      
      }
      else{
         PlayerPrefs.SetInt("SelectedLevel", PlayerPrefs.GetInt("SelectedLevel")%5+1);
      }
    }
    menu.SetTilesInPlace(GameApp.Instance.TotalTilesInCorrectPosition);
  }

  public void SaveGameProgress(){
    PuzzleSaveData data=new PuzzleSaveData();
    data.imagePath=imageFilename;
    data.Time=GameApp.Instance.SecondsSinceStart;
    data.LevelNumber=PlayerPrefs.GetInt("SelectedLevel");
    foreach (Transform child in parentForTiles)
    {
            PuzzlePieceData piecedata=new PuzzlePieceData();
            piecedata.name=child.name;
            Debug.Log(child.name+ "  "+child.position+"  "+child.rotation);
            piecedata.transform=new SerializableTransform(child);
            piecedata.isMoveable=child.GetComponent<TileMovement>()?true:false;
            
            
            data.pieces.Add(piecedata);

           
    }

    PuzzleSaveSystem.SaveGame(data);
    Debug.Log("Game Saved success");
    PlayerPrefs.SetInt("gamesaved",1);

  }


  public void LoadSavedGame(){
    getSavedData=PuzzleSaveSystem.LoadGame();
    GameApp.Instance.SecondsSinceStart=getSavedData.Time;
    PlayerPrefs.SetInt("SelectedLevel",getSavedData.LevelNumber);
  }


  public void SetLoadPiecesPositions(List<PuzzlePieceData> pieceData){
    int i=0;
       foreach (Transform child in parentForTiles)
        {
          if(pieceData[i].name.Equals(child.name)){
          Debug.Log(pieceData[i].name+ "  "+pieceData[i].transform.posX+"  "+pieceData[i].transform.posY+"  "+pieceData[i].transform.posZ);
            
              pieceData[i].transform.ApplyToTransform(child);
              if(!pieceData[i].isMoveable){
                OnTileInPlace(child.GetComponent<TileMovement>());
              }
          }
          i++;
        }


         menu.LoadingScreen.SetActive(false);
  }




}
