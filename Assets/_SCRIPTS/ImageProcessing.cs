using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

//==========================================================================
// Process the webcam flux to extract the position of an object held by the player
// The object must have an uniform color and high saturation
// Object color can be calibrated in the game menu with a slider
//==========================================================================
public class ImageProcessing : MonoBehaviour {

	[SerializeField] private RawImage displayRawImage;
	[SerializeField] private Slider HueSlider;
	[Header("Filter parameters")]
	[SerializeField]  private float lowerHue = 0;
	[SerializeField]  private float higherHue = 10;
	[SerializeField]  private float lowerSat = 50;
	[SerializeField]  private float higherSat = 255;
	[SerializeField]  private float lowerValue = 100;
	[SerializeField]  private float higherValue = 200;
	[Header("Texture Size")]
	[SerializeField]  private int processing_WIDTH = 640;
	[SerializeField]  private int processing_HEIGHT = 480;
	[SerializeField]  private int display_WIDTH = 60;
	[SerializeField]  private int display_HEIGHT = 45;

	private Vector2 objectPosition = Vector2.zero;
	private VideoCapture webcam;
	private VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
	private VectorOfPoint biggestContour = new VectorOfPoint();
	private const int operationSize = 2;
	private Mat structuringElement;
	private Mat hierarchy = new Mat();
	private Mat imgInput, imgBackup, imgDisplay;
	private Hsv lower, upper;
	private Image<Hsv,Byte> imgTempHsv;
	private Image<Gray,Byte> imgTempGray;
	private Image<Bgr,Byte> imgTempBgr;
	private MCvMoments moments;
	private Texture2D displayTexture;

	// Use this for initialization
	//==========================================================================
	void Start () {

		// initialize objects

		webcam = new VideoCapture(0);
		displayTexture = new Texture2D(display_WIDTH,display_HEIGHT,TextureFormat.RGBA32,false);
		displayTexture.filterMode = FilterMode.Point;
		imgInput = webcam.QueryFrame ();
		imgBackup = imgInput;
		imgDisplay = imgInput;
		imgDisplay = CellShading (imgDisplay);
		structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle,new Size(2*operationSize+1,2*operationSize+1),new Point(operationSize,operationSize));

		// listener for the value slider
		HueSlider.onValueChanged.AddListener (delegate {
			ChangeFilterHue ();
		});
		HueSlider.value = 0.5f*(lowerHue + higherHue);

		// setup filter range
		lower = new Hsv(lowerHue,lowerSat,lowerValue);
		upper = new Hsv(higherHue,higherSat,higherValue);

		// use event handler to get webcam flux
		webcam.ImageGrabbed += new EventHandler (HandleWebcamFrame);
		webcam.Start ();

	}
	
	// Update is called once per frame
	//==========================================================================
	void Update () {

		// update the rawImage texture with a processed image
		if (imgDisplay != null) {
			displayRawImage.texture = convertFromMatToTex2D (imgDisplay.Clone(), displayTexture);
		}
	}

	// retrieve a grabbed webcam frame
	//==========================================================================
	void HandleWebcamFrame(object sender, EventArgs e){

		if (webcam != null && webcam.IsOpened) {
			webcam.Retrieve (imgInput);
			if (imgInput.IsEmpty)
				return;
			
			ProcessFrame ();
		
		}
	}

	// process the frame to extract the position of the object held by the player
	//==========================================================================
	void ProcessFrame(){

		CvInvoke.Resize(imgInput, imgInput, new Size(processing_WIDTH,processing_HEIGHT));
		CvInvoke.Flip (imgInput, imgInput, FlipType.Horizontal);

		imgBackup = imgInput.Clone ();

		CvInvoke.CvtColor (imgInput, imgInput, ColorConversion.Bgr2Hsv);
		CvInvoke.MedianBlur (imgInput, imgInput, 3);

		imgTempHsv = imgInput.ToImage<Hsv,Byte>();

		// filtering according to the HSV value
		imgInput = (imgTempHsv.InRange (lower, upper)).Mat;

		// erosion and dilation
		CvInvoke.Erode (imgInput, imgInput, structuringElement, new Point (-1, -1), 5, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
		CvInvoke.Dilate(imgInput, imgInput, structuringElement, new Point (-1, -1), 5, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

		// find and process contours =====================

		int biggestContourIndex = -1;
		double biggestContourArea = 0;

		CvInvoke.FindContours (imgInput, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

		if (contours.Size > 0) {
			for (int i = 0; i < contours.Size; i++) {
				double a = CvInvoke.ContourArea (contours [i], false);
				if (a > biggestContourArea) {
					biggestContourArea = a;
					biggestContourIndex = i;
				}
			}

			biggestContour = contours [biggestContourIndex];

			// retrieve the position of the object in the player hand, using the centroid of the biggest contour
			// the position is normalized in camera space (from -1 to +1)
			moments = CvInvoke.Moments (biggestContour);
			objectPosition.x = (-1.0f + 2.0f * (float)(moments.M10 / moments.M00) / processing_WIDTH);
			objectPosition.y = (+1.0f - 2.0f * (float)(moments.M01 / moments.M00) / processing_HEIGHT);
		
			// apply cell shading effect (purely cosmetic)
			imgDisplay = CellShading (imgBackup);

			// draw contours on the displayed image
			CvInvoke.DrawContours (imgDisplay, contours, biggestContourIndex, new MCvScalar (0, 255, 0), 7);

		} else {
			// if we dont find a contour...
			// apply cell shading effect (purely cosmetic)
			imgDisplay = CellShading (imgBackup);
		}

	}

	// Mat conversion to texture2D for displaying in a RawImage
	//==========================================================================
	private Texture2D convertFromMatToTex2D(Mat matImage, Texture2D texture){

		// texture size must also be equal to display_WIDTH,display_HEIGHT !!
		CvInvoke.Resize(matImage, matImage, new Size(display_WIDTH,display_HEIGHT));
		CvInvoke.Flip (matImage, matImage, FlipType.Vertical);

		texture.LoadRawTextureData (matImage.ToImage<Rgba,Byte> ().Bytes);
		texture.Apply ();
	
		return texture;
	}

	// cellshading effect : return quantized color values
	//==========================================================================
	private Mat CellShading(Mat m1){
		
		imgTempGray = m1.ToImage<Gray,Byte>();
		imgTempBgr = m1.ToImage<Bgr,Byte>();

		for (int i = 0; i < imgTempGray.Height; i++) {
			for (int j = 0; j < imgTempGray.Width; j++) {
				
				if (imgTempGray.Data [i, j, 0] < 60) {
					imgTempBgr.Data [i, j, 0] = 0;
					imgTempBgr.Data [i, j, 1] = 0;
					imgTempBgr.Data [i, j, 2] = 0;
				} else if (imgTempGray.Data [i, j, 0] < 120) {
					imgTempBgr.Data [i, j, 0] = 75;
					imgTempBgr.Data [i, j, 1] = 25;
					imgTempBgr.Data [i, j, 2] = 50;
				} else if (imgTempGray.Data [i, j, 0] < 160) {
					imgTempBgr.Data [i, j, 0] = 50;
					imgTempBgr.Data [i, j, 1] = 100;
					imgTempBgr.Data [i, j, 2] = 50;
				} else {
					imgTempBgr.Data [i, j, 0] = 200;
					imgTempBgr.Data [i, j, 1] = 150;
					imgTempBgr.Data [i, j, 2] = 100;
				}
			}
		}

		return imgTempBgr.Mat;
	}

	//==========================================================================
	private void OnDestroy(){
		webcam.Stop ();
	}

	//==========================================================================
	public Vector2 GetObjectPosition(){

		return objectPosition;
	}

	//==========================================================================
	void ChangeFilterHue(){
		lower.Hue= HueSlider.value - 10;
		upper.Hue = HueSlider.value + 10;
		//ValueSlider.gameObject.GetComponentInChildren<Text> ().text = ValueSlider.value.ToString ();
		HueSlider.gameObject.GetComponentInChildren<UnityEngine.UI.Image> ().color = UnityEngine.Color.HSVToRGB ((HueSlider.value / 180.0f), 1.0f, 0.75f);
	}
}
