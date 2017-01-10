using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosUtil {

	public static void CalculateIndex(int x, int y, int width, out int index) {
		index = x + y * width;
	}

	public static void CalculatePosition(int index, int width, out int x, out int y) {
		x = index % width; 
		y = index / width;
	}
}
