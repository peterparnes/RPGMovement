﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapMovementController : MonoBehaviour  {

	public Map map;
	public Vector2 tileSize;
	public int currentTile;
	public float speed = 1f;
	public bool moving;
	public int[] blockedTileTypes;
	public delegate void TileAction(int type);
	public TileAction tileActionCallback;
	public delegate void MoveAction();
	public MoveAction moveActionCallback;

	private float moveTime; 
	private Vector2 startPos; 
	private Vector2 endPos; 
	private int tmpIndex;
	private int tmpX;
	private int tmpY;

	public void MoveTo(int index, bool animate = false) {

		if (!CanMove (index)) {
			return;
		}

		if (moveActionCallback != null) {
			moveActionCallback ();
		}

		currentTile = index;
		PosUtil.CalculatePosition (index, map.columns, out tmpX, out tmpY); 
		tmpX *= (int)tileSize.x;
		tmpY *= -(int)tileSize.x;

		var newPos = new Vector3(tmpX, tmpY, 0);
		if (!animate) {
			transform.position = newPos;
			if (tileActionCallback != null) {
				tileActionCallback(map.tiles[currentTile].autotileID);
			}
		} else {
			startPos = transform.position;
			endPos = newPos; 
			moving = true;
			moveTime = 0; 
		}
	}


	public void MoveInDirection(Vector2 dir) {
		PosUtil.CalculatePosition (currentTile, map.columns, out tmpX, out tmpY);

		tmpX += (int)dir.x;
		tmpY += (int)dir.y;

		if (tmpX < 0 || tmpX >= map.columns) { // do not warp around edge 
			return;
		}

		PosUtil.CalculateIndex(tmpX, tmpY, map.columns, out tmpIndex);

		Debug.Log (tmpX + " " + tmpY + " " + map.columns + " " + tmpIndex);

		MoveTo(tmpIndex, true);
	}

	private void Update() {
		if(moving) {
			moveTime += Time.deltaTime;
			if(moveTime>speed) {
				moving = false;
				transform.position = endPos;

				if (tileActionCallback != null) {
					tileActionCallback(map.tiles[currentTile].autotileID);
				}
			}

			transform.position = Vector2.Lerp(startPos, endPos, moveTime/speed);
		} 
	}

	bool CanMove(int index) {
		if (index < 0 || index >= map.tiles.Length) {
			return false;
		}

		var tileType = map.tiles [index].autotileID;
	
		if (moving || Array.IndexOf(blockedTileTypes, tileType) > -1) {
			return false;
		}

		return true; 
	}


}
