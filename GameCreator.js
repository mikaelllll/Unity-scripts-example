#pragma strict
import System.IO;
var Wall : GameObject;
var PacMan : GameObject;
var Food : GameObject;
var InviFood : GameObject;
var Ghosth : GameObject;
var MainCamera : Camera;

//Script that load a file and transform the pixels of a image file into a instance of a map for a pacman game. One different object for each different collor

public class GameCreator extends MonoBehaviour{
	var totalFood : int = 0;
	var adjustw : float = 0;
	var adjusth : float = 0;
	var currentMap : int = 0;
	var MapDirectoryPath : DirectoryInfo;
	var maps : FileInfo[];
	var Player : GameObject;
	var Foods : GameObject[];
		
	function Awake () {
		DontDestroyOnLoad(gameObject);
		MapDirectoryPath = new DirectoryInfo(String.Concat(Application.dataPath,"/Map"));
		maps = MapDirectoryPath.GetFiles("*.png");
	}

	function Start () {
		Application.LoadLevel("Empty");
		yield;
		yield;
		LoadMap(0);
	}
	
	function Update () {
	}
	
	function LateUpdate () {
		if(Player != null){
			MainCamera.transform.position.x = Player.transform.position.x;
			MainCamera.transform.position.y = Player.transform.position.y;
			MainCamera.transform.position.z = Player.transform.position.z-6;
		}
	}
	
	function LoadMap (mapnumber : int) {
		var map : WWW = new WWW("file://"+maps[mapnumber].FullName);
		var mapwidth : int = map.texture.width;
		var mapheight : int = map.texture.height;
		
		if(mapwidth % 2 == 0){
			adjustw = 0.5;
		}
		else{
			adjustw = 1;
		}
		if(mapheight % 2 == 0){
			adjusth = 0.5;
		}
		else{
			adjusth = 1;
		}
		
		for (var i : int = 0; i<mapheight; i++){
			for(var j : int = 0; j < mapwidth; j++){
				var pixelColor : Color = map.texture.GetPixel(i, j);
				var position : Vector3 = new Vector3(i-(mapheight/2.0)-adjusth, j-(mapwidth/2.0)-adjustw, 10);
				var rotation : Quaternion = Quaternion.identity;
				if(pixelColor == Color.black){
					Instantiate(Wall, position, rotation);
				}
				if(pixelColor == Color.red){
					Instantiate(InviFood, position, rotation);
				}
				if(pixelColor == Color.blue){
					Instantiate(Ghosth, position, rotation);
				}
				if(pixelColor == Color.green){
					MainCamera.transform.position = new Vector3(position.x, position.y, position.z-6);
					Instantiate(PacMan, position, rotation);
					Player = GameObject.FindGameObjectWithTag("Player");
				}
				if(pixelColor == Color.white){
					Instantiate(Food, position, rotation);
					totalFood+= 1;
				}
			}
		}
	}
}