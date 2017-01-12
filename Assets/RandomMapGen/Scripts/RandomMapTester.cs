using UnityEngine;
using System.Collections;

public class RandomMapTester : MonoBehaviour {

	[Header("Map Dimensions")]
	public int mapWidth = 20;
	public int mapHeight = 20;

	[Space]
	[Header("Vizualize Map")]
	public GameObject mapContainer;
	public GameObject tilePrefab;
	public Vector2 tileSize = new Vector2(16,16);

	[Space]
	[Header("Map Sprites")]
	public Texture2D islandTexture;

	[Space]
	[Header("FoW Texture")]
	public Texture2D fowTexture;

	[Space]
	[Header("Player")]
	public GameObject playerPrefab; 
	public GameObject player;
	public int distance = 3;

	[Space]
	[Header("Decorate Map")]
	[Range(0, .9f)]
	public float erodePercent = .5f;
	public int erodeIterations = 2;
	[Range(0, .9f)]
	public float treePercent = .3f;
	[Range(0, .9f)]
	public float hillPercent = .2f;
	[Range(0, .9f)]
	public float mountainsPercent = .1f;
	[Range(0, .9f)]
	public float townPercent = .05f;
	[Range(0, .9f)]
	public float monsterPercent = .1f;
	[Range(0, .9f)]
	public float lakePercent = .05f;

	public Map map;

	private int tmpX;
	private int tmpY;
	private Sprite[] islandTileSprites;
	private Sprite[] fowTileSprites;

	// Use this for initialization
	void Start () {
		islandTileSprites = Resources.LoadAll<Sprite> (islandTexture.name);
		fowTileSprites = Resources.LoadAll<Sprite> (fowTexture.name);

		Reset ();
	}

	public void Reset() {
		map = new Map ();
		MakeMap ();
		StartCoroutine (AddPlayer ());
	}

	IEnumerator AddPlayer() {
		yield return new WaitForEndOfFrame ();
		CreatePlayer ();
	}
	
	public void MakeMap(){
		map.NewMap (mapWidth, mapHeight);
		map.CreateIsland (
			erodePercent,
			erodeIterations,
			treePercent,
			hillPercent,
			mountainsPercent,
			townPercent,
			monsterPercent,
			lakePercent
		);
		CreateGrid ();
		CenterMap (map.castleTile.id);
	}

	void CreateGrid(){
		ClearMapContainer ();

		var total = map.tiles.Length;
		var maxColumns = map.columns;
		var column = 0;
		var row = 0;

		for (var i = 0; i < total; i++) {

			column = i % maxColumns;

			var newX = column * tileSize.x;
			var newY = -row * tileSize.y;

			var go = Instantiate (tilePrefab);
			go.name = "Tile " + i;
			go.transform.SetParent (mapContainer.transform);
			go.transform.position = new Vector3 (newX, newY, 0);

			DecorateTile (i);

			if (column == (maxColumns - 1)) {
				row++;
			}

		}

	}

	private void DecorateTile(int tileID) {
		var tile = map.tiles [tileID];
		var spriteID = tile.autotileID;
		var go = mapContainer.transform.GetChild (tileID).gameObject;

		if (spriteID >= 0) {
			var sr = go.GetComponent<SpriteRenderer> ();
			if (tile.visited) {
				sr.sprite = islandTileSprites [spriteID];
			} else {
				tile.CalculateFoWAutotileID ();
				sr.sprite = fowTileSprites [Mathf.Min(tile.fowAutotileID,fowTileSprites.Length-1)];
			}
		}
	}

	public void CreatePlayer() {
		player = Instantiate (playerPrefab);
		player.name = "Player";
		player.transform.SetParent (mapContainer.transform);

		var controller = player.GetComponent<MapMovementController> ();
		controller.map = map; 
		controller.tileSize = tileSize;

		controller.tileActionCallback += TileActionCallback;

		var moveScript = Camera.main.GetComponent<MoveCamera> ();
		moveScript.target = player;

		controller.MoveTo (map.castleTile.id);

	}

	void TileActionCallback(int type) {
		var tileID = player.GetComponent<MapMovementController> ().currentTile;
		VisitTile (tileID);
	}

	void ClearMapContainer(){

		var children = mapContainer.transform.GetComponentsInChildren<Transform> ();
		for (var i = children.Length - 1; i > 0; i--) {
			Destroy (children [i].gameObject);
		}
	}

	void CenterMap(int index){

		var camPos = Camera.main.transform.position;
		var width = map.columns;

		PosUtil.CalculatePosition (index, width, out tmpX, out tmpY);

		camPos.x = tmpX * tileSize.x;
		camPos.y = -tmpY * tileSize.y;
		Camera.main.transform.position = camPos;
	}

	void VisitTile(int index) {
		int column, newX, newY, row = 0;
		PosUtil.CalculatePosition (index, map.columns, out tmpX, out tmpY);

		var half = Mathf.FloorToInt (distance / 2f);
		tmpX -= half; 
		tmpY -= half; 

		var total = distance * distance; 
		var maxColumns = distance - 1;
		for (int i = 0; i < total; i++) {
			column = i % distance; 
			newX = column + tmpX;
			newY = row + tmpY;

			PosUtil.CalculateIndex (newX, newY, map.columns, out index);

			if (newX > -1 && newX < map.columns && index > -1 && index < map.tiles.Length) {
				var tile = map.tiles [index];
				tile.visited = true;
				DecorateTile (index);
				foreach (var neighbor in tile.neighbors) {
					if (neighbor != null) {
						if (!neighbor.visited) {
							neighbor.CalculateFoWAutotileID ();
							DecorateTile (neighbor.id);
						}
					}
				}
			}
			if (column == maxColumns) {
				row++;
			}
		}
	}
}
