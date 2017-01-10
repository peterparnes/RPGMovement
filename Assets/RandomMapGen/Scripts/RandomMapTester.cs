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
	[Header("Player")]
	public GameObject playerPrefab; 
	public GameObject player;

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

	// Use this for initialization
	void Start () {
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
		Sprite[] sprites = Resources.LoadAll<Sprite> (islandTexture.name);

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

			var tile = map.tiles [i];
			var spriteID = tile.autotileID;

			if (spriteID >= 0) {
				var sr = go.GetComponent<SpriteRenderer> ();
				sr.sprite = sprites [spriteID];
			}

			if (column == (maxColumns - 1)) {
				row++;
			}

		}

	}

	public void CreatePlayer() {
		player = Instantiate (playerPrefab);
		player.name = "Player";
		player.transform.SetParent (mapContainer.transform);

		PosUtil.CalculatePosition (map.castleTile.id, map.columns, out tmpX, out tmpY);

		tmpX *= (int)tileSize.x;
		tmpY *= -(int)tileSize.y;

		player.transform.position = new Vector3 (tmpX, tmpY, 0);
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
}
