using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

//==========================================================================
// Process the webcam flux to extract the number of human faces on screen
//==========================================================================
public class FaceRecognitionTest : MonoBehaviour {

	[SerializeField] private int MIN_FACE_SIZE = 25;
	[SerializeField] private int MAX_FACE_SIZE = 300;
	[SerializeField] private int processing_WIDTH = 640;
	[SerializeField] private int processing_HEIGHT = 480;
	[SerializeField] private int display_WIDTH = 60;
	[SerializeField] private int display_HEIGHT = 45;
	[SerializeField] private RawImage displayRawImage;
	[SerializeField] private Text faceText;

	// path the haarcascade file
	private string pathToFrontFaceClassifier = "C:/Users/Manu/Documents/OpenCV2/Assets/_DATA/haarcascades/haarcascade_frontalface_default.xml";

	Mat webcamFrame, webcamFrameGray, imgDisplay;
	VideoCapture webcam;
	Image<Hsv,Byte> imgTemp;
	private int numberOfFaces = 0;
	private CascadeClassifier _frontFacesCascadeClassifier;
	private Rectangle[] _frontFaces;
	private Texture2D displayTexture;

	//==========================================================================
	void Start () {

		displayTexture = new Texture2D(display_WIDTH,display_HEIGHT,TextureFormat.RGBA32,false);
		displayTexture.filterMode = FilterMode.Point;

		_frontFacesCascadeClassifier = new CascadeClassifier (pathToFrontFaceClassifier);

		webcam = new VideoCapture(0);

		webcamFrame = webcam.QueryFrame();
		webcamFrameGray = webcam.QueryFrame ();
		imgDisplay = webcamFrameGray;

		// use event handler to get webcam flux
		webcam.ImageGrabbed += new EventHandler (HandleWebcamFrame);
		webcam.Start ();

	}

	//==========================================================================
	void Update () {

		// update the rawImage texture with a processed image
		if (webcamFrame != null) {
			displayRawImage.texture = convertFromMatToTex2D (imgDisplay.Clone(), displayTexture);

		}

		// update text to show current number of faces
		faceText.text = "Number of faces : " + numberOfFaces;

		// user input (exit game)
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
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

	// retrieve a grabbed webcam frame
	// process the frame to extract the number of faces
	//==========================================================================
	void HandleWebcamFrame(object sender, EventArgs e){

		if (webcam != null && webcam.IsOpened) {
			webcam.Retrieve (webcamFrame);
			if (webcamFrame.IsEmpty)
				return;

			// resize, convert to grayscale, median filter
			CvInvoke.Resize (webcamFrame, webcamFrame, new Size (processing_WIDTH, processing_HEIGHT));
			CvInvoke.CvtColor (webcamFrame, webcamFrameGray, ColorConversion.Bgr2Gray);
			CvInvoke.MedianBlur (webcamFrameGray, webcamFrameGray, 3);

			// extract number of faces
			_frontFaces = _frontFacesCascadeClassifier.DetectMultiScale (
				image: webcamFrameGray,
				scaleFactor: 1.1,
				minNeighbors: 5,
				minSize: new Size (MIN_FACE_SIZE, MIN_FACE_SIZE),
				maxSize: new Size (MAX_FACE_SIZE, MAX_FACE_SIZE));
			
			numberOfFaces =  _frontFaces.Length;

			// draw rectangle around each detected face
			for (int i = 0; i < _frontFaces.Length; i++) {
				CvInvoke.Rectangle (webcamFrame, _frontFaces [i], new MCvScalar (0, 255, 0), 5);
			}
			imgDisplay = webcamFrame.Clone ();
		}
	}

	//==========================================================================
	private void OnDestroy(){
		webcam.Stop ();
		CvInvoke.DestroyAllWindows ();
	}

}


