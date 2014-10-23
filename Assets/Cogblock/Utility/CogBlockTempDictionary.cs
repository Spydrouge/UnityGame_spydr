using UnityEngine;
using UnityEditor;

using System.Collections;
using Cubiquity;
using Cubiquity.Impl;
using System.Collections.Generic;

using CubSub;
using System;

namespace CogBlock
{
	public class CogBlockTempDictionary:MonoBehaviour
	{

		protected Dictionary<int, Material> translator;
		protected Dictionary<String, String> names;

		public Material Translate(QuantizedColor color)
		{
			int decColor = CubSub.HexColor.DecQuant(color);

			if(translator.ContainsKey(decColor))
			{
				return translator[decColor];
			}
			else
			{
				return translator[0];
			}
			
		}

		public void DefaultNames()
		{
			names = new Dictionary<string, string>();
			names.Add ("default", "Materials/Default_Block");
			names.Add ("dirt", "Materials/Dirt_Block");
			names.Add ("grass", "Materials/Grass_Block");
			names.Add ("rock", "Materials/Rock_Block");
		}

		public void Start()
		{
			DefaultNames ();
			DefaultDictionary();
		}

		protected Material MatMake(string name)
		{
			return Instantiate(Resources.Load(names[name], typeof(Material))) as Material;
		}

		protected void DefaultDictionary()
		{
			translator = new Dictionary<int, Material>(){
			
				{HexColor.DecHex("00000000"), MatMake ("default")},  //This should be an empty block. I watched as it was used in the 'delete cube' function of Cubiquity (Quantized color(0,0,0,0))
				{HexColor.DecHex("636573FF"), MatMake ("rock")},  	//Stone
				{HexColor.DecHex("19E619FF"), MatMake ("grass")},		//Grass
				{HexColor.DecHex("8F4700FF"), MatMake ("dirt")},  	//Dirt
				{HexColor.DecHex("8A8C96FF"), MatMake ("rock")},		//Cobble
				{HexColor.DecHex("CCAA00FF"), MatMake ("dirt")},		//Wood
				{HexColor.DecHex("CBFF96FF"), MatMake ("grass")},		//Sapling
				{HexColor.DecHex("000A24FF"), MatMake ("rock")},		//Bedrock
				{HexColor.DecHex("4064C7FF"), MatMake ("default")},		//Flowing Water
				{HexColor.DecHex("1B24A6FF"), MatMake ("default")},		//Water
				{HexColor.DecHex("ED590EFF"), MatMake ("default")},	//Flowing Lava
				{HexColor.DecHex("D94800FF"), MatMake ("default")},	//Lava
				{HexColor.DecHex("F7F2C8FF"), MatMake ("dirt")},	//Sand
				{HexColor.DecHex("1A1616FF"), MatMake ("dirt")},	//Coal 
				{HexColor.DecHex("FFCC00FF"), MatMake ("dirt")},	//Log
				{HexColor.DecHex("39A377FF"), MatMake ("grass")},	//Leaves
				{HexColor.DecHex("FFB8E0FF"), MatMake ("default")},	//Bed
				{HexColor.DecHex("E7E8D8FF"), MatMake ("default")},	//Wool
				{HexColor.DecHex("CCCCCCFF"), MatMake ("rock")},	//Stone slab
				{HexColor.DecHex("753437FF"), MatMake ("dirt")},	//Brick
				{HexColor.DecHex("FF1C27FF"), MatMake ("dirt")},	//TNT
				{HexColor.DecHex("2E2D2DFF"), MatMake ("rock")},	//Furnace
				{HexColor.DecHex("67ED00FF"), MatMake ("grass")},	//Cactus
				{HexColor.DecHex("C795F0FF"), MatMake ("default")}	//Portal
				};
		}


	}
}
