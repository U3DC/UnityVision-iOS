﻿using System.Linq;
using System.Runtime.CompilerServices;
using Possible.Vision;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This example shows how to perform real-time image classification using the video feed from the rear camera. 
/// </summary>
public class Example : MonoBehaviour
{
	[SerializeField] private Vision _vision;
	[SerializeField] private RawImage _image;
	[SerializeField] private Text _text;
	
	// We use Unity's WebCamTexture API to access image data from device camera.
	private WebCamTexture _webCamTexture;

	private void Awake()
	{
		// Allocate webcam texture
		_webCamTexture = new WebCamTexture(requestedWidth: 1280, requestedHeight: 720);
		
		// We'll display the camera image on a full screen texture overlay
		_image.texture = _webCamTexture;
		
		// We need to tell the Vision plugin what kind of requests do we want it to perform.
		// This call not only prepares the managed wrapper for the specified image requests,
		// but allocates VisionRequest objects on the native side. You only need to call this
		// method when you initialize your application, and later if you need to change the type
		// of request you want to perform.
		// When performing image image classification requests,
		// maxObservations refers to the number of guesses extracted, ordered by confidence.
		_vision.SetAndAllocateRequests(VisionRequest.Classification, maxObservations: 1);
	}

	private void OnEnable()
	{
		_vision.OnObjectClassified += Vision_OnObjectClassified;
	}

	private void OnDisable()
	{
		_vision.OnObjectClassified -= Vision_OnObjectClassified;
	}

	private void Start()
	{
		// Start capturing
		_webCamTexture.Play();
	}

#if !UNITY_EDITOR && UNITY_IOS
	private void Update()
	{
		// We only classify a new image if no other vision requests are in progress
		if (!_vision.InProgress)
		{
			// This is the call where we pass in the handle to the image data to be analysed
			_vision.EvaluateBuffer(
				// This argument is always of type IntPtr, that refers to the data buffer
				buffer: _webCamTexture.GetNativeTexturePtr(), 
				// We need to tell the plugin the nature of the underlying data.
				// The plugin only supports CVPixelBuffer (CoreVideo) and MTLTexture (Metal).
				// Unity's Texture and all of its derived types return MTLTextureRef
				// when using Metal graphics API on iOS. OpenGLES 2 is not supported.
				// For more information refer to the official API documentation:
				// https://docs.unity3d.com/ScriptReference/Texture.GetNativeTexturePtr.html
				dataType: ImageDataType.MetalTexture);
		}
	}
#endif
	
	private void Vision_OnObjectClassified(object sender, ClassificationResultArgs e)
	{
		// Display the top guess for the dominant object on the image
		_text.text = e.observations.First().identifier;
	}
}