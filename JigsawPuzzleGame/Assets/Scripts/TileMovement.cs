using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileMovement : MonoBehaviour
{
    public Tile tile { get; set; }
    private Vector3 mOffset = Vector3.zero;
    private SpriteRenderer mSpriteRenderer;

    public delegate void DelegateOnTileInPlace(TileMovement tm);
    public DelegateOnTileInPlace onTileInPlace;
    public bool isDragging = false;
    GameSound SOUND;
    void Start()
    {
      SOUND = FindFirstObjectByType<GameSound>();
        mSpriteRenderer = GetComponent<SpriteRenderer>();
       
    }
    public void RandomRotaion(){
         int Rotaion=Random.Range(0,4);
        for(int i=0;i<=Rotaion;i++){
            RotateTile();
        }
    }

    private Vector3 GetCorrectPosition()
    {
        return new Vector3(tile.xIndex * 100f, tile.yIndex * 100f, 0f);
    }
    Vector3 tempTransform;

    private void OnMouseDown()
    {
      
      tempTransform=transform.position;
        if (!GameApp.Instance.TileMovementEnabled) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        isDragging = false; // Reset dragging status on mouse down
        SOUND.select();
        mOffset = transform.position - Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f));

        // Bring tile to top layer
        Tile.tilesSorting.BringToTop(mSpriteRenderer);
        GameObject.FindFirstObjectByType<BoardGen>().selectedTile = transform;
    }

    private void OnMouseDrag()
    {
        if (!GameApp.Instance.TileMovementEnabled) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;


        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + mOffset;
        transform.position = curPosition;
        if(tempTransform!=transform.position){
          isDragging = true; // Dragging is happening
         

        }
    }

    private void OnMouseUp()
    {
        if (!GameApp.Instance.TileMovementEnabled) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        float dist = (transform.position - GetCorrectPosition()).magnitude;
        float pad = PlayerPrefs.GetInt("SelectedLevel") == 5 ? 50f : 20f;

        if (dist < pad && transform.rotation.z == 0)
        {
            transform.position = GetCorrectPosition();
            onTileInPlace?.Invoke(this);
        }

        // Only rotate if the tile was NOT dragged
        if (!isDragging)
        {
            RotateTile();
        }

        isDragging = false; // Reset dragging status on mouse release
    }

    private void RotateTile()
    {
        SpriteRenderer spriteRenderer = transform.GetComponent<SpriteRenderer>();
        Vector3 pivotOffset = spriteRenderer.bounds.extents * 2; // Half-size of the sprite
        float currentRotation = transform.eulerAngles.z; // Get current rotation

        currentRotation -= 90;
        if (currentRotation < 0) currentRotation += 360; // Keep within 0-360 range

        // Adjust position based on new rotation
        Vector3 positionOffset = Vector3.zero;
        if (currentRotation == 270) positionOffset = new Vector3(0, pivotOffset.y, 0); // -90°
        else if (currentRotation == 180) positionOffset = new Vector3(pivotOffset.x, 0, 0); // -180°
        else if (currentRotation == 90) positionOffset = new Vector3(0, -pivotOffset.y, 0); // 90°
        else if (currentRotation == 0) positionOffset = new Vector3(-pivotOffset.x, 0, 0); // 0°

        transform.position += positionOffset;
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }

    void Update()
    {
        // No need for Update() in this case
    }
}
