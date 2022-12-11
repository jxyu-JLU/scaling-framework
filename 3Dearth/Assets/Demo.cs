#define LIGHTSPEED

using UnityEngine;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using FlowGraphNet;

namespace WPM {

	public class Demo : MonoBehaviour {

		
        WorldMapGlobe map;
		GUIStyle labelStyle, labelStyleShadow, buttonStyle, sliderStyle, sliderThumbStyle;
		ColorPicker colorPicker;
		bool changingFrontiersColor;
		bool minimizeState = false;
		bool animatingField;
		float zoomLevel = 1.0f;
		bool enableRotation;
		bool avoidGUI = true; // press SPACE during gameplay to hide GUI

		Color32[] colors;
		Texture2D earthTex;
		int texWidth, texHeight;
		Color penColor = Color.cyan;
		Color penColor_realmap = Color.yellow;
		int penWidth = 3;
		float lastTextureUpdateTime;
		bool needTextureUpdate;

		//gK2Recog_script gK2Recog;
        Data_get m_Data_get;
        string input_string;
        //string old_string;
        public float pitch_mot, yaw_mot, roll_mot, x_mot, y_mot, z_mot, pitch_mot_old, yaw_mot_old, roll_mot_old, x_mot_old, y_mot_old, z_mot_old;
        float vx, vy, vz, vpitch, vyaw, vroll;
        float factor_r, factor_p;
        float inputdatap, inputdatap_old, inputdatar, inputdatar_old, input_vp, input_vr;
        
        float inputdata, inputdata0, inputdata0r, inputdata0p, scalingdata, inputdata_v, scalingdata_pp, scalingdata_rr, scalingdata_p, scalingdata_r, scalingdata_old;
        RealMapCalibrator_script RMCalibrator;
        float lastultra_x, lastultra_y, lastultra_z;
        Vector3 lastultra_point;
        System.DateTime lastultra_time;

		NetworkSync NS;

		bool NS_FocusSet = false;
		Vector3 NS_FocusLocation;
		bool NS_NeedClearInk = false;

		void Awake () {
			// Get a reference to the World Map API:
			map = WorldMapGlobe.instance;

#if LIGHTSPEED
			Camera.main.fieldOfView = 60;
			animatingField = true;
#endif
			map.earthInvertedMode = false;
			map.OnDrag += PaintEarth;
			//ResetTexture();
		}

		void Start () {

            m_Data_get = GameObject.Find("Pen_Data").GetComponent<Data_get>();
            if (m_Data_get == null) Debug.Log("ERROR: m_Data_get == null");

			//RMCalibrator = GameObject.Find("RealMapCalibrator").GetComponent<RealMapCalibrator_script>();
			//if (RMCalibrator == null) Debug.Log("ERROR: RMCalibrator == null");
   //         RMCalibrator.ReadXml();
			/*
			RMCalibrator.SetAnchor(0, 0, 0.01f, 0.01f);
			RMCalibrator.SetAnchor(2, 0, 1, 0.02f);
			RMCalibrator.SetAnchor(6, 1.01f, 0, 0.01f);
			RMCalibrator.SetAnchor(4, 1, 1, 0.03f);
			RMCalibrator.SaveXml();
			RMCalibrator.ReadXml();
			RMCalibrator.Calibrate();
			*/

			//gK2Recog = GameObject.Find("gK2Recog").GetComponent<gK2Recog_script>();
			//if (gK2Recog == null) Debug.Log("ERROR: gK2Recog == null");

			NS = new NetworkSync(this);
			NS.GoOnline();

			// UI Setup - non-important, only for this demo
			labelStyle = new GUIStyle ();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.normal.textColor = Color.white;
			labelStyleShadow = new GUIStyle (labelStyle);
			labelStyleShadow.normal.textColor = Color.black;
			buttonStyle = new GUIStyle (labelStyle);
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.normal.background = Texture2D.whiteTexture;
			buttonStyle.normal.textColor = Color.white;
			colorPicker = gameObject.GetComponent<ColorPicker> ();
			sliderStyle = new GUIStyle ();
			sliderStyle.normal.background = Texture2D.whiteTexture;
			sliderStyle.fixedHeight = 4.0f;
			sliderThumbStyle = new GUIStyle ();
			sliderThumbStyle.normal.background = Resources.Load<Texture2D> ("thumb");
			sliderThumbStyle.overflow = new RectOffset (0, 0, 8, 0);
			sliderThumbStyle.fixedWidth = 20.0f;
			sliderThumbStyle.fixedHeight = 12.0f;

			// setup GUI resizer - only for the demo
			GUIResizer.Init (800, 500);
			//map = WorldMapGlobe.instance;
			//map.OnDrag += PaintEarth;

			ResetTexture();
			//ResetTexture();

			// Some example commands below
			//			map.ToggleCountrySurface("Brazil", true, Color.green);
			//			map.ToggleCountrySurface(35, true, Color.green);
			//			map.ToggleCountrySurface(33, true, Color.green);
			//			map.FlyToCountry(33);
			//			map.FlyToCountry("Brazil");
			//			map.navigationTime = 0; // jump instantly to next country
			//			map.FlyToCountry ("India");

			/* Register events: this is optionally but allows your scripts to be informed instantly as the mouse enters or exits a country, province or city */
			//map.OnCityEnter += (int cityIndex) => Debug.Log ("Entered city " + map.cities [cityIndex].name);
			//map.OnCityExit += (int cityIndex) => Debug.Log ("Exited city " + map.cities [cityIndex].name);
			//map.OnCityClick += (int cityIndex) => Debug.Log ("Clicked city " + map.cities [cityIndex].name);
			//map.OnCountryEnter += (int countryIndex, int regionIndex) => Debug.Log ("Entered country " + map.countries [countryIndex].name);
			//map.OnCountryExit += (int countryIndex, int r1024egionIndex) => Debug.Log ("Exited country " + map.countries [countryIndex].name);
			//map.OnCountryClick += (int countryIndex, int regionIndex) => Debug.Log ("Clicked country " + map.countries [countryIndex].name);
			//map.OnProvinceEnter += (int provinceIndex, int regionIndex) => Debug.Log ("Entered province " + map.provinces [provinceIndex].name);
			//map.OnProvinceExit += (int provinceIndex, int regionIndex) => Debug.Log ("Exited province " + map.provinces [provinceIndex].name);
			//map.OnProvinceClick += (int provinceIndex, int regionIndex) => Debug.Log ("Clicked province " + map.provinces [provinceIndex].name);
		}

		void OnDestroy()
		{
			NS.GoOffline();
		}

		Vector3 LastCursorLocation;
		System.DateTime LastDragTime = System.DateTime.Now;
		void PaintEarth(Vector3 cursorLocation)
		{
			float distance = Vector3.Distance(LastCursorLocation, cursorLocation);
			double elapsed = (System.DateTime.Now - LastDragTime).TotalMilliseconds;

			float starti = 0;
			float step = 0.02f;

			if (elapsed > 200)
				starti = 1;

			for (float i = starti; i <= 1; i += step)
			{
				// Convert cursor location to texture coordinates
				//Vector2 uv = Conversion.GetUVFromSpherePoint(cursorLocation);
				Vector3 middle = Vector3.Lerp(LastCursorLocation, cursorLocation, i);
				Vector2 uv = Conversion.GetUVFromSpherePoint(middle);
				PaintEarthUV(uv);

				NS.SendCIUV1(uv.x, 1 - uv.y);
			}
			LastCursorLocation = cursorLocation;
			LastDragTime = System.DateTime.Now;
		}

		Vector3 LastTextureUvLocation;
		System.DateTime LastHitTime = System.DateTime.Now;
		void PaintEarthFromMap(Vector2 textureUvLocation)
		{
			double elapsed = (System.DateTime.Now - LastHitTime).TotalMilliseconds;
            double distance = Vector2.Distance(LastTextureUvLocation, textureUvLocation);

			float starti = 0;
			float step = 0.05f;

			if (elapsed > 400)
				starti = 1;
            if (distance > 0.2)
                starti = 1;

			for (float i = starti; i <= 1; i += step)
			{
				Vector2 uv = Vector2.Lerp(LastTextureUvLocation, textureUvLocation, i);
				PaintEarthUV(uv);

				NS.SendCIUV1(uv.x, 1 - uv.y);
			}
			LastTextureUvLocation = textureUvLocation;
			LastHitTime = System.DateTime.Now;
		}

		void PaintEarthUV(Vector2 uv)
		{
			// Paints thick pixel on texture position
			int x = (int)(uv.x * texWidth);
			int y = (int)(uv.y * texHeight);
			for (int j = -penWidth; j < penWidth; j++)
			{
				int jj = (y + j) * texWidth;
				for (int k = -penWidth; k < penWidth; k++)
				{
					int colorIndex = jj + x + k;
					if (colorIndex < 0 || colorIndex >= colors.Length) continue;
					//Color32 currentColor = colors[colorIndex];

					//float t = 1.0f - Mathf.Clamp01((float)(j * j + k * k) / (penWidth * penWidth)); // for smooth drawing
					//colors[colorIndex] = Color32.Lerp(currentColor, penColor, t);
					colors[colorIndex] = penColor_realmap;
				}
			}
			needTextureUpdate = true;
		}

		void ResetTexture()
		{

			map.ReloadEarthTexture();

			// Get current pixels
			earthTex = map.earthTexture;
			colors = earthTex.GetPixels32();
			texWidth = earthTex.width;
			texHeight = earthTex.height;

			NS.SendCIClear1();
		}

		void OnGUI () {

			if (avoidGUI) return;

			// Do autoresizing of GUI layer
			GUIResizer.AutoResize ();

			// Check whether a country or city is selected, then show a label
			if (map.mouseIsOver) {
				string text;
				Vector3 mousePos = Input.mousePosition;
				float x, y;
				if (map.countryHighlighted != null || map.cityHighlighted != null || map.provinceHighlighted != null) {
					City city = map.cityHighlighted;
					if (city != null) {
						if (city.province!=null && city.province.Length>0) {
							text = "City: " + map.cityHighlighted.name + " (" + city.province + ", " + map.countries [map.cityHighlighted.countryIndex].name + ")";
						} else {
							text = "City: " + map.cityHighlighted.name + " (" + map.countries [map.cityHighlighted.countryIndex].name + ")";
						}
					} else if (map.provinceHighlighted != null) {
						text = map.provinceHighlighted.name + ", " + map.countryHighlighted.name;
						List<Province> neighbours = map.ProvinceNeighboursOfCurrentRegion ();
						if (neighbours.Count > 0)
							text += "\n" + EntityListToString<Province> (neighbours);
					} else if (map.countryHighlighted != null) {
						text = map.countryHighlighted.name + " (" + map.countryHighlighted.continent + ")";
						List<Country> neighbours = map.CountryNeighboursOfCurrentRegion ();
						if (neighbours.Count > 0)
							text += "\n" + EntityListToString<Country> (neighbours);
					} else {
						text = "";
					}
					x = GUIResizer.authoredScreenWidth * (mousePos.x / Screen.width);
					y = GUIResizer.authoredScreenHeight - GUIResizer.authoredScreenHeight * (mousePos.y / Screen.height) - 20 * (Input.touchSupported ? 3 : 1); // slightly up for touch devices
					GUI.Label (new Rect (x - 1, y - 1, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x + 1, y + 2, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x + 2, y + 3, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x + 3, y + 4, 0, 10), text, labelStyleShadow);
					GUI.Label (new Rect (x, y, 0, 10), text, labelStyle);
				}
				text = map.calc.prettyCurrentLatLon;
				x = GUIResizer.authoredScreenWidth / 2.0f;
				y = GUIResizer.authoredScreenHeight - 20; 
				GUI.Label (new Rect (x, y, 0, 10), text, labelStyle);
			}

			// Assorted options to show/hide frontiers, cities, Earth and enable country highlighting
			map.showFrontiers = GUI.Toggle (new Rect (10, 20, 150, 30), map.showFrontiers, "Toggle Frontiers");
			map.showEarth = GUI.Toggle (new Rect (10, 50, 150, 30), map.showEarth, "Toggle Earth");
			map.showCities = GUI.Toggle (new Rect (10, 80, 150, 30), map.showCities, "Toggle Cities");
			map.showCountryNames = GUI.Toggle (new Rect (10, 110, 150, 30), map.showCountryNames, "Toggle Labels");
			map.showProvinces = GUI.Toggle (new Rect (10, 140, 170, 30), map.showProvinces, "Toggle Provinces");

			GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.5f);

			// Add button to toggle Earth texture
			if (GUI.Button (new Rect (10, 170, 160, 30), "  Change Earth style", buttonStyle)) {
				map.earthStyle = (EARTH_STYLE)(((int)map.earthStyle + 1) % 10);
			}

			// Add buttons to show the color picker and change colors for the frontiers or fill
			if (GUI.Button (new Rect (10, 210, 160, 30), "  Change Frontiers Color", buttonStyle)) {
				colorPicker.showPicker = true;
				changingFrontiersColor = true;
			}
			if (GUI.Button (new Rect (10, 250, 160, 30), "  Change Fill Color", buttonStyle)) {
				colorPicker.showPicker = true;
				changingFrontiersColor = false;
			}
			if (colorPicker.showPicker) {
				if (changingFrontiersColor) {
					map.frontiersColor = colorPicker.setColor;
				} else {
					map.fillColor = colorPicker.setColor;
				}
			}

			// Add a button which demonstrates the navigateTo functionality -- pass the name of a country
			// For a list of countries and their names, check map.Countries collection.
			if (GUI.Button (new Rect (10, 290, 180, 30), "  Fly to Australia (Country)", buttonStyle)) {
				FlyToCountry ("Australia"); 
			}
			if (GUI.Button (new Rect (10, 325, 180, 30), "  Fly to Mexico (Country)", buttonStyle)) {
				FlyToCountry ("Mexico");
			}
			if (GUI.Button (new Rect (10, 360, 180, 30), "  Fly to New York (City)", buttonStyle)) {
				FlyToCity ("New York");
			}
			if (GUI.Button (new Rect (10, 395, 180, 30), "  Fly to Madrid (City)", buttonStyle)) {
				FlyToCity ("Madrid");
			}

			// Slider to show the new set zoom level API in V4.1
			GUI.Button (new Rect (10, 430, 85, 30), "  Zoom Level", buttonStyle);
			float prevZoomLevel = zoomLevel;
			GUI.backgroundColor = Color.white;
			zoomLevel = GUI.HorizontalSlider (new Rect (100, 445, 80, 85), zoomLevel, 0, 1, sliderStyle, sliderThumbStyle);
			GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.95f);
			if (zoomLevel != prevZoomLevel) {
				prevZoomLevel = zoomLevel;
				map.SetZoomLevel (zoomLevel);
			}


			// Add a button to colorize countries
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 20, 180, 30), "  Colorize Europe", buttonStyle)) {
				map.FlyToCity ("Brussels");
				for (int colorizeIndex =0; colorizeIndex < map.countries.Length; colorizeIndex++) {
					if (map.countries [colorizeIndex].continent.Equals ("Europe")) {
						Color color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
						map.ToggleCountrySurface (map.countries [colorizeIndex].name, true, color);
					}
				}
			}

			// Colorize random country and fly to it
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 60, 180, 30), "  Colorize Random", buttonStyle)) {
				map.FlyToCity ("Brussels");
				int countryIndex = UnityEngine.Random.Range (0, map.countries.Length);
				Color color = new Color (UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f), UnityEngine.Random.Range (0.0f, 1.0f));
				map.ToggleCountrySurface (countryIndex, true, color);
				map.FlyToCountry (countryIndex);
			}
			
			// Button to clear colorized countries
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 100, 180, 30), "  Reset countries", buttonStyle)) {
				map.HideCountrySurfaces ();
			}

			// Tickers sample
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 140, 180, 30), "  Tickers Sample", buttonStyle)) {
				TickerSample ();
			}

			// Decorator sample
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 180, 180, 30), "  Texture Sample", buttonStyle)) {
				TextureSample ();
			}

			// Moving the Earth sample
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 220, 180, 30), "  Toggle Minimize", buttonStyle)) {
				ToggleMinimize ();
			}

			// Add marker sample (gameobject)
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 260, 180, 30), "  Add Marker (Object)", buttonStyle)) {
				AddMarkerGameObjectOnRandomCity ();
			}
		
			// Add marker sample (gameobject)
			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 300, 180, 30), "  Add Marker (Circle)", buttonStyle)) {
				AddMarkerCircleOnRandomPosition ();
			}

			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 340, 180, 30), "  Add Trajectories", buttonStyle)) {
				AddTrajectories (10);
			}

			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 380, 180, 30), "  Locate Mount Point", buttonStyle)) {
				LocateMountPoint ();
			}

			if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 420, 180, 30), "  Fire Bullet!", buttonStyle)) {
				FireBullet ();
			}
		}

		void Update()
		{
            

			// Animates the camera field of view (just a cool effect at the begining)
			
            if (animatingField)
			{
				if (Camera.main.fieldOfView > 60)
				{
					Camera.main.fieldOfView -= (181.0f - Camera.main.fieldOfView) / (220.0f - Camera.main.fieldOfView);
				}
				else
				{
					Camera.main.fieldOfView = 60;
					animatingField = false;
				}
			}

			// update ink texture

			if (needTextureUpdate && Time.time - lastTextureUpdateTime > 0.05f)
			{
				needTextureUpdate = false;
				lastTextureUpdateTime = Time.time;
				earthTex.SetPixels32(colors);
				earthTex.Apply();
			}

			// keyboard control

			bool AbsoluteMove = true;
			if (!AbsoluteMove)
			{
				float level = Vector3.SignedAngle(map.transform.up, Camera.main.transform.up, Camera.main.transform.right);
				if (Input.GetKey(KeyCode.RightArrow))
				{
					map.transform.Rotate(new Vector3(0, 1, 0), -1);
				}
				if (Input.GetKey(KeyCode.LeftArrow))
				{
					map.transform.Rotate(new Vector3(0, 1, 0), 1);
				}
				if (Input.GetKey(KeyCode.UpArrow))
				{
					if (level > -90)
						map.transform.Rotate(new Vector3(1, 0, 0), 1, Space.World);
				}
				if (Input.GetKey(KeyCode.DownArrow))
				{
					if (level < 90)
						map.transform.Rotate(new Vector3(1, 0, 0), -1, Space.World);
				}
			}
			else
			{
				if (Input.GetKey(KeyCode.RightArrow))
				{
					map.transform.Rotate(new Vector3(0, 1, 0), -1, Space.World);
				}
				if (Input.GetKey(KeyCode.LeftArrow))
				{
					map.transform.Rotate(new Vector3(0, 1, 0), 1, Space.World);
				}
				if (Input.GetKey(KeyCode.UpArrow))
				{
					map.transform.Rotate(new Vector3(1, 0, 0), 1, Space.World);
				}
				if (Input.GetKey(KeyCode.DownArrow))
				{
					map.transform.Rotate(new Vector3(1, 0, 0), -1, Space.World);
				}
			}
			if (Input.GetKey(KeyCode.PageUp))
			{
				if (Camera.main.transform.position.z < -0.65)
				{
					Camera.main.transform.Translate(0, 0, 0.01f);
				}
			}
			if (Input.GetKey(KeyCode.PageDown))
			{
				if (Camera.main.transform.position.z > -1.2)
				{
					Camera.main.transform.Translate(0, 0, -0.01f);
				}
			}

			
            

            // ultrasound point calculation

            int peninputstate = m_Data_get.getpeninputstate();
            if (Input.GetKeyDown(KeyCode.B))
            {
                Application.Quit();
            }
            if (peninputstate == 2||peninputstate == 3)
            {
                float ultra_x = m_Data_get.x_mot;
                float ultra_y = m_Data_get.y_mot;
                float ultra_z = m_Data_get.z_mot;
                x_mot = m_Data_get.x_mot;
                y_mot = m_Data_get.y_mot;
                z_mot = m_Data_get.z_mot;
                pitch_mot = m_Data_get.pitch_mot;
                yaw_mot = m_Data_get.yaw_mot;
                roll_mot = m_Data_get.roll_mot;
                vx = m_Data_get.vx;
                vy = m_Data_get.vy;
                input_vp = m_Data_get.vz;
                vpitch = m_Data_get.vpitch;
                vyaw = m_Data_get.vyaw;
                input_vr = m_Data_get.vroll;
                Vector3 ultra_point = new Vector3(m_Data_get.x_mot, m_Data_get.y_mot, m_Data_get.z_mot);
                float lastultra_distance = Vector3.Distance(ultra_point, lastultra_point);
                float lastultra_elapsed = (float)(System.DateTime.Now - lastultra_time).TotalMilliseconds / 1000;
                bool lastultra_elapsedlong = lastultra_elapsed > 0.2;

                // real map ink

                if (!lastultra_elapsedlong && RMCalibrator.Calibrated)
                {
                    Vector2 uv = RMCalibrator.GetUV(m_Data_get.x, m_Data_get.y, m_Data_get.z);
                    float distance = RMCalibrator.GetDistance(m_Data_get.x, m_Data_get.y, m_Data_get.z);
                    float lastdistance = RMCalibrator.GetDistance(lastultra_x, lastultra_y, lastultra_z);
                    float speed = Mathf.Abs(distance - lastdistance) / lastultra_elapsed;
                    //Debug.Log(speed);
                    if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1 && distance < 100 && speed < 300)
                        PaintEarthFromMap(uv);
                }

                // pen/vive navigation

                if (!lastultra_elapsedlong&&peninputstate == 2)
                {
                    
                    float ultrascale = 0.03f;
                    map.transform.Rotate(new Vector3(0, -1, 0), (ultra_x - lastultra_x) * ultrascale, Space.World);
                    map.transform.Rotate(new Vector3(1, 0, 0), (ultra_y - lastultra_y) * ultrascale, Space.World);
                }

                if (!lastultra_elapsedlong && peninputstate == 3)
                {
                    if (Math.Abs(input_vp) < 10) factor_p = 0;
                    else factor_p = 1;
                    if (Math.Abs(input_vr) < 10) factor_r = 0;
                    else factor_r = 1;
                    ////需要给出速度的阈值，大于多少怎么处理，加入比较。
                    //Debug.Log("Vr= "+ input_vr+ ";  Vp= " + input_vp + "; f_r= " + factor_r + "; f_p= " + factor_p+"; 比值="+ Math.Abs(input_vr / (input_vp + 0.0001)));
                    if (Math.Abs(input_vr / (input_vp + 10)) > 3)
                    {
                        scalingdata_r = scalingdata_r + factor_r * (inputdatar - inputdatar_old);
                        //scalingdata = Mathf.Lerp(scalingdata_old,(float)Math.Pow(scalingdata_r * Mathf.PI / 18, 3)+ scalingdata_p / 10,0.5f) ;//以角度为准。累加形式
                    }
                    else
                    {
                        scalingdata_p = scalingdata_p + factor_p * (inputdatap - inputdatap_old);
                        //scalingdata = (float)Math.Pow(scalingdata_r * Mathf.PI / 18, 3) + scalingdata_p / 10;//以位移为准。累加形式
                    }
                    scalingdata = scalingdata_r * scalingdata_r / 36 + scalingdata_p * 1.5f;
                    Camera.main.transform.position = new Vector3(0, 0, scalingdata/90-2);
                    float ultrascalez = 0.0001f;
                    //Camera.main.transform.Translate(0, 0, (lastultra_z - ultra_z) * ultrascalez);
                    if (Camera.main.transform.position.z > -0.65) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -0.65f);
                    if (Camera.main.transform.position.z < -2) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -2f);
                }

                // ultrasound point store

                lastultra_x = m_Data_get.x_mot;
                lastultra_y = m_Data_get.y_mot;
                lastultra_z = m_Data_get.z_mot;
                lastultra_point = new Vector3(m_Data_get.x, m_Data_get.y, m_Data_get.z);
                lastultra_time = System.DateTime.Now;
            }

			// network focus navigation

			if (NS_FocusSet)
			{
				map.FlyToLocation(NS_FocusLocation, 0);
				NS_FocusSet = false;
			}
            Vector2 uv2 = Conversion.GetUVFromSpherePoint(map.GetCurrentMapLocation());
            NS.SendCRUV1(uv2.x, 1 - uv2.y);

			// network clear ink

			if (NS_NeedClearInk)
			{
				ResetTexture();
				NS_NeedClearInk = false;
			}
		}

		public void CallBackIIUV(float u, float v)
		{
			PaintEarthUV(new Vector2(u, v));
		}

		public void CallBackIIClear()
		{
			NS_NeedClearInk = true;
		}

		public void CallBackIRUV(float u, float v)
		{
			NS_FocusLocation = Conversion.GetSpherePointFromUV(new Vector2(u, v));
			NS_FocusSet = true;
		}

		void FlyToSpokenCountry()
        {
            if (Input.GetKeyDown(KeyCode.Space)) FlyToCountry("China");
            Debug.Log("2");
            //Debug.Log(input_string);
            //if (input_string != "澳大利亚")
            //    Debug.Log("不等于");
            if (input_string.Contains("美国"))
            {     
                FlyToCountry("United States of America");
                Debug.Log("美国");
            }
            else if (input_string.Contains("中国"))
            {
                    FlyToCountry("China");
                    Debug.Log("中国");
            }
                else if (input_string.Contains( "澳大利亚")){
                    FlyToCountry("Australia");
                    Debug.Log("澳大利亚");
                    }
                else if (input_string.Contains( "俄罗斯")){
                    FlyToCountry("Russia");
                    Debug.Log("俄罗斯");
                    }
				else if (input_string.Contains( "丹麦")){
                    FlyToCountry("Denmark");
                    Debug.Log("丹麦");
                    }
				else if (input_string.Contains( "瑞典")){
                    FlyToCountry("Sweden");
                    Debug.Log("瑞典");
                    }
				else if (input_string.Contains( "瑞士")){
                    FlyToCountry("Switzerland");
                    Debug.Log("瑞士");
                    }
                else if (input_string.Contains( "英国")){
                    FlyToCountry("United Kingdom");
                    Debug.Log("英国");
                    }
                else if (input_string.Contains( "加拿大")){
                    FlyToCountry("Canada");
                    Debug.Log("加拿大");
                    }
                else if (input_string.Contains( "阿根廷")){
                    FlyToCountry("Argentina");
                    Debug.Log("阿根廷");
                    }
                else if (input_string.Contains( "墨西哥")){
                    FlyToCountry("Mexico");
                    Debug.Log("墨西哥");
                    }
                else if (input_string.Contains( "西班牙")){
                    FlyToCountry("Spain");
                    Debug.Log("西班牙");
                    }
                else if (input_string.Contains( "哥伦比亚")){
                    FlyToCountry("Colombia");
                    Debug.Log("哥伦比亚");
                    }
                else if (input_string.Contains( "委内瑞拉")){
                    FlyToCountry("Venezuela");
                    Debug.Log("委内瑞拉");
                    }
                else if (input_string.Contains( "日本")){
                    FlyToCountry("Japan");
                    Debug.Log("日本");
                    }
                else if (input_string.Contains( "印度")){
                    FlyToCountry("India");
                    Debug.Log("印度");
                    }
                else if (input_string.Contains( "伊朗")){
                    FlyToCountry("Iran");
                    Debug.Log("伊朗");
                    }
                else if (input_string.Contains( "沙特阿拉伯")){
                    FlyToCountry("Saudi Arabia");
                    Debug.Log("沙特阿拉伯");
                    }
                else if (input_string.Contains( "南非")){
                    FlyToCountry("South Africa");
                    Debug.Log("南非");
                    }
                else if (input_string.Contains( "土耳其")){
                    FlyToCountry("Turkey");
                    Debug.Log("土耳其");
                    }
                //else if (input_string.Contains( "肯尼亚")){
                //    FlyToCountry("Kenya");
                //    Debug.Log("肯尼亚");
                //    }
                else if (input_string.Contains( "苏丹")){
                    FlyToCountry("Sudan");
                    Debug.Log("苏丹");
                    }
                else if (input_string.Contains( "尼日利亚")){
                    FlyToCountry("Nigeria");
                    Debug.Log("尼日利亚");
                    }
                //else if (input_string.Contains( "安哥拉")){
                //    FlyToCountry("Angola");
                //    Debug.Log("安哥拉");
                //    }
                else if (input_string.Contains( "智利")){
                    FlyToCountry("Chile");
                    Debug.Log("智利");
                    }
                else if (input_string.Contains( "韩国")){
                    FlyToCountry("South Korea");
                    Debug.Log("韩国");
                    }
                else if (input_string.Contains( "朝鲜")){
                    FlyToCountry("North Korea");
                    Debug.Log("朝鲜");
                    }
                else if (input_string.Contains( "哈萨克斯坦")){
                    FlyToCountry("Kazakhstan");
                    Debug.Log("哈萨克斯坦");
                    }
                else if (input_string.Contains( "乌克兰")){
                    FlyToCountry("Ukraine");
                    Debug.Log("乌克兰");
                    }
                else if (input_string.Contains( "葡萄牙")){
                    FlyToCountry("Portugal");
                    Debug.Log("葡萄牙");
                    }
                else if (input_string.Contains( "蒙古")){
                    FlyToCountry("Mongolia");
                    Debug.Log("蒙古");
                    }
                else if (input_string.Contains( "巴基斯坦")){
                    FlyToCountry("Pakistan");
                    Debug.Log("巴基斯坦");
                    }
                //else if (input_string.Contains( "赞比亚")){
                //    FlyToCountry("Zambia");
                //    Debug.Log("赞比亚");
                //    }
                else if (input_string.Contains( "秘鲁")){
                    FlyToCountry("Peru");
                    Debug.Log("秘鲁");
                    }
                else if (input_string.Contains( "缅甸")){
                    FlyToCountry("Myanmar");
                    Debug.Log("缅甸");
                    }
                else if (input_string.Contains( "泰国")){
                    FlyToCountry("Thailand");
                    Debug.Log("泰国");
                    }
                else if (input_string.Contains( "柬埔寨")){
                    FlyToCountry("Cambodia");
                    Debug.Log("柬埔寨");
                    }
                else if (input_string.Contains( "越南")){
                    FlyToCountry("Vietnam");
                    Debug.Log("越南");
                    }
                else if (input_string.Contains( "阿富汗")){
                    FlyToCountry("Afghanstan");
                    Debug.Log("阿富汗");
                    }
                //else if (input_string.Contains( "毛里塔尼亚")){
                //    FlyToCountry("Mauritania");
                //    Debug.Log("毛里塔尼亚");
                //    }
                else if (input_string.Contains( "比利时")){
                    FlyToCountry("Belgium");
                    Debug.Log("比利时");
                    }
                else if (input_string.Contains( "德国")){
                    FlyToCountry("Germany");
                    Debug.Log("德国");
                    }
                else if (input_string.Contains( "塞尔维亚")){
                    FlyToCountry("Republic of Serbia");
                    Debug.Log("塞尔维亚");
                    }
                else if (input_string.Contains( "波兰")){
                    FlyToCountry("Poland");
                    Debug.Log("波兰");
                    }
                else if (input_string.Contains( "埃及")){
                    FlyToCountry("Egypt");
                    Debug.Log("埃及");
                    }
                else if (input_string.Contains( "希腊")){
                    FlyToCountry("Greece");
                    Debug.Log("希腊");
                    }
                else if (input_string.Contains( "孟加拉")){
                    FlyToCountry("Bangladesh");
                    Debug.Log("孟加拉");
                    }
                else if (input_string.Contains( "伊拉克")){
                    FlyToCountry("Iraq");
                    Debug.Log("伊拉克");
                    }
                else if (input_string.Contains( "乌拉圭")){
                    FlyToCountry("Uruguay");
                    Debug.Log("乌拉圭");
                    }

                else if (input_string.Contains( "巴西")){
                    FlyToCountry("Brazil");
                    Debug.Log("巴西");
                    }
                else if (input_string.Contains( "法国")){
                    FlyToCountry("France");
                    Debug.Log("法国");
                    }
        }

		string EntityListToString<T> (List<T>entities) {
			StringBuilder sb = new StringBuilder ("Neighbours: ");
			for (int k=0; k<entities.Count; k++) {
				if (k > 0) {
					sb.Append (", ");
				}
				sb.Append (((IAdminEntity)entities [k]).name);
			}
			return sb.ToString ();
		}


		// Sample code to show how to:
		// 1.- Navigate and center a country in the map
		// 2.- Add a blink effect to one country (can be used on any number of countries)
		void FlyToCountry (string countryName) {
			int countryIndex = map.GetCountryIndex (countryName);
			map.FlyToCountry (countryIndex,1.5f);
			map.BlinkCountry (countryIndex, Color.black, Color.green, 3, 0.2f);
		}

		// Sample code to show how to navigate to a city:
		void FlyToCity (string cityName) {
			map.FlyToCity (cityName);
		}


		// Sample code to show how tickers work
		void TickerSample () {
			map.ticker.ResetTickerBands ();

			// Configure 1st ticker band: a red band in the northern hemisphere
			TickerBand tickerBand = map.ticker.tickerBands [0];
			tickerBand.verticalOffset = 0.2f;
			tickerBand.backgroundColor = new Color (1, 0, 0, 0.9f);
			tickerBand.scrollSpeed = 0;	// static band
			tickerBand.visible = true;
			tickerBand.autoHide = true;

			// Prepare a static, blinking, text for the red band
			TickerText tickerText = new TickerText (0, "WARNING!!");
			tickerText.textColor = Color.yellow;
			tickerText.blinkInterval = 0.2f;
			tickerText.horizontalOffset = 0.1f;
			tickerText.duration = 10.0f;

			// Draw it!
			map.ticker.AddTickerText (tickerText);

			// Configure second ticker band (below the red band)
			tickerBand = map.ticker.tickerBands [1];
			tickerBand.verticalOffset = 0.1f;
			tickerBand.verticalSize = 0.05f;
			tickerBand.backgroundColor = new Color (0, 0, 1, 0.9f);
			tickerBand.visible = true;
			tickerBand.autoHide = true;

			// Prepare a ticker text
			tickerText = new TickerText (1, "INCOMING MISSILE!!");
			tickerText.textColor = Color.white;

			// Draw it!
			map.ticker.AddTickerText (tickerText);
		}

		// Sample code to show how to use decorators to assign a texsture
		void TextureSample () {
			// 1st way (best): assign a flag texture to USA using direct API - this texture will get cleared when you call HideCountrySurfaces()
			Texture2D texture = Resources.Load<Texture2D> ("flagUSA");
			int countryIndex = map.GetCountryIndex("United States of America");
			map.ToggleCountrySurface(countryIndex, true, Color.white, texture);
			
			// 2nd way: assign a flag texture to Brazil using decorator - the texture will stay when you call HideCountrySurfaces()
			string countryName = "Brazil";
			CountryDecorator decorator = new CountryDecorator ();
			decorator.isColorized = true;
			decorator.texture = Resources.Load<Texture2D> ("flagBrazil");
			decorator.textureOffset = Misc.Vector2down * 2.4f;
			map.decorator.SetCountryDecorator (0, countryName, decorator);
			
			Debug.Log ("USA flag added with direct API.");
			Debug.Log ("Brazil flag added with decorator (persistent texture).");

			map.FlyToCountry("Panama", 2f);
		}
		



		// The globe can be moved and scaled at wish
		void ToggleMinimize () {
			minimizeState = !minimizeState;

			Camera.main.transform.position = Vector3.back * 1.1f;
			Camera.main.transform.rotation = Quaternion.Euler (Misc.Vector3zero);
			if (minimizeState) {
				map.gameObject.transform.localScale = Misc.Vector3one * 0.20f;
				map.gameObject.transform.localPosition = new Vector3 (0.0f, -0.5f, 0);
				map.allowUserZoom = false;
				map.earthStyle = EARTH_STYLE.Alternate2;
				map.earthColor = Color.black;
				map.longitudeStepping = 4;
				map.latitudeStepping = 40;
				map.showFrontiers = false;
				map.showCities = false;
				map.showCountryNames = false;
				map.gridLinesColor = new Color (0.06f, 0.23f, 0.398f);
			} else {
				map.gameObject.transform.localScale = Misc.Vector3one;
				map.gameObject.transform.localPosition = Misc.Vector3zero;
				map.allowUserZoom = true;
				map.earthStyle = EARTH_STYLE.Natural;
				map.longitudeStepping = 15;
				map.latitudeStepping = 15;
				map.showFrontiers = true;
				map.showCities = true;
				map.showCountryNames = true;
				map.gridLinesColor = new Color (0.16f, 0.33f, 0.498f);
			}
		}


		/// <summary>
		/// Illustrates how to add custom markers over the globe using the AddMarker API.
		/// In this example a building prefab is added to a random city (see comments for other options).
		/// </summary>
		void AddMarkerGameObjectOnRandomCity () {

			// Every marker is put on a spherical-coordinate (assuming a radius = 0.5 and relative center at zero position)
			Vector3 sphereLocation;

			// Add a marker on a random city
			City city = map.cities [UnityEngine.Random.Range (0, map.cities.Count)];
			sphereLocation = city.unitySphereLocation;

			// or... choose a city by its name:
//		int cityIndex = map.GetCityIndex("Moscow");
//		sphereLocation = map.cities[cityIndex].unitySphereLocation;

			// or... use the centroid of a country
//		int countryIndex = map.GetCountryIndex("Greece");
//		sphereLocation = map.countries[countryIndex].center;

			// or... use a custom location lat/lon. Example put the building over New York:
//		map.calc.fromLatDec = 40.71f;	// 40.71 decimal degrees north
//		map.calc.fromLonDec = -74.00f;	// 74.00 decimal degrees to the west
//		map.calc.fromUnit = UNIT_TYPE.DecimalDegrees;
//		map.calc.Convert();
//		sphereLocation = map.calc.toSphereLocation;

			// Send the prefab to the AddMarker API setting a scale of 0.02f (this depends on your marker scales)
			GameObject building = Instantiate (Resources.Load<GameObject> ("Building/Building"));

			map.AddMarker (building, sphereLocation, 0.02f);


			// Fly to the destination and see the building created
			map.FlyToLocation (sphereLocation);

			// Optionally add a blinking effect to the marker
			MarkerBlinker.AddTo (building, 4, 0.2f);
		}

		void AddMarkerCircleOnRandomPosition () {
			// Draw a beveled circle
			Vector3 sphereLocation = UnityEngine.Random.onUnitSphere * 0.5f;
			float km = UnityEngine.Random.value * 500 + 500; // Circle with a radius of (500...1000) km

//			sphereLocation = map.cities[map.GetCityIndex("Paris")].unitySphereLocation;
//			km = 1053;
//			sphereLocation = map.cities[map.GetCityIndex("New York")].unitySphereLocation;
//			km = 500;
			map.AddMarker (MARKER_TYPE.CIRCLE, sphereLocation, km, 0.975f, 1.0f, new Color (0.85f, 0.45f, 0.85f, 0.9f));
			map.AddMarker (MARKER_TYPE.CIRCLE, sphereLocation, km, 0, 0.975f, new Color (0.5f, 0, 0.5f, 0.9f));
			map.FlyToLocation (sphereLocation);
		}


		/// <summary>
		/// Example of how to add custom lines to the map
		/// Similar to the AddMarker functionality, you need two spherical coordinates and then call AddLine
		/// </summary>
		void AddTrajectories (int numberOfLines) {

			// In this example we will add random lines from a group of cities to another cities (see AddMaker example above for other options to get locations)
			for (int line=0; line<numberOfLines; line++) {
				// Get two random cities
				int city1 = UnityEngine.Random.Range (0, map.cities.Count);
				int city2 = UnityEngine.Random.Range (0, map.cities.Count);

				// Get their sphere-coordinates
				Vector3 start = map.cities [city1].unitySphereLocation;
				Vector3 end = map.cities [city2].unitySphereLocation;

				// Add lines with random color, speeds and elevation
				Color color = new Color (UnityEngine.Random.Range (0.5f, 1), UnityEngine.Random.Range (0.5f, 1), UnityEngine.Random.Range (0.5f, 1));
				float elevation = UnityEngine.Random.Range (0, 0.5f); 	// elevation is % relative to the Earth radius
				float drawingDuration = 4.0f;
				float lineWidth = 0.0025f;
				float fadeAfter = 2.0f; // line stays for 2 seconds, then fades out - set this to zero to avoid line removal
				map.AddLine (start, end, color, elevation, drawingDuration, lineWidth, fadeAfter);
			}
		}



		/// <summary>
		/// Mount points are special locations on the map defined by user in the Map Editor.
		/// </summary>
		void LocateMountPoint() {
			int mountPointsCount = map.mountPoints.Count;
		    Debug.Log ("There're " + map.mountPoints.Count + " mount point(s). You can define more mount points using the Map Editor. Mount points are stored in mountPoints.txt file inside Resources/Geodata folder.");
			if (mountPointsCount>0) {
				Debug.Log ("Locating random mount point...");
				int mp = UnityEngine.Random.Range(0, mountPointsCount-1);
				Vector3 location = map.mountPoints[mp].unitySphereLocation;
				map.FlyToLocation(location);
			}
		}


		#region Bullet shooting!

		/// <summary>
		/// Creates a simple gameobject (sphere) on current map position and launch it over a random position on the globe following an arc
		/// </summary>
		void FireBullet() {

			// Create a "bullet" with a simple sphere at current map position from camera perspective
			GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphere.GetComponent<Renderer>().material.color = Color.yellow;

			// Choose starting pos
			Vector3 startPos = map.GetCurrentMapLocation();

			// Get a random target city
			int randomCity = UnityEngine.Random.Range(0, map.cities.Count);
			Vector3 endPos = map.cities[randomCity].unitySphereLocation;
			
			// Fire the bullet!
			StartCoroutine(AnimateBullet (sphere, 0.01f, startPos, endPos));
		}


		IEnumerator AnimateBullet(GameObject sphere, float scale, Vector3 startPos, Vector3 endPos, float duration = 3f, float arc = 0.25f) {

			// Optional: Draw the trajectory
			map.AddLine(startPos, endPos, Color.red, arc, duration, 0.002f, 0.1f);

			// Optional: Follow the bullet
			map.FlyToLocation(endPos, duration);

			// Animate loop for moving bullet over time
			float bulletFireTime = Time.time;
			float elapsed = Time.time - bulletFireTime;
			while (elapsed < duration) {
				float t = elapsed / duration;
				Vector3 pos = Vector3.Lerp(startPos, endPos, t).normalized * 0.5f;
				float altitude = Mathf.Sin (t * Mathf.PI) * arc / scale;
				map.AddMarker (sphere, pos, scale, true, altitude);
				yield return new WaitForFixedUpdate();
				elapsed = Time.time - bulletFireTime;
			}

			Destroy (sphere);

		}


		#endregion

	}

}

